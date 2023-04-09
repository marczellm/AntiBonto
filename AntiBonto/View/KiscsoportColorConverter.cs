using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;

namespace AntiBonto.View
{
    /// <summary>
    /// Mapping from kiscsoport numbers to colors. The coloring is conditional on them being in the same sleeping group
    /// </summary>
    class KiscsoportColorConverter : IMultiValueConverter
    {
        private static readonly byte[,] colors = new byte[,] { { 125, 135, 185 }, { 190, 193, 212 }, { 214, 188, 192 }, { 187, 119, 132 },
             { 133, 149, 225 }, { 181, 187, 227 }, { 230, 175, 185 }, { 224, 123, 145 },
              { 141, 213, 147 }, { 198, 222, 199 }, { 234, 211, 198 }, { 240, 185, 141 },
             { 15, 207, 192 }, { 156, 222, 214 }, { 213, 234, 231 }, { 243, 225, 235 }, { 246, 196, 225 } };
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Person p = (Person)values[0];
            var viewModel = (ViewModel.MainWindow)values[1];
            int i = p.Kiscsoport;
            if (i != -1 && viewModel.Kiscsoportok[i].Cast<Person>().Any(q => p != q && p.Alvocsoport == q.Alvocsoport))
                return new SolidColorBrush(Color.FromArgb(127, colors[i, 0], colors[i, 1], colors[i, 2]));
            else return SystemColors.ControlBrush;            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}