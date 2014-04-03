
namespace Microsoft.Phone.Controls
{
    public static class PageOrientationExtensions
    {
        public static bool IsAnyLandscape(this PageOrientation orientation)
        {
            return
                orientation == PageOrientation.Landscape ||
                orientation == PageOrientation.LandscapeLeft ||
                orientation == PageOrientation.LandscapeRight;
        }

        public static bool IsAnyPortrait(this PageOrientation orientation)
        {
            return
                orientation == PageOrientation.Portrait ||
                orientation == PageOrientation.PortraitDown ||
                orientation == PageOrientation.PortraitUp;
        }
    }
}
