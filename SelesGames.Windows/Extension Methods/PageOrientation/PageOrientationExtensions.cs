using Microsoft.Phone.Controls;

namespace SelesGames.Phone
{
    internal static class PageOrientationExtensions
    {
        public static SupportedPageOrientation AsSupportedPageOrientation(this PageOrientation orientation)
        {
            if (orientation == PageOrientation.Landscape || orientation == PageOrientation.LandscapeLeft || orientation == PageOrientation.LandscapeRight)
                return SupportedPageOrientation.Landscape;

            else if (orientation == PageOrientation.Portrait || orientation == PageOrientation.PortraitDown || orientation == PageOrientation.PortraitUp)
                return SupportedPageOrientation.Portrait;

            else
                return SupportedPageOrientation.Portrait;
        }
    }
}
