using PocketSharp;
using System;
using System.Threading.Tasks;
using System.Windows;
using Weave.SavedState;
using Weave.ViewModels;

namespace Weave.Services.Pocket
{
    class PocketSavingWorkflow
    {
        const string CALLBACK_URI = "http://www.selesgames.com";

        readonly string consumerKey;
        readonly PermanentState permState;
        readonly NewsItem newsItem;

        public PocketSavingWorkflow(string consumerKey, PermanentState permState, NewsItem newsItem)
        {
            if (permState == null) throw new ArgumentNullException("permState");
            if (newsItem == null) throw new ArgumentNullException("newsItem");

            this.consumerKey = consumerKey;
            this.permState = permState;
            this.newsItem = newsItem;
        }

        internal Task Save()
        {
            return TrySave(continueOnSaveFailure: true);
        }

        async Task TrySave(bool continueOnSaveFailure)
        {
            try
            {
                if (continueOnSaveFailure)
                {
                    await TrySaveRegisterOnFail();
                }
                else
                {
                    await InnerSave();
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Unable to save to Pocket");
            }
        }

        async Task TrySaveRegisterOnFail()
        {
            try
            {
                await InnerSave();
                return;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }

            await Register();
        }

        async Task InnerSave()
        {
            var accessCode = permState.ThirdParty.PocketAccessCode;

            if (string.IsNullOrWhiteSpace(accessCode))
            {
                throw new Exception("access code not set");
            }

            var client = new PocketClient(
                consumerKey: consumerKey,
                accessCode: accessCode,
                isMobileClient: true);

            try
            {
                ToastService.ToastPrompt("Sending to Pocket...");

                var result = await client.Add(
                    uri: new Uri(newsItem.Link),
                    title: newsItem.Title,
                    tags: new[] { newsItem.Feed.Category, newsItem.Feed.Name });

                ToastService.ToastPrompt("Saved to Pocket!");
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                throw;
            }
        }

        async Task Register()
        {
            var client = new PocketClient(
                consumerKey: consumerKey,
                callbackUri: CALLBACK_URI,
                isMobileClient: true);

            var requestCode = await client.GetRequestCode();
            var authenticationUri = client.GenerateAuthenticationUri(requestCode);

            await GlobalDispatcher.Current.InvokeAsync(() =>
                GlobalNavigationService.ToOAuthPage(
                    authenticationUri.OriginalString,
                    async () => await OnCallback(requestCode)));
        }

        async Task OnCallback(string requestCode)
        {
            bool isRegSuccessful = false;

            var client = new PocketClient(consumerKey: consumerKey);

            try
            {
                var response = await client.GetUser(requestCode: requestCode);
                permState.ThirdParty.PocketAccessCode = response.Code;
                isRegSuccessful = true;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                isRegSuccessful = false;
            }

            if (isRegSuccessful)
                await TrySave(continueOnSaveFailure: false);
        }
    }
}