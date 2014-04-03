using SelesGames.Instapaper;
using System;
using System.Windows;
using Weave.ViewModels;

namespace Weave.Services.Instapaper
{
    public static class InstapaperService
    {
        static NewsItem pendingRequest;

        public static async void SendToInstapaper(NewsItem newsItem)
        {
            InstapaperAccount account = null;

            try
            {
                account = await InstapaperAccount2.Current.GetCredentials();
            }
            catch { }

            if (account == null)
            {
                GlobalNavigationService.ToInstapaperAccountCredentialsPage();
                pendingRequest = newsItem;
                return;
            }


            var dispatcher = GlobalDispatcher.Current;

            try
            {
                var ip = await account.SendToInstapaper(newsItem.Link, newsItem.Title, string.Empty);

                if (ip.ResultType == InstapaperResultType.InvalidCredentials)
                    dispatcher.BeginInvoke(() =>
                    {
                        var result = MessageBox.Show("Invalid username or password.  Press \"OK\" to reset your Instapaper username or password.", "Invalid Login", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                            GlobalNavigationService.ToInstapaperAccountCredentialsPage();
                    });

                else if (ip.ResultType == InstapaperResultType.BadRequest)
                    dispatcher.BeginInvoke(() => MessageBox.Show("Bad request - the news article may have a malformed link."));

                else if (ip.ResultType == InstapaperResultType.ErrorContactingInstapaper)
                    dispatcher.BeginInvoke(() => MessageBox.Show("There was an error contacting Instapaper.  Please try again in a few minutes."));

                else if (ip.ResultType == InstapaperResultType.Created)
                    dispatcher.BeginInvoke(() => ToastService.ToastPrompt("Sent to Instapaper!"));
            }
            catch(Exception e)
            {
                DebugEx.WriteLine("INSTAPAPER ERROR:\r\n", e);
                dispatcher.BeginInvoke(() =>
                {
                    var result = MessageBox.Show("There was an error contacting Instapaper.  Have you changed your username or password recently?  Press \"OK\" to reset your Instapaper username or password.", string.Empty, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                        GlobalNavigationService.ToInstapaperAccountCredentialsPage();
                });
            }
        }

        public static void FlushAnyPendingRequests()
        {
            if (pendingRequest != null)
                SendToInstapaper(pendingRequest);
        }
    }
}
