using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Weave.ViewModels;

namespace weave
{
    public static class MediaTypeIconsBrushSet
    {
        static Brush all = "/Assets/Icons/mediaTypeIcons/video_podcast_zuneDL.png".ToImageBrush();
        static Brush videoPodcast = "/Assets/Icons/mediaTypeIcons/video_podcast.png".ToImageBrush();
        static Brush videoZune = "/Assets/Icons/mediaTypeIcons/video_zuneDL.png".ToImageBrush();
        static Brush video = "/Assets/Icons/mediaTypeIcons/video.png".ToImageBrush();
        static Brush podcastZune = "/Assets/Icons/mediaTypeIcons/podcast_zuneDL.png".ToImageBrush();
        static Brush podcast = "/Assets/Icons/mediaTypeIcons/podcast.png".ToImageBrush();
        static Brush zune = "/Assets/Icons/mediaTypeIcons/zuneDL.png".ToImageBrush();

        static Brush ToImageBrush(this string s)
        {
            var image = new BitmapImage { UriSource = new Uri(s, UriKind.Relative), CreateOptions = BitmapCreateOptions.None };
            return new ImageBrush { ImageSource = image };
            //return image;
        }

        static bool IsPresent(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        public static Brush GetMediaTypeImageBrush(this NewsItem o)
        {
            if (o == null)
                return null;

            if ((o.YoutubeId.IsPresent() || o.VideoUri.IsPresent()) && o.PodcastUri.IsPresent() && o.ZuneAppId.IsPresent())
                return all;

            else if ((o.YoutubeId.IsPresent() || o.VideoUri.IsPresent()) && o.PodcastUri.IsPresent())
                return videoPodcast;

            else if ((o.YoutubeId.IsPresent() || o.VideoUri.IsPresent()) && o.ZuneAppId.IsPresent())
                return videoZune;

            else if (o.YoutubeId.IsPresent() || o.VideoUri.IsPresent())
                return video;

            else if (o.PodcastUri.IsPresent() && o.ZuneAppId.IsPresent())
                return podcastZune;

            else if (o.PodcastUri.IsPresent())
                return podcast;

            else if (o.ZuneAppId.IsPresent())
                return zune;

            else
                return null;
        }
    }
}
