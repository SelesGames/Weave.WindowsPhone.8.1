using SelesGames;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Weave.Customizability;
using Weave.SavedState;
using Weave.Settings;

namespace weave
{
    public partial class FontSizePopup : UserControl, IPopup<Unit>, IDisposable
    {
        PermanentState permanentState;
        FontSizes fontSizes;
        ArticleFontSet fonts;
        CompositeDisposable disposables = new CompositeDisposable();

        public event EventHandler<EventArgs<FontSizeProperties>> FontSizeChanged;
        public event EventHandler<EventArgs<FontProperties>> FontChanged;

        public FontSizePopup()
        {
            InitializeComponent();
            themeName.SetBinding(TextBlock.TextProperty, new Binding("CurrentTheme.Name") { Source = AppSettings.Instance.Themes });
            permanentState = ServiceResolver.Get<PermanentState>();
            fontSizes = Resources["FontSizes"] as FontSizes;
            fonts = Resources["Fonts"] as ArticleFontSet;
            fontSizePicker.SelectedItem = fontSizes.GetById(permanentState.ArticleViewFontSize);
            fontSelector.SelectedItem = fonts.GetByFontName(permanentState.ArticleViewFontName);
#if DEBUG
            grid.SizeChanged += new System.Windows.SizeChangedEventHandler(grid_SizeChanged);
#endif
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

        void ListPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var newItem = e.AddedItems.OfType<FontSizeProperties>().FirstOrDefault();
            if (newItem == null)
                return;

            FontSizeChanged.Raise(this, newItem);
            permanentState.ArticleViewFontSize = newItem.Id;
        }

        void fontSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var newItem = e.AddedItems.OfType<FontProperties>().FirstOrDefault();
            if (newItem == null)
                return;

            FontChanged.Raise(this, newItem);
            permanentState.ArticleViewFontName = newItem.FontName;
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
