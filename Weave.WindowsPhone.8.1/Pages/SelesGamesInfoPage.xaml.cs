using Microsoft.Phone.Controls;
using SelesGames.Phone;
using Telerik.Windows.Controls;

namespace weave
{
    public partial class SelesGamesInfoPage : PhoneApplicationPage
    {
        public SelesGamesInfoPage()
        {
            InitializeComponent();
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumTransition());
            publisherControl.PublisherName = "Seles Games";       
        }

        void OnEmailLinkTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TaskService.ToEmailComposeTask(To: "info@selesgames.com");
        }
    }
}