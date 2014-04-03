using Microsoft.Phone.Controls;
using Portable.Common;
using SelesGames.Phone;
using System;
using System.Linq;
using System.Windows.Navigation;

namespace weave
{
    public partial class OAuthPage : PhoneApplicationPage
    {
        string target, callbackUri, requestCode;

        public Action Callback { get; set; }

        public OAuthPage()
        {
            InitializeComponent();
            browser.IsScriptEnabled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            target = NavigationContext.QueryString["target"];

            var lookup = new Uri(target).ParseQueryString()
                .ToDictionary(o => o.Key, o => o.Value);

            callbackUri = lookup["redirect_uri"];
            requestCode = lookup["request_token"];

            browser.Navigate(new Uri(target));
            browser.Navigating += browser_Navigating;
        }

        void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            var url = e.Uri;
            System.Diagnostics.Debug.WriteLine(url);

            if (url.Equals(new Uri(callbackUri)))
            {
                e.Cancel = true;
                if (Callback != null)
                    Callback();

                NavigationService.TryGoBack();
            }
        }
    }
}