using Microsoft.Phone.Controls;
using SelesGames.Instapaper;
using System.Net;
using System.Net.Http;
using System.Windows;
using Weave.Services.Instapaper;

namespace weave
{
    public partial class InstapaperAccountCredentialsPage : PhoneApplicationPage
    {
        public InstapaperAccountCredentialsPage()
        {
            InitializeComponent();
        }

        async void saveButton_Click(object sender, System.EventArgs e)
        {
            var username = usernameField.Text;
            var password = passwordField.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Username is mandatory.");
                return;
            }

            var account = new InstapaperAccount 
            { 
                UserName = username, 
                Password = password 
            };
            string verifyUrl = account.CreateVerificationString();
            var client = new HttpClient();

            try
            {
                var temp = await client.GetAsync(verifyUrl);
                var response = temp as HttpResponseMessage;
                if (response == null) return;

                if (response.StatusCode == HttpStatusCode.Forbidden) // 403
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Invalid username or password."));

                else if (response.StatusCode == HttpStatusCode.InternalServerError) // 500
                    Dispatcher.BeginInvoke(() => MessageBox.Show("There was an error contacting Instapaper.  Please try again in a few minutes."));

                else if (response.StatusCode == HttpStatusCode.OK) // 200
                {
                    // TODO: add a progressbar or spinner here
                    await InstapaperAccount2.Current.SaveCredentials(account);
                    InstapaperService.FlushAnyPendingRequests();

                    Dispatcher.BeginInvoke(() =>
                    {
                        if (NavigationService.CanGoBack)
                            NavigationService.GoBack();
                    });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Whoops!  You either entered an invalid username/password combo, or Instapaper is down at the moment."));
            }
        }
    }
}