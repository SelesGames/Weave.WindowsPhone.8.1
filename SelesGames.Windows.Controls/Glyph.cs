using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace SelesGames.Phone.Controls
{
    public class Glyph : Control
    {
        Rectangle rectangle;

        public Glyph()
        {
            DefaultStyleKey = typeof(Glyph);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            rectangle = GetTemplateChild("Rectangle") as Rectangle;

            if (rectangle != null && Source != null)
                rectangle.OpacityMask = new ImageBrush { ImageSource = Source };
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(Glyph), new PropertyMetadata(OnSourceChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var glyph = (Glyph)obj;

            if (e.NewValue is ImageSource && glyph.rectangle != null)
            {
                var image = (ImageSource)e.NewValue;
                glyph.rectangle.OpacityMask = new ImageBrush { ImageSource = image };
            }
        }
    }
}
