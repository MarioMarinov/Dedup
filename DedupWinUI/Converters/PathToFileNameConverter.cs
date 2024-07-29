using System;
using System.IO;
using Microsoft.UI.Xaml.Data;

namespace DedupWinUI.Converters
{
    public class PathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var path = (string)value;
            if (string.IsNullOrEmpty(path)) { return null; }
            return Path.GetFileName(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
