using SelesGames;
using SelesGames.Phone;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace weave
{
    public partial class SocialShareContextMenuControl : UserControl, IPopup<string>, IDisposable
    {
        CompositeDisposable disposables = new CompositeDisposable();
        Control[] buttons;
        Mode currentMode;

        enum Mode
        {
            Vertical,
            Horizontal
        }


        public SocialShareContextMenuControl()
        {
            InitializeComponent();

            buttons = new[] { instapaperButton, socialShareButton, ieButton, smsButton, emailButton, pocketButton, oneNoteButton };

            var topRow = new[] { instapaperButton, pocketButton, oneNoteButton };
            var bottomRow = new[] { socialShareButton, ieButton, smsButton, emailButton };

            var accent = (Color)Resources["PhoneAccentColor"];
            var palette = accent.GetComplementary();

            var topRowBrush = new SolidColorBrush(palette.ComplementaryColorDarker);
            var bottomRowBrush = new SolidColorBrush(accent);

            //foreach (var button in buttons)
            //{
            //    SelesGames.
            //    button.SetBinding(TileButton.BackgroundProperty, new System.Windows.Data.Binding("Background") { Source = this });
            //}
            foreach (var button in topRow)
                button.Background = topRowBrush;

            foreach (var button in bottomRow)
                button.Background = bottomRowBrush;

            var now = DateTimeOffset.UtcNow;

#if DEBUG
            now = new DateTime(2014, 3, 17, 7, 1, 0);
#endif

            if (now < new DateTimeOffset(2014, 3, 17, 14, 0, 0, TimeSpan.Zero))
                oneNoteButton.Visibility = Visibility.Collapsed;
        }

        internal void SetVerticalMode()
        {
            currentMode = Mode.Vertical;
            foreach (var button in buttons)
            {
                ((CompositeTransform)button.RenderTransform).TranslateX = 0d;
                ((CompositeTransform)button.RenderTransform).TranslateY = 400d;
            }
        }

        internal void SetHorizontalMode()
        {
            currentMode = Mode.Horizontal;
            foreach (var button in buttons)
            {
                ((CompositeTransform)button.RenderTransform).TranslateX = 240d;
                ((CompositeTransform)button.RenderTransform).TranslateY = 0d;
            }
        }

        //internal void HideCloseButtonForAppBarSetup()
        //{
        //    grid.Margin = new Thickness(0, 0, 0, 48);
        //}




        #region IPopup Methods

        public event EventHandler ShowCompleted;
        public event EventHandler HideCompleted;
        public event EventHandler<EventArgs<PopupResult<string>>> ResultCompleted;

        public void BeginShow()
        {
            disposables.Clear();

            foreach (var button in buttons)
            {
                button.IsEnabled = false;
            }

            StopAllAnimations();

            if (currentMode == Mode.Vertical)
                VerticalOpenSB.BeginWithNotification().Take(1).Subscribe(_ => ShowCompleted.Raise(this)).DisposeWith(disposables);
            else if (currentMode == Mode.Horizontal)
                HorizontalOpenSB.BeginWithNotification().Take(1).Subscribe(_ => ShowCompleted.Raise(this)).DisposeWith(disposables);

            Observable.Timer(0.2d.Seconds()).Take(1).ObserveOnDispatcher().Subscribe(ListenToButtonClicks)
                .DisposeWith(disposables);
        }

        void StopAllAnimations()
        {
            VerticalCloseSB.Stop();
            VerticalOpenSB.Stop();
            HorizontalCloseSB.Stop();
            HorizontalOpenSB.Stop();
        }

        public void BeginHide()
        {
            disposables.Clear();
            if (currentMode == Mode.Vertical)
                VerticalCloseSB.BeginWithNotification().Take(1).Subscribe(_ => HideCompleted.Raise(this)).DisposeWith(disposables);
            else if (currentMode == Mode.Horizontal)
                HorizontalCloseSB.BeginWithNotification().Take(1).Subscribe(_ => HideCompleted.Raise(this)).DisposeWith(disposables);
        }

        #endregion




        #region Button Handling

        void ListenToButtonClicks()
        {
            buttons
                .Select(button => button.GetTap().Select(o => button))
                .Merge()
                .Subscribe(HandleButtonPress)
                .DisposeWith(disposables);

            foreach (var button in buttons)
            {
                button.IsEnabled = true;
            }
        }

        void HandleButtonPress(UIElement button)
        {
            if (button == null)
                return;

            if (button == instapaperButton)
                RaiseResultCompleted("instapaper");

            else if (button == smsButton)
                RaiseResultCompleted("sms");

            else if (button == ieButton)
                RaiseResultCompleted("ie");

            else if (button == socialShareButton)
                RaiseResultCompleted("socialShare");

            else if (button == emailButton)
                RaiseResultCompleted("email");

            else if (button == pocketButton)
                RaiseResultCompleted("pocket");

            else if (button == oneNoteButton)
                RaiseResultCompleted("onenote");
        }

        void RaiseResultCompleted(string label)
        {
            ResultCompleted.Raise(this, PopupResult.Create(label));
        }

        void OnOutsideTap(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnCloseButtonClicked();
        }

        void OnCloseButtonClicked()
        {
            ResultCompleted.Raise(this, PopupResult.CreateUserDismissed<string>());
            BeginHide();
        }

        #endregion




        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}