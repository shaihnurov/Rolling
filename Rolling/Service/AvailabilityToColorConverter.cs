using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Rolling.Service;

public class AvailabilityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? Brushes.Red : Brushes.Green;
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported.");
    }
}