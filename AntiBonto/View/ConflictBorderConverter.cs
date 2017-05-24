using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AntiBonto.View
{
    /// <summary>
    /// Displays a border around persons for whom a conflict exists
    /// </summary>
    class ConflictBorderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Person p = (Person)values[0];
            var viewModel = (ViewModel.MainWindow)values[1];
            Edge edge = viewModel.Edges.FirstOrDefault(e => e.Dislike && e.Persons.Contains(p));
            if (edge != null && p.Alvocsoport == edge.Persons[1 - Array.IndexOf(edge.Persons, p)].Alvocsoport)
                return new Thickness(2);
            else return new Thickness(0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
