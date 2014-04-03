using Microsoft.Phone.Controls;
using System;

namespace SelesGames.Phone
{
    internal static class SupportedPageOrientationExtensions
    {
        public static bool Contains(this SupportedPageOrientation sp, PageOrientation po)
        {
            if (sp == SupportedPageOrientation.PortraitOrLandscape)
                return true;
            else if (sp == SupportedPageOrientation.Portrait)
                return po == PageOrientation.Portrait || po == PageOrientation.PortraitUp || po == PageOrientation.PortraitDown || po == PageOrientation.None;

            else if (sp == SupportedPageOrientation.Landscape)
                return po == PageOrientation.Landscape || po == PageOrientation.LandscapeLeft || po == PageOrientation.LandscapeRight;

            else
                throw new Exception("unrecognized SupportedPageOrientation sp in SupportedPageOrientationExtensions.Contains extension method");
        }
    }
}
