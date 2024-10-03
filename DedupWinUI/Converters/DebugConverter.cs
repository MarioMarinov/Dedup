using System;
using Microsoft.UI.Xaml.Data;
using Serilog;

namespace DedupWinUI.Converters
{
 /// <summary>
 /// Use to debug binding issues
 /// </summary>
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Log.Information($"Convert called -> {parameter}:{value}");
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            Log.Information($"ConvertBack called -> {parameter}:{value}");
            return value;
        }
    }
}
