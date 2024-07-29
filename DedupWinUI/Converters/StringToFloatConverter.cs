using Microsoft.UI.Xaml.Data;
using System;

namespace DedupWinUI.Converters
{

    public class StringToFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string strValue && float.TryParse(strValue, out float result))
            {
                return result;
            }
            return 0f; // Default value if conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }
    }

}
