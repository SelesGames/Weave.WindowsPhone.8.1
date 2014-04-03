using SelesGames;
using System;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.SavedState;
using Weave.Settings;
using Weave.ViewModels;
using Weave.ViewModels.Mobilizer;

namespace Weave.WP.ViewModels
{
    public class ReadabilityPageViewModel
    {
        Formatter formatter;
        StandardThemeSet themes;
        FontSizes fontSizes;
        PermanentState permState;

        public NewsItem NewsItem { get; set; }
        public ArticleViewingType LastViewingType { get; set; }
        public MobilizedArticle CurrentMobilizedArticle { get; private set; }
        public string FullHtml { get; private set; }

        public ReadabilityPageViewModel()
        {
            formatter = ServiceResolver.Get<Formatter>();
            themes = AppSettings.Instance.Themes;
            fontSizes = new FontSizes();
            permState = ServiceResolver.Get<PermanentState>();
        }

        public async Task LoadMobilizedArticle()
        {
            if (NewsItem == null)
                throw new ArgumentNullException("NewsItem in ReadabilityPageViewModel");

            try
            {
                if (NewsItem.Feed != null)
                {
                    var articleViewingType = NewsItem.Feed.ArticleViewingType;
                    LastViewingType = articleViewingType;

                    var mobilizerRepo = new MobilizedArticleRepository(new Client());

                    CurrentMobilizedArticle = await mobilizerRepo.Get(NewsItem);
                    var html = GetMobilizedHtml();
                    FullHtml = html.ConvertExtendedASCII();
                }
            }
            catch 
            {
                CurrentMobilizedArticle = null;
                FullHtml = null;
                throw;
            }
        }




        #region Private helper method - create the full HTML based on mobilized article, theme colors, and fonts

        string GetMobilizedHtml()
        {
            var theme = themes.CurrentTheme;

            var foreground = theme.Text;
            var background = theme.Background;
            var linkColor = theme.Accent;

            var title = NewsItem.Title;
            var link = NewsItem.Link;

            var content = CurrentMobilizedArticle.ContentHtml;
            var heroImage = CurrentMobilizedArticle.HeroImageUrl;

            string source, pubDate;

            if (!CurrentMobilizedArticle.HasImage)
            {
                source = CurrentMobilizedArticle.CombinedPublicationAndDate;
                pubDate = null;
            }
            else
            {
                source = CurrentMobilizedArticle.Source.ToUpperInvariant();
                pubDate = CurrentMobilizedArticle.FullDate;
            }

            var fontSize = fontSizes.GetById(permState.ArticleViewFontSize).HtmlTextSize();

            var html = formatter.CreateHtml(source, title, pubDate, link, heroImage, content, foreground, background, permState.ArticleViewFontName, fontSize, linkColor);
            return html;
        }

        #endregion
    }
}