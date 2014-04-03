using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace SelesGames.Phone
{
    public static class TimespanExtensions
    {
        public static TimeSpan Seconds(this double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        public static TimeSpan Milliseconds(this double ms)
        {
            return TimeSpan.FromMilliseconds(ms);
        }

        public static TimeSpan Minutes(this double minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        }
    }

    public static class ext
    {
        public static DoubleAnimationBuilder Fade(this DependencyObject o)
        {
            if (!(o is UIElement))
                throw new Exception("o is not derived from UIElement, and has no Opacity property");

            return new DoubleAnimationBuilder(o);
        }

        public static DoubleAnimationBuilder From(this DoubleAnimationBuilder builder, double fromValue)
        {
            builder.dubAnimation.From = fromValue;
            return builder;
        }

        public static DoubleAnimationBuilder To(this DoubleAnimationBuilder builder, double toValue)
        {
            builder.dubAnimation.To = toValue;
            return builder;
        }

        public static DoubleAnimationBuilder Over(this DoubleAnimationBuilder builder, TimeSpan duration)
        {
            builder.dubAnimation.Duration = duration;
            return builder;
        }

        public static DoubleAnimationBuilder BeginIn(this DoubleAnimationBuilder builder, TimeSpan beginTime)
        {
            builder.dubAnimation.BeginTime = beginTime;
            return builder;
        }

        public static DoubleAnimationBuilder WithEase(this DoubleAnimationBuilder builder, EasingFunctionBase easeFunction)
        {
            builder.dubAnimation.EasingFunction = easeFunction;
            return builder;
        }
    }

    public static class AnimationBuilderExtensions
    {
        public static Storyboard ToStoryboard(this AnimationBuilder builder)
        {
            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(builder.Animation, builder.DObject);
            Storyboard.SetTargetProperty(builder.Animation, builder.Property);// new PropertyPath(builder.Property));
            sb.Children.Add(builder.Animation);

            return sb;
        }
    }

    public class AnimationBuilder
    {
        internal Timeline Animation { get; protected set; }
        internal DependencyObject DObject { get; protected set; }
        internal /*DependencyProperty*/ string Property { get; protected set; }
    }    
    
    public class DoubleAnimationBuilder : AnimationBuilder
    {
        internal DoubleAnimationBuilder(DependencyObject o)
        {
            base.Animation = dubAnimation = new DoubleAnimation();
            base.DObject = o;
            base.Property = "Opacity";// UIElement.OpacityProperty;
        }

        internal DoubleAnimation dubAnimation { get; private set; }
    }
}
