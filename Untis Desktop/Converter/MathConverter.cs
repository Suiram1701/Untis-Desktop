using System;
using System.Globalization;
using System.Windows.Data;

namespace UntisDesktop.Converter;


[ValueConversion(typeof(double), typeof(double), ParameterType = typeof(string))]
internal class MathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double val = (double)value;
        string para = (string)parameter;

        char @operator = para[0];
        double otherValue = double.Parse(para[1..]);

        return @operator switch
        {
            '+' => val + otherValue,
            '-' => val - otherValue,
            '*' => val * otherValue,
            '/' => val / otherValue,
            _ => throw new ArgumentException($"The operator '{@operator}' is not supported")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
