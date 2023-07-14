using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace UntisDesktop.Converter;

[ValueConversion(typeof(bool), typeof(Visibility), ParameterType = typeof(string))]
internal class BoolVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string str)
            return (str.Contains("Inv") ? !(bool)value : (bool)value) ? Visibility.Visible : (str.Contains("Hidden") ? Visibility.Hidden : Visibility.Collapsed);
        else
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
