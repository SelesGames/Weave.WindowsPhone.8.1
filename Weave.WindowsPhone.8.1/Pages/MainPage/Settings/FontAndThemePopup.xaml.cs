using SelesGames;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Weave.Customizability;
using Weave.SavedState;
using Weave.Settings;

namespace weave
{
    public partial class FontAndThemePopup : UserControl, IPopup<Unit>, IDisposable
    {
        PermanentState permanentState;
        FontSizes fontSizes;
        ArticleListFontSet fonts;
        ArticleListFormats formats;
        CompositeDisposable disposables = new CompositeDisposable();

        public event EventHandler<EventArgs<FontSizeProperties>> FontSizeChanged;
        public event EventHandler<EventArgs<FontProperties>> FontChanged;
        public event EventHandler<EventArgs<ArticleListFormatProperties>> ArticleListFormatChanged;

        public FontAndThemePopup()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            //themeName.SetBinding(TextBlock.TextProperty, new Binding("CurrentTheme.Name") { Source = AppSettings.Instance.Themes });

            fontSizes = Resources["FontSizes"] as FontSizes;
            fonts = Resources["Fonts"] as ArticleListFontSet;
            formats = Resources["ArticleListFormats"] as ArticleListFormats;

            permanentState = ServiceResolver.Get<PermanentState>();

            fontSizePicker.SelectedItem = fontSizes.GetById(permanentState.ArticleListFontSize);
            fontSelector.SelectedItem = fonts.GetByFontName(permanentState.ArticleListFontName);
            articleListFormatPicker.SelectedItem = formats.GetArticleListFormatByFormatType(permanentState.ArticleListFormat);
#if DEBUG
            grid.SizeChanged += new System.Windows.SizeChangedEventHandler(grid_SizeChanged);
#endif
            //var formatBinding = new Binding("ArticleListFormat") 
            //{ 
            //    Source = permanentState, 
            //    Converter = new DelegateValueConverter<ArticleListFormatType, ArticleListFormatProperties>(
            //        o => formats.GetArticleListFormatByFormatType(o), 
            //        o => o.FormatType)
            //};
            //articleListFormatPicker.SetBinding(RadListPicker.SelectedItemProperty, formatBinding);
        }

        void grid_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            DebugEx.WriteLine("new size {0}, old size {1}", e.NewSize, e.PreviousSize);
        }




        #region IPopup Methods

        public event EventHandler ShowCompleted;
        public event EventHandler HideCompleted;
        public event EventHandler<EventArgs<PopupResult<Unit>>> ResultCompleted;

        public void BeginShow()
        {
            OpenSB.BeginWithNotification().Take(1).Subscribe(_ => ShowCompleted.Raise(this)).DisposeWith(disposables);
        }

        public void BeginHide()
        {
            CloseSB.BeginWithNotification().Take(1).Subscribe(_ => HideCompleted.Raise(this)).DisposeWith(disposables);
        }

        #endregion




        void OnOutsideTap(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BeginHide();
            ResultCompleted.Raise(this, PopupResult.CreateUserDismissed<Unit>());
        }

        void themeButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppSettings.Instance.Themes.NextTheme();
        }

        void FontSizePicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var newItem = e.AddedItems.OfType<FontSizeProperties>().FirstOrDefault();
            if (newItem == null)
                return;

            permanentState.ArticleListFontSize = newItem.Id;
            FontSizeChanged.Raise(this, newItem);
        }

        void fontSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var newItem = e.AddedItems.OfType<FontProperties>().FirstOrDefault();
            if (newItem == null)
                return;

            permanentState.ArticleListFontName = newItem.FontName;
            FontChanged.Raise(this, newItem);
        }

        void ArticleListFontFormatPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var newItem = e.AddedItems.OfType<ArticleListFormatProperties>().FirstOrDefault();
            if (newItem == null)
                return;

            permanentState.ArticleListFormat = newItem.FormatType;
            ArticleListFormatChanged.Raise(this, newItem);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
