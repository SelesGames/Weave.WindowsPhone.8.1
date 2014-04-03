using System.Net;
using System.Text.RegularExpressions;

namespace Weave.ViewModels.Mobilizer
{
    public static class MobilizedArticleExtensions
    {
        public static string CreateSpokenRepresentation(this MobilizedArticle article)
        {
            var title = article.Title.Trim();
            var content = HttpUtility.HtmlDecode(Sanitize(article.ContentHtml).Trim()).Trim();

            var fullText = string.Format(
                "{0}.\r\n\r\nPublished by {1}.\r\n\r\n{2}",
                title,
                article.Source,
                content);

            return fullText;
        }




        #region Private helper functions.  Sanitize html, stripping out all html tags

        static Regex _tags = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            string tagname;
            Match tag;

            // match every HTML tag in the input
            MatchCollection tags = _tags.Matches(html);
            for (int i = tags.Count - 1; i > -1; i--)
            {
                tag = tags[i];
                tagname = tag.Value.ToLowerInvariant();

                html = html.Remove(tag.Index, tag.Length);
            }

            return html;
        }

        #endregion
    }
}
