using Microsoft.Phone.Controls;
using SelesGames;
using System.Threading.Tasks;

namespace weave
{
    public partial class DummyPage : PhoneApplicationPage
    {
        public DummyPage()
        {
            InitializeComponent();
        }

        public async Task LayoutPopups()
        {
            SelesGames.PopupService.ForceLayout();

            var tempAccentShare = ServiceResolver.Get<SocialShareContextMenuControl>("accent");
            var tempFontSize = ServiceResolver.Get<FontSizePopup>();
            var tempArticleListFontPopup = ServiceResolver.Get<FontAndThemePopup>();

            LayoutRoot.Children.Insert(0, tempAccentShare);
            LayoutRoot.Children.Insert(1, tempFontSize);
            LayoutRoot.Children.Insert(2, tempArticleListFontPopup);

            await Task.Yield();

            LayoutRoot.Children.Remove(tempArticleListFontPopup);
            LayoutRoot.Children.Remove(tempFontSize);
            LayoutRoot.Children.Remove(tempAccentShare);
        }
    }
}