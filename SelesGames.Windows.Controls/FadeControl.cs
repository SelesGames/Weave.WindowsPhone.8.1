using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SelesGames.Phone.Controls
{
    public class FadeControl : Control
    {
        Rectangle FadingElement;

        public FadeControl()
        {
            DefaultStyleKey = typeof(FadeControl);
            //FadeStops = new List<FadeStop>();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FadingElement = base.GetTemplateChild("FadingElement") as Rectangle;

            CreateFade();
        }

        void CreateFade()
        {
            if (FadeStops == null || FadingElement == null || !(Background is SolidColorBrush))
                return;

            var fsc = FadeStops;

            var color = ((SolidColorBrush)Background).Color;
            var newStops = fsc.Select(o =>
                new GradientStop
                {
                    Color = Color.FromArgb((byte)(255d * o.Percent), color.R, color.G, color.B),
                    Offset = o.Offset,
                })
                .ToList();

            var gsc = new GradientStopCollection();
            foreach (var stop in newStops)
                gsc.Add(stop);

            var lgb = new LinearGradientBrush(gsc, Angle);

            FadingElement.Fill = lgb;
        }

        public static readonly DependencyProperty FadeStopsProperty =
            DependencyProperty.Register("FadeStops", typeof(List<FadeStop>), typeof(FadeControl), new PropertyMetadata(new List<FadeStop>(), OnFadeStopsChanged));

        public List<FadeStop> FadeStops
        {
            get { return (List<FadeStop>)GetValue(FadeStopsProperty); }
            set { SetValue(FadeStopsProperty, value); }
        }

        static void OnFadeStopsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (FadeControl)obj;
            c.CreateFade();
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(FadeControl), new PropertyMetadata(0d, OnAngleChanged));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        static void OnAngleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (FadeControl)obj;
            c.CreateFade();
        }
    }

    // Summary:
    //     Describes the location and color of a transition point in a gradient.
    [ContentProperty("Color")]
    public sealed class FadeStop : DependencyObject
    {
        public static readonly DependencyProperty PercentProperty =
            DependencyProperty.Register("Percent", typeof(double), typeof(FadeStop), null);

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(double), typeof(FadeStop), null);


        public double Percent
        {
            get { return (double)GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }

        public double Offset
        {
            get { return (double)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }
    }
}
