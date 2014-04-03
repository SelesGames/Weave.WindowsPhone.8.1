using SelesGames;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.ViewModels;
using Weave.ViewModels.Mobilizer;

namespace Weave.Services.OneNote
{
    public class OneNoteHelper
    {
        readonly static OneNoteHelper current = new OneNoteHelper();
        public static OneNoteHelper Current { get { return current; } }

        public Task Save(MobilizedArticle article, NewsItem newsItem, bool isArticleMobilizedSuccessfully)
        {
            var permState = ServiceResolver.Get<PermanentState>();
            var workflow = new OneNoteSavingWorkflow(permState, article, newsItem, isArticleMobilizedSuccessfully);
            return workflow.Save();
        }
    }
}
