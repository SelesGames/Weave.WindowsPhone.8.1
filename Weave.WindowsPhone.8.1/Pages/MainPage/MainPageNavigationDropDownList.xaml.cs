using System;
using System.Windows.Controls;
using Weave.WP.ViewModels.GroupedNews;

namespace weave
{
    public partial class MainPageNavigationDropDownList : UserControl
    {
        public MainPageNavigationDropDownList()
        {
            InitializeComponent();
        }

        public event EventHandler<CategoryOrFeedEventArgs> ItemSelected;

        void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var vm  = (sender as Button).DataContext as NewsItemGroup;
            //OnCategorySelected(vm);
            if (ItemSelected != null && vm != null)
                ItemSelected(this, new CategoryOrFeedEventArgs(vm));
        }
    }

    public class CategoryOrFeedEventArgs : EventArgs
    {
        public NewsItemGroup Selected { get; private set; }

        public CategoryOrFeedEventArgs(NewsItemGroup selected)
        {
            Selected = selected;
        }
    }
}