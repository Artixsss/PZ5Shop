using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PZ5Shop.Views.Converters
{
    public class ImageUrlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var fullPath = Path.Combine(baseDir, url);
                    uri = new Uri(fullPath, UriKind.Absolute);
                }

                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = uri;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
