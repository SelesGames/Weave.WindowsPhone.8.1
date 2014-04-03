using SelesGames;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.Settings;
using Weave.ViewModels;

namespace Weave.Services.Pocket
{
    public class PocketHelper
    {
        readonly static PocketHelper current = new PocketHelper();
        public static PocketHelper Current { get { return current; } }

        public Task Save(NewsItem newsItem)
        {
            var consumerKey = AppSettings.Instance.ThirdParty.Pocket.ConsumerKey;
            var permState = ServiceResolver.Get<PermanentState>();
            var workflow = new PocketSavingWorkflow(consumerKey, permState, newsItem);
            return workflow.Save();
        }
    }
}
