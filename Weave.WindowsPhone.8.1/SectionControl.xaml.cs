using Weave.WindowsPhone._8._1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Weave.WindowsPhone._8._1
{
    public sealed partial class SectionControl : UserControl
    {

        #region SubtitleVisibility (DependencyProperty)

        /// <summary>
        /// Shows or hides the subtitle line.
        /// </summary>
        public Visibility SubtitleVisibility
        {
            get { return (Visibility)GetValue(SubtitleVisibilityProperty); }
            set { SetValue(SubtitleVisibilityProperty, value); }
        }
        public static readonly DependencyProperty SubtitleVisibilityProperty =
            DependencyProperty.Register("SubtitleVisibility", typeof(Visibility), typeof(SectionControl),
              new PropertyMetadata(Visibility.Visible));

        #endregion


        public SectionControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// See more will show more content about the Hub Section
        /// </summary>
        /// <param name="sender">TextBlock containing the See More text</param>
        /// <param name="e">Event data that describes the See More tapped</param>
        private void Group_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                //Show more groups on a different page
                TextBlock seeMoreTextBlock = e.OriginalSource as TextBlock;
                var group = seeMoreTextBlock.DataContext;

                if(!frame.Navigate(typeof(SectionPage), ((SampleDataGroup)group).UniqueId))
                {
                    throw new Exception("Navigation failed");
                }
            }
        }
    }
}
