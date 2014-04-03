using Common.Microsoft;
using Microsoft.Live;
using Microsoft.Phone.Controls;
using SelesGames;
using System;
using System.Threading.Tasks;
using Weave.SavedState;

namespace weave
{
    public partial class OneNoteSignInPage : PhoneApplicationPage
    {
        public Action Callback { get; set; }

        public OneNoteSignInPage()
        {
            InitializeComponent();
        }

        async void OnSessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            var permState = ServiceResolver.Get<PermanentState>();

            switch (e.Status)
            {
                case LiveConnectSessionStatus.Connected:
                    permState.ThirdParty.LiveAccessToken = new LiveOfflineAccessToken(
                        clientId: signInButton.ClientId,
                        accessToken: e.Session.AccessToken,
                        accessTokenExpiration: e.Session.Expires,
                        refreshToken: e.Session.RefreshToken);

                    infoTextBlock.Text = "Authentication successful";
                    await Task.Delay(500);                
                    if (Callback != null)
                        Callback();
                    NavigationService.GoBack();
                    break;

                case LiveConnectSessionStatus.NotConnected:
                    infoTextBlock.Text = "Authentication failed.";
                    break;

                default:
                    infoTextBlock.Text = "Tap the button below to authenticate";
                    break;
            }
        }
    }
}