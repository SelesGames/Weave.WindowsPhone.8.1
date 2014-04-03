using System;

namespace weave.Pages.WebBrowser
{
    public class ArticleNotViewableException : Exception
    {
        public ArticleNotViewableException(string message) : base(message) { }
    }
}
