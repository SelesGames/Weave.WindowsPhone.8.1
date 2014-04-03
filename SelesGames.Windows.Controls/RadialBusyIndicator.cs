using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace SelesGames.Phone.Controls
{
    public class RadialBusyIndicator : Control
    {
        Grid Container;
        Style EllipseStyle;
        Storyboard PlayingSB;

        public RadialBusyIndicator()
        {
            DefaultStyleKey = typeof(RadialBusyIndicator);
        }

        public void CollapseAndStopAnimation()
        {
            Visibility = Visibility.Collapsed;
            IsPlaying = false;
        }

        public void MakeVisibleAndStartAnimation()
        {
            Visibility = Visibility.Visible;
            IsPlaying = true;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Container = GetTemplateChild("Container") as Grid;
            EllipseStyle = Container.Resources["EllipseStyle"] as Style;
            PlayingSB = Container.Resources["PlayingSB"] as Storyboard;

            CreateRadial();
            if (IsPlaying)
                OnIsPlayingChanged(IsPlaying);
        }

        void CreateRadial()
        {
            Container.Children.Clear();

            var split = 360d / NumberOfItems;
            for (int i = 0; i < NumberOfItems; i++)
            {
                var ellipse = new Ellipse { Style = EllipseStyle, Width = EllipseDiameter, Height = EllipseDiameter };
                var grid = new Grid
                {
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new CompositeTransform { Rotation = split * i },
                    CacheMode = new BitmapCache()
                };
                grid.Children.Add(ellipse);
                Container.Children.Add(grid);
            }
        }

        void OnIsPlayingChanged(bool isPlaying)
        {
            if (isPlaying)
            {
                PlayingSB.Stop();
                PlayingSB.Begin();
            }
            else
            {
                PlayingSB.Stop();
            }
        }




        #region Dependency Properties

        #region EllipseDiameter

        public static readonly DependencyProperty EllipseDiameterProperty =
            DependencyProperty.Register("EllipseDiameter", typeof(double), typeof(RadialBusyIndicator), new PropertyMetadata(8d, OnEllipseDiameterChanged));

        public double EllipseDiameter
        {
            get { return (double)GetValue(EllipseDiameterProperty); }
            set { SetValue(EllipseDiameterProperty, value); }
        }

        static void OnEllipseDiameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var button = (RadialBusyIndicator)obj;

            if (e.NewValue is double && button.Container != null)
            {
                var diameter = (double)e.NewValue;
                button.CreateRadial();
            }
        }

        #endregion




        #region NumberOfItems

        public static readonly DependencyProperty NumberOfItemsProperty =
            DependencyProperty.Register("NumberOfItems", typeof(int), typeof(RadialBusyIndicator), new PropertyMetadata(8, OnNumberOfItemsChanged));

        public int NumberOfItems
        {
            get { return (int)GetValue(NumberOfItemsProperty); }
            set { SetValue(NumberOfItemsProperty, value); }
        }

        static void OnNumberOfItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var button = (RadialBusyIndicator)obj;

            if (e.NewValue is int && button.Container != null)
            {
                var count = (int)e.NewValue;
                button.CreateRadial();
            }
        }

        #endregion




        #region IsPlaying

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(
                "IsPlaying",
                typeof(bool),
                typeof(RadialBusyIndicator),
                new PropertyMetadata(false, OnIsPlayingPropertyChanged));

        static void OnIsPlayingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as RadialBusyIndicator;
            if (source != null && source.PlayingSB != null)
            {
                source.OnIsPlayingChanged((bool)e.NewValue);
            }
        }

        #endregion

        #endregion
    }
}
