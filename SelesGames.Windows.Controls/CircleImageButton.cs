using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace SelesGames.Phone.Controls
{
    public class CircleImageButton : Button
    {
        UIElement image;

        public CircleImageButton()
        {
            DefaultStyleKey = typeof(CircleImageButton);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            image = GetTemplateChild("Image") as UIElement;

            if (image != null)
                image.OpacityMask = new ImageBrush { ImageSource = Source };

            var visualState = GetTemplateChild("Pressed") as VisualState;
            var frame = visualState.Storyboard.Children[0] as ObjectAnimationUsingKeyFrames;
            frame.KeyFrames[0].Value = PressedForeground;

            frame = visualState.Storyboard.Children[1] as ObjectAnimationUsingKeyFrames;
            frame.KeyFrames[0].Value = PressedBackground;

            frame = visualState.Storyboard.Children[2] as ObjectAnimationUsingKeyFrames;
            frame.KeyFrames[0].Value = PressedBackground;
        }

        /// <summary>
        /// Identifies the Source DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(CircleImageButton), new PropertyMetadata(null, OnSourceChanged));

        /// <summary>
        /// Gets or sets the Image source.
        /// </summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var button = (CircleImageButton)obj;

            if (e.NewValue is ImageSource && button.image != null)
            {
                var image = (ImageSource)e.NewValue;
                button.image.OpacityMask = new ImageBrush { ImageSource = image };
                //button.UpdateLayout();
            }
        }




        public static readonly DependencyProperty PressedForegroundProperty =
            DependencyProperty.Register("PressedForeground", typeof(Brush), typeof(CircleImageButton), null);

        /// <summary>
        /// Gets or sets the Image source.
        /// </summary>
        public Brush PressedForeground
        {
            get { return (Brush)GetValue(PressedForegroundProperty); }
            set { SetValue(PressedForegroundProperty, value); }
        }

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register("PressedBackground", typeof(Brush), typeof(CircleImageButton), null);

        /// <summary>
        /// Gets or sets the Image source.
        /// </summary>
        public Brush PressedBackground
        {
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }


        public static readonly DependencyProperty CircleThicknessProperty =
            DependencyProperty.Register("CircleThickness", typeof(double), typeof(CircleImageButton), new PropertyMetadata(3d));

        /// <summary>
        /// Gets or sets the Image source.
        /// </summary>
        public double CircleThickness
        {
            get { return (double)GetValue(CircleThicknessProperty); }
            set { SetValue(CircleThicknessProperty, value); }
        }
    }
}
