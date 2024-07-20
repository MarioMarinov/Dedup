using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;


namespace DedupWinUI.Converters 
{
    /// <summary>
    /// One-way convert from System.Drawing.Image to System.Windows.Media.ImageSource
    /// </summary>
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) { return null; }

            var image = (Image)value;
            using (var ms = new MemoryStream()) {
                image.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(ms.AsRandomAccessStream());
                return bitmapImage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
      
    }
}