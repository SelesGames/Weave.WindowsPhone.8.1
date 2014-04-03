using System;
using System.Windows;

namespace Weave.Services
{
    internal class SwipeGestureHelper : IDisposable
    {
        Delegate handler;
        UIElement element;

        public double VerticalMotionThreshold { get; set; }
        public double HorizontalVelocityThreshold { get; set; }

        public SwipeGestureHelper(UIElement element)
        {
            this.element = element;
            handler = new EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(OnManipulationCompleted);
            element.AddHandler(UIElement.ManipulationCompletedEvent, handler, true);

            VerticalMotionThreshold = 50d;
            HorizontalVelocityThreshold = 750d;
        }

        void OnManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            // flicks always finish on interia
            if (!e.IsInertial)
                return;

            var finalHorizontalVelocity = e.FinalVelocities.LinearVelocity.X;
            var absoluteVerticalMotion = Math.Abs(e.TotalManipulation.Translation.Y);

            if (absoluteVerticalMotion < VerticalMotionThreshold)
            {
                if (finalHorizontalVelocity < -HorizontalVelocityThreshold)
                    OnRightSwipe();
                else if (finalHorizontalVelocity > HorizontalVelocityThreshold)
                    OnLeftSwipe();
            }
            //DebugEx.WriteLine("************ **********  horizontal velocity = {0}, total vertical motion: {1}", finalHorizontalVelocity, absoluteVerticalMotion);
        }

        void OnLeftSwipe()
        {
            if (Swipe != null)
                Swipe(this, new SwipeEventArgs(SwipeDirection.Left));
        }

        void OnRightSwipe()
        {
            if (Swipe != null)
                Swipe(this, new SwipeEventArgs(SwipeDirection.Right));
        }

        public enum SwipeDirection
        {
            Left,
            Right
        }

        public class SwipeEventArgs : EventArgs
        {
            public SwipeEventArgs(SwipeDirection direction)
            {
                Direction = direction;
            }

            public SwipeDirection Direction { get; private set; }
        }

        public event EventHandler<SwipeEventArgs> Swipe;

        public void Dispose()
        {
            element.RemoveHandler(UIElement.ManipulationCompletedEvent, handler);
        }
    }
}
