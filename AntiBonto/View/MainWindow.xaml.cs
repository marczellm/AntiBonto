using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AntiBonto
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ViewModel.MainWindow viewModel { get { return (ViewModel.MainWindow)DataContext; } }

        private void LoadXLS(object sender, RoutedEventArgs e)
        {
            if (Type.GetTypeFromProgID("Excel.Application") == null)
            {
                MessageBox.Show("Excel nincs telepítve!");
                return;
            }
            LoadingAnimation.Visibility = Visibility.Visible;
            var dialog = new OpenFileDialog
            {
                Filter = "Excel|*.xls;*.xlsx;*.xlsm",
                DereferenceLinks = true,
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (dialog.ShowDialog(this) == true)
            {
                viewModel.People.Clear();
                viewModel.People.AddRange(ExcelHelper.LoadXLS(dialog.FileName));
            }
            LoadingAnimation.Visibility = Visibility.Hidden;
        }

        private void AddPerson(object sender, RoutedEventArgs e)
        {
            viewModel.People.CollectionChanged += People_CollectionChanged;
            viewModel.People.Add(new Person());
        }

        private void People_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            viewModel.People.CollectionChanged -= People_CollectionChanged;
            Person p = (Person) e.NewItems[0];
            var cp = (FrameworkElement)PeopleView.ItemContainerGenerator.ContainerFromItem(p);
            cp.ApplyTemplate();
            Button button = (Button)PeopleView.ItemTemplate.FindName("PersonButton", cp);
            TextBox textBox = new TextBox { MinWidth = 10};
            textBox.Tag = p;
            textBox.KeyDown += TextBox_KeyDown;
            button.Content = textBox;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() => Keyboard.Focus(textBox)));
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                ((Person)textBox.Tag).Name = textBox.Text;
                ((Button)textBox.Parent).Content = textBox.Text;
            }
        }
    }
}
