using Microsoft.Phone.Controls;
using SelesGames;
using SelesGames.HttpClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Weave.FeedLibrary;
using Weave.SavedState;
using Weave.Services;
using Weave.Settings;
using Weave.ViewModels;

namespace weave
{
    public partial class SelectTheCategoriesThatInterestYouPage : PhoneApplicationPage
    {
        public class Category
        {
            public string Name { get; set; }
            public bool IsEnabled { get; set; }
        }

        BundledLibrary library;
        IEnumerable<Category> categories;

        public SelectTheCategoriesThatInterestYouPage()
        {
            InitializeComponent();
            ApplicationTitle.Text = "welcome to " + AppSettings.Instance.AppName;
            warning.Visibility = Visibility.Collapsed;

            library = ServiceResolver.Get<BundledLibrary>();
            categories = library.Feeds.Value.UniqueCategoryNames().OrderBy(o => o).Select(o => new Category { Name = o }).ToList();

            var permState = ServiceResolver.Get<PermanentState>();
            if (permState.IsFirstTime)
            {
                foreach (var category in categories)
                {
                    category.IsEnabled = false;
                };
            }

            list.ItemsSource = (IList)categories;
            nextButton.Click += (s, e) => OnNextButtonClick();
        }

        async void OnNextButtonClick()
        {
            bool atLeast1Selected = CheckForAtLeast1();
            if (atLeast1Selected)
            {
                var enabledCategories = categories.Where(o => o.IsEnabled).Select(o => o.Name).ToList();
                var feedsToAdd = library.Feeds.Value.Where(o => enabledCategories.Contains(o.Category)).ToList();

                var frame = GlobalNavigationService.CurrentFrame;

                frame.OverlayText = "Creating user...";
                frame.IsLoading = true;

                var user = ServiceResolver.Get<UserInfo>();
                user.Feeds = new ObservableCollection<Feed>(feedsToAdd);
                try
                {
                    await user.Create();
                }
                //catch(ResponseException ex)
                //{
                //    if (ex.Response.ReasonPhrase != "A user with that Id already exists")
                //        throw;
                //}
                catch(ErrorResponseException ex)
                {
                    if (ex.ResponseMessage.ReasonPhrase != "A user with that Id already exists")
                        throw;
                }
                catch(Exception ex)
                {
                    DebugEx.WriteLine(ex);
                }
                await user.Load(refreshNews: true);

                frame.IsLoading = false;

                GlobalNavigationService.ToPanoramaPage();
            }
            else
                NotifyYouNeed1();
        }

        bool CheckForAtLeast1()
        {
            return categories.Where(o => o.IsEnabled).Count() >= 1;
        }
        
        void NotifyYouNeed1()
        {
            warning.Visibility = Visibility.Visible;
        }
    }
}