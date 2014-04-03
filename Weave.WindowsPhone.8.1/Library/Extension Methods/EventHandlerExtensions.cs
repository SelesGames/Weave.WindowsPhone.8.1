using System;
using System.Reactive;
using System.Reactive.Linq;
using Windows.UI.Xaml;

namespace SelesGames
{
    public static class EventHandlerExtensions
    {
        public static void Raise(this EventHandler handler, object sender, EventArgs args)
        {
            if (handler != null)
                handler(sender, args);
        }

        public static void Raise(this EventHandler handler, object sender)
        {
            if (handler != null)
                handler(sender, EventArgs.Empty);
        }

        public static void Raise<T>(this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            if (handler != null)
                handler(sender, args);
        }

        public static void Raise<T>(this EventHandler<EventArgs<T>> handler, object sender, T obj)
        {
            if (handler != null)
                handler(sender, new EventArgs<T> { Item = obj });
        }
    }
}

namespace System.Windows
{
    public static class RoutedEventHandlerExtensions
    {
        public static void Raise(this RoutedEventHandler handler, object sender, RoutedEventArgs e)
        {
            if (handler != null)
                handler(sender, e);
        }
    }
}

namespace System.ComponentModel
{
    public static class PropertyChangedEventHandlerExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            if (handler != null)
                handler(sender, new PropertyChangedEventArgs(propertyName));
        }

        public static IObservable<string> GetPropertyChanged(this INotifyPropertyChanged p)
        {
            return Observable.FromEventPattern<PropertyChangedEventArgs>(p, "PropertyChanged").Select(o => o.EventArgs.PropertyName);
        }

        public static IObservable<Unit> GetPropertyChanged(this INotifyPropertyChanged p, string propertyName)
        {
            return GetPropertyChanged(p).Where(o => o.Equals(propertyName)).Select(_ => Unit.Default);
        }
    }
}

