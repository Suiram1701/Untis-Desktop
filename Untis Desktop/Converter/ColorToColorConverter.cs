using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UntisDesktop.Converter;

[ValueConversion(typeof(Color), typeof(System.Windows.Media.Color))]
internal class ColorToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Color color = (Color)value;
        return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
