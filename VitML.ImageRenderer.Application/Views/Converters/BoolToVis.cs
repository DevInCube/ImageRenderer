using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace VitML.ImageRenderer.App.Views.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = bool.Parse(value.ToString());
            bool inverse = false;
            if (parameter != null)
                inverse = bool.Parse(parameter.ToString());
            bool result = inverse ? !val : val;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
