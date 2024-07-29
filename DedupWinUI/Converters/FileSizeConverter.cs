using Microsoft.UI.Xaml.Data;
using System;

namespace DedupWinUI.Converters
{
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var length = (long)value;
            if (length <= 0) { return string.Format($"{length} bytes"); }
            if (length <= 1024) { return string.Format($"{length/1024} KB"); }
            if (length <= 1_048_576) { return string.Format($"{length / 1_048_576} MB"); }
            if (length <= 1_073_741_824) { return string.Format($"{length / 1_073_741_824} GB"); }
            return string.Empty;
        }

       
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
