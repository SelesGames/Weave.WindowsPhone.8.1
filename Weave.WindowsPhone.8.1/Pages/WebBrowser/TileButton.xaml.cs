using SelesGames.Phone.ValueConverters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace weave
{
    public partial class TileButton : UserControl
    {
        public TileButton()
        {
            InitializeComponent();

            var b = new Binding("ImageSource")
            {
                Source = this,
                Converter = new DelegateValueConverter<ImageSource, ImageBrush>(
                    imageSource => new ImageBrush { ImageSource = imageSource },
                    null),
            };

            this.button.SetBinding(Button.ContentProperty, b);
            this.button.SetBinding(Button.BackgroundProperty, new Binding("Background") { Source = this });
            this.ButtonLabel.SetBinding(TextBlock.TextProperty, new Binding("Label") { Source = this });
        }

        public DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label",
            typeof(string),
            typeof(TileButton),
            null);

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(TileButton),
            null);

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
    }
}
