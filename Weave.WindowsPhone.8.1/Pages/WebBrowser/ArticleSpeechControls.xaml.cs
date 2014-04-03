using SelesGames;
using System;
using System.Windows.Controls;

namespace weave.Pages.WebBrowser
{
    public partial class ArticleSpeechControls : UserControl, IPopup<object>
    {
        public event EventHandler StopButtonPressed;

        public ArticleSpeechControls()
        {
            InitializeComponent();
        }

        void OnStopButtonTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            StopButtonPressed.Raise(this);
        }

        public void BeginShow()
        {
            ShowCompleted.Raise(this);
        }

        public void BeginHide()
        {
            busyIndicator.IsPlaying = false;
            HideCompleted.Raise(this);
        }

        public event EventHandler ShowCompleted;
        public event EventHandler HideCompleted;
        public event EventHandler<EventArgs<PopupResult<object>>> ResultCompleted;
    }
}
