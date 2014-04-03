using SelesGames;
using SelesGames.Phone;
using SelesGames.Phone.ValueConverters;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Weave.Settings;
using Weave.ViewModels;

namespace weave
{
    public partial class MainPageNewsItemUI : BaseNewsItemControl, IDisposable
    {
        static Brush phoneChromeBrush, imageOutlineBrush;
        static Thickness noImageMargin, imageMargin;

        BindableMainPageFontStyle bindingSource;
        SerialDisposable disp = new SerialDisposable();

        

        static MainPageNewsItemUI()
        {
            phoneChromeBrush = App.Current.Resources["PhoneChromeBrush"] as Brush;
            imageOutlineBrush = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
            noImageMargin = new Thickness(24, 2, 24, 12);
            imageMargin = new Thickness(24, 2, 156, 12);
        }

        public MainPageNewsItemUI()
        {
            //InitializeComponent();
            OptimizedInitializeComponent();

            if (this.IsInDesignMode())
                return;

            bindingSource = ServiceResolver.Get<BindableMainPageFontStyle>();

            this.title.SetBinding(TextBlock.FontSizeProperty, bindingSource.TitleSizeBinding);
            this.title.SetBinding(TextBlock.FontFamilyProperty, bindingSource.FontFamilyBinding);

            this.feedName.SetBinding(TextBlock.FontSizeProperty, bindingSource.PublicationLineSizeBinding);
            this.feedName.SetBinding(TextBlock.FontFamilyProperty, bindingSource.FontFamilyBinding);

            this.SetBinding(FrameworkElement.MarginProperty, bindingSource.MainPageNewsItemMarginBinding);
        }




        #region Optimized Initialization

        void OptimizedInitializeComponent()
        {
            //this.Width = 480d;

            //CacheMode="BitmapCache" x:Name="tiltContentControl" HorizontalContentAlignment="Stretch" VerticalContentAlignment="Stretch" RenderTransformOrigin="0.5,0.5"
            this.tiltContentControl = new TiltContentControl
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new CompositeTransform(),
            };


            //<Grid x:Name="grid" Background="Transparent" CacheMode="BitmapCache">
            this.grid = new Grid { Background = new SolidColorBrush(Colors.Transparent) };


            //<StackPanel x:Name="textGrid" Margin="24,2,0,12" VerticalAlignment="Center" HorizontalAlignment="Left" Width="300">
            this.textGrid = new StackPanel
            {
                Margin = new Thickness(24d, 2d, 0d, 12d),
                VerticalAlignment = VerticalAlignment.Center,
                //HorizontalAlignment = HorizontalAlignment.Left,
                IsHitTestVisible = false,
                CacheMode = new BitmapCache(),
            };


            //<TextBlock x:Name="title" Text="Text" FontSize="{StaticResource PhoneFontSizeMediumLarge}" TextWrapping="Wrap" VerticalAlignment="Bottom" Margin="0,0,0,4" LineStackingStrategy="BlockLineHeight" />
            this.title = new TextBlock
            {
                //Foreground = foregroundBrush,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0d, 0d, 0d, 4d),
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                IsHitTestVisible = false,
            };
            //title.SetBinding(TextBlock.ForegroundProperty, new Binding("CurrentTheme.ForegroundBrush") { Source = AppSettings.Instance.Themes });



            //<TextBlock x:Name="publishedDateOverlay" Text="Engadget" Foreground="{StaticResource PhoneAccentBrush}" VerticalAlignment="Top" FontSize="20" Margin="0,4,9,0" TextWrapping="Wrap" Grid.Row="1" FontFamily="{StaticResource PhoneFontFamilyNormal}" FontWeight="Bold" />
            this.feedName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0d, 4d, 0d, 0d),
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false,
            };


            //<Rectangle x:Name="mediaTypesIcon" Fill="{StaticResource PhoneSubtleBrush}" Height="32" Width="128" HorizontalAlignment="Left" Margin="0,12,0,0" >
            //    <Rectangle.OpacityMask>
            //        <ImageBrush ImageSource="/Assets/Icons/mediaTypeIcons/zuneDL.png"/>
            //    </Rectangle.OpacityMask>
            //</Rectangle>
            this.mediaTypesIcon = new Rectangle
            {
                Height = 32d,
                Width = 128d,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(0d, 12d, 0d, 0d),
                IsHitTestVisible = false,
            };


            //<ImageBrush x:Name="image" Stretch="UniformToFill" ImageSource="http://gamernode.com/upload/manager///Dan%20Crabtree/Reviews/halo-reach-beta-gameplay1285174234.jpg"/>
            this.image = new System.Windows.Controls.Image
            {
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CacheMode = new BitmapCache(),
            };

            //<Rectangle x:Name="imageRect" Stretch="UniformToFill" RadiusX="0" RadiusY="0" Width="120" Height="120" Stroke="#FF777777" >
            this.imageRect = new Border
            {
                Background = phoneChromeBrush,
                BorderThickness = new Thickness(1),
                Width = 120d,
                Height = 120d,
                BorderBrush = imageOutlineBrush,
                Margin = new Thickness(0d, 12d, 24d, 12d),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                IsHitTestVisible = false,
                CacheMode = new BitmapCache(),
                Child = image,
            };

            this.textGrid.Children.Add(this.title);
            this.textGrid.Children.Add(this.feedName);
            this.textGrid.Children.Add(this.mediaTypesIcon);

            this.grid.Children.Add(this.textGrid);
            this.grid.Children.Add(this.imageRect);

            this.tiltContentControl.Content = this.grid;

            this.Content = this.tiltContentControl;



            /*
<Storyboard x:Name="ImageFadeInSB">
    <DoubleAnimation Duration="0:0:0.5" From="0" To="1" Storyboard.TargetProperty="(UIElement.Opacity)" Storyboard.TargetName="imageWrap"/>
</Storyboard>*/
            this.ImageFadeInSB = new Storyboard();
            var da = new DoubleAnimation { Duration = 0.5.Seconds(), From = 0d, To = 1d };
            Storyboard.SetTarget(da, this.image);
            Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
            this.ImageFadeInSB.Children.Add(da);

            this.OnLoadSB = new Storyboard();
            this.OnLoadBackwardsSB = new Storyboard();

            da = new DoubleAnimation { Duration = 0.5.Seconds(), From = 480d, To = 0d, EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(da, (CompositeTransform)this.tiltContentControl.RenderTransform);
            Storyboard.SetTargetProperty(da, new PropertyPath(CompositeTransform.TranslateXProperty));
            this.OnLoadSB.Children.Add(da);

            da = new DoubleAnimation { Duration = 0.5.Seconds(), From = -480d, To = 0d, EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(da, (CompositeTransform)this.tiltContentControl.RenderTransform);
            Storyboard.SetTargetProperty(da, new PropertyPath(CompositeTransform.TranslateXProperty));
            this.OnLoadBackwardsSB.Children.Add(da);


            /*            <Storyboard x:Name="OnLoadSB">
                <DoubleAnimation Duration="0:0:0.5" From="480" To="0" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.TranslateX)" Storyboard.TargetName="tiltContentControl">
                    <DoubleAnimation.EasingFunction>
                        <QuinticEase EasingMode="EaseOut"/>
                    </DoubleAnimation.EasingFunction>
                </DoubleAnimation>
            </Storyboard>            
            <Storyboard x:Name="OnLoadBackwardsSB">
                <DoubleAnimation Duration="0:0:0.5" From="-480" To="0" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.TranslateX)" Storyboard.TargetName="tiltContentControl">
                    <DoubleAnimation.EasingFunction>
                        <QuinticEase EasingMode="EaseOut"/>
                    </DoubleAnimation.EasingFunction>
                </DoubleAnimation>
            </Storyboard>*/
        }

        #endregion




        protected override void SetNewsItem(NewsItem newsItem)
        {
            disp.Disposable = null;
            var disposables = new CompositeDisposable();
            disp.Disposable = disposables;

            OnLoadSB.Stop();
            OnLoadBackwardsSB.Stop();
            ImageFadeInSB.Stop();

            ClearExistingImage();

            title.Text = newsItem.Title;
            feedName.Text = newsItem.FormattedForMainPageSourceAndDate;

            var mediaTypeImageBrush = newsItem.GetMediaTypeImageBrush();
            if (mediaTypeImageBrush != null)
            {
                mediaTypesIcon.OpacityMask = mediaTypeImageBrush;
                mediaTypesIcon.Visibility = Visibility.Visible;
            }
            else
            {
                mediaTypesIcon.Visibility = Visibility.Collapsed;
            }

            if (newsItem.HasImage)
            {
                //this.textGrid.Width = 300d;
                textGrid.Margin = imageMargin;

                image.Opacity = 0;
                imageRect.Visibility = Visibility.Visible;

                ImageCache
                    .GetImageAsync(newsItem.ImageUrl)
                    .SafelySubscribe(SetImage, ex => SetImage(FailImage))
                    .DisposeWith(disposables);
            }
            else
            {
                //this.textGrid.Width = 432d;
                textGrid.Margin = noImageMargin;
                imageRect.Visibility = Visibility.Collapsed;
            }

            Binding b = new Binding("DisplayState")
            {
                Converter = new DelegateValueConverter(value =>
                {
                    var displayState = (NewsItem.ColoringDisplayState)value;
                    return (displayState == NewsItem.ColoringDisplayState.Viewed) ? 0.6d : 1d;
                },
                    null),
                Source = newsItem
            };

            this.grid.SetBinding(UIElement.OpacityProperty, b);

            ColorByline(newsItem);

            Observable.FromEventPattern<PropertyChangedEventArgs>(newsItem, "PropertyChanged")
                .Where(o => o.EventArgs.PropertyName == "DisplayState")
                .SafelySubscribe(o => ColorByline(newsItem))
                .DisposeWith(disposables);
        }

        void ColorByline(NewsItem newsItem)
        {
            if (newsItem == null || feedName == null || mediaTypesIcon == null)
                return;

            Brush brush;

            if (newsItem.IsFavorite)
                brush = AppSettings.Instance.Themes.CurrentTheme.ComplementaryBrush;
            else if (newsItem.IsDisplayedAsNew)
                brush = AppSettings.Instance.Themes.CurrentTheme.AccentBrush;
            else
                brush = AppSettings.Instance.Themes.CurrentTheme.SubtleBrush;

            feedName.Foreground = brush;
            mediaTypesIcon.Fill = brush;
        }

        void ClearExistingImage()
        {
            if (image.Source != null && image.Source is BitmapImage)
            {
                var bi = (BitmapImage)image.Source;
                bi.UriSource = null;
                bi = null;
                image.Source = null;
            }
        }

        void SetImage(ImageSource image)
        {
            this.ImageFadeInSB.Stop();
            this.image.Source = image;
            this.ImageFadeInSB.Begin();
        }

        public override void PageRight()
        {
            OnLoadSB.Begin();
        }

        public override void PageLeft()
        {
            OnLoadBackwardsSB.Begin();
        }

        public void Dispose()
        {
            disp.Dispose();
        }
    }
}