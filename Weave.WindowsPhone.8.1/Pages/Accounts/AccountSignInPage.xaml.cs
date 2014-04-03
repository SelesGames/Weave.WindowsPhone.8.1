using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;
using SelesGames;
using System;
using System.Threading.Tasks;
using System.Windows;
using Weave.ViewModels.Contracts.Client;
using Weave.ViewModels.Identity;

namespace weave.Pages.Accounts
{
    public partial class AccountSignInPage : PhoneApplicationPage
    {
        IdentityInfo viewModel;
        bool isViewModelInitialized = false;

        public AccountSignInPage()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            var userCache = ServiceResolver.Get<IUserCache>();
            var user = userCache.Get();
            viewModel = new IdentityInfo(new Weave.Identity.Service.Client.ServiceClient());
            viewModel.UserId = user.Id;
            DataContext = viewModel;
            viewModel.UserIdChanged += async (s, e) =>
            {
                user.Id = viewModel.UserId;
                // refresh user news?
                await user.Load(refreshNews: true);
            };
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (isViewModelInitialized)
                return;

            isViewModelInitialized = true;

            try
            {
                await viewModel.LoadFromUserId();
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        MobileServiceClient CreateMobileServiceClient()
        {
            return new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
        }

        async void OnFacebookButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                await viewModel.SyncFacebook(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        async void OnTwitterButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Twitter);
                await viewModel.SyncTwitter(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        async void OnMicrosoftButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                await viewModel.SyncMicrosoft(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        async void OnGoogleButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Google);
                await viewModel.SyncGoogle(mobileUser.UserId);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        //async void OnLoginButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    var username = userNameTB.Text;
        //    var password = passwordTB.Text;

        //    if (string.IsNullOrWhiteSpace(username))
        //    {
        //        MessageBox.Show("You must enter a username to sign-in");
        //        return;
        //    }
        //    if (string.IsNullOrWhiteSpace(password))
        //    {
        //        MessageBox.Show("You must enter a password to sign-in");
        //        return;
        //    }

        //    viewModel.UserName = username;
        //    viewModel.PasswordHash = password;

        //    try
        //    {
        //        await viewModel.LoadFromUsernameAndPassword();
        //    }
        //    catch (Exception ex)
        //    {
        //        DebugEx.WriteLine(ex);
        //        //message = "You must log in. Login Required";
        //    }
        //}

        //void OnCreateAccountButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    GlobalNavigationService.ToCreateAccountPage();  
        //}
    }
}