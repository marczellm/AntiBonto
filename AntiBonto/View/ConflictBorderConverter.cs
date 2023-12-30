using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

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
            var pp = edge?.Persons;
            if (edge != null && pp[0].SleepingGroup == pp[1].SleepingGroup)
                return Brushes.Red;
            else if (p.WhoseNewcomer?.SleepingGroup == p.SleepingGroup || viewModel.SleepingGroups[p.SleepingGroup].Cast<Person>().Any(q => q.WhoseNewcomer == p))
                return Brushes.Green;
            else return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
