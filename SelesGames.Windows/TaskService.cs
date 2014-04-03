using Microsoft.Phone.Tasks;
using System;

namespace SelesGames.Phone
{
    public static class TaskService
    {
        public static void ToMarketplaceDetailTask(Action<Exception> onFail = null)
        {
            try
            {
                new MarketplaceDetailTask().Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToMarketplaceDetailTask(string appId, Action<Exception> onFail = null)
        {
            try
            {
                new MarketplaceDetailTask { ContentIdentifier = appId }.Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToMarketplaceReviewTask(Action<Exception> onFail = null)
        {
            try
            {
                new MarketplaceReviewTask().Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToMarketplaceAppSearchTask(string SearchTerms, Action<Exception> onFail = null)
        {
            try
            {
                new MarketplaceSearchTask 
                { 
                    ContentType = MarketplaceContentType.Applications,
                    SearchTerms = SearchTerms
                }.Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToEmailComposeTask(string To = null, string Subject = null, string Body = null, string Cc = null, Action<Exception> onFail = null)
        {
            try
            {
                new EmailComposeTask
                {
                    Subject = Subject,
                    Body = Body,
                    To = To,
                    Cc = Cc,
                }.Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToInternetExplorerTask(string url, Action<Exception> onFail = null)
        {
            try
            {
                new WebBrowserTask { URL = url }.Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToInternetExplorerTask(Uri uri, Action<Exception> onFail = null)
        {
            try
            {
                new WebBrowserTask { Uri = uri }.Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToShareLinkTask(string link, string title = null, string message = null, Action<Exception> onFail = null)
        {
            try
            {
                var uri = new Uri(link, UriKind.Absolute);
                new ShareLinkTask { LinkUri = uri, Message = message, Title = title }.Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }

        public static void ToSmsComposeTask(string body, Action<Exception> onFail = null)
        {
            try
            {
                new SmsComposeTask { Body = body }.Show();
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail(e);
            }
        }
    }
}
