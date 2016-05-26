using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AntiBonto.View
{
    class PersonTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((PersonType)value) == PersonType.Ujonc ? Brushes.Green : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
