using AntiBonto.View;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;

namespace AntiBonto
{
    [Serializable]
    public class AppData
    {
        public Person[] Persons;
        public Edge[] Edges;
    }
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
            kcs = new DnDItemsControl[] { kcs1, kcs2, kcs3, kcs4, kcs5, kcs6, kcs7, kcs8, kcs9, kcs10, kcs11, kcs12};
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiBonto");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            filepath = Path.Combine(folder, "state.xml");
        }
        private DnDItemsControl[] kcs;
        private string filepath;
        private CancellationTokenSource cts;
        private AppData AppData
        {
            get { return new AppData { Persons = viewModel.People.ToArray(), Edges = viewModel.Edges.ToArray() }; }
            set { viewModel.People.AddRange(value.Persons); viewModel.Edges.AddRange(value.Edges); }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var xs = new XmlSerializer(typeof(AppData));
            if (File.Exists(filepath))
            {
                using (var file = new StreamReader(filepath))
                {
                    try
                    {
                        AppData = (AppData)xs.Deserialize(file);
                        // The XML serializer doesn't handle object references, so we replace Person copies with references by name
                        foreach (Edge edge in viewModel.Edges)
                            for (int i = 0; i < edge.Persons.Count(); i++)
                                edge.Persons[i] = viewModel.People.Single(p => p.Name == edge.Persons[i].Name);
                        foreach (Person person in viewModel.People)
                            if (person.KinekAzUjonca != null)
                                person.KinekAzUjonca = viewModel.People.Single(p => p.Name == person.KinekAzUjonca.Name);
                    }
                    catch { } // If for example the XML is written by a previous version of this app, we shouldn't attempt to load it
                }
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var xs = new XmlSerializer(typeof(AppData));
            using (var file = new StreamWriter(filepath))
            {
                xs.Serialize(file, AppData);
            }
        }

        private ViewModel.MainWindow viewModel { get { return (ViewModel.MainWindow)DataContext; } }
        
        /// <summary>
        /// Event handler
        /// </summary>
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

        /// <summary>
        /// Event handler
        /// </summary>
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
            var label = (ContentControl)PeopleView.ItemTemplate.FindName("PersonButton", cp);
            TextBox textBox = new TextBox { MinWidth = 10 };
            textBox.Tag = p;
            textBox.KeyDown += TextBox_KeyDown;
            label.Content = textBox;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() => Keyboard.Focus(textBox)));
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                ((Person)textBox.Tag).Name = textBox.Text;
                ((ContentControl)textBox.Parent).Content = textBox.Text;
            }
        }

        private void AddEdge(object sender, RoutedEventArgs e)
        {
            viewModel.Edges.Add(viewModel.Edge);
            viewModel.Edge = new Edge();
        }

        private void RemoveEdge(object sender, RoutedEventArgs e)
        {
            Edge edge = (Edge)((FrameworkElement)sender).DataContext;
            viewModel.Edges.Remove(edge);
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            viewModel.People.Clear();
            viewModel.Edges.Clear();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] == Kiscsoportbeoszto)
            {
                string message = null;
                var newTab = e.AddedItems[0];
                var v = viewModel;
                var k = viewModel.KiscsoportbaOsztando.Cast<Person>().ToList();
                if (v.People.Count() == 0)
                {
                    message = "Nincsenek résztvevők!";
                    newTab = Resztvevok;
                }                
                else if (k.Any(p => p.Age < 0 || p.Age > 100))
                {
                    message = "Állítsd be az életkorokat!";
                    newTab = Eletkorok;
                }
                else if (v.Kiscsoportvezetok.IsEmpty)
                {
                    message = "Jelöld ki a kiscsoportvezetőket!";
                    newTab = Szerepek;
                }
                else if (v.Ujoncok.IsEmpty)
                {
                    message = "Nincsenek újoncok!";
                    newTab = Szerepek;
                }
                else if (v.Team.IsEmpty)
                {
                    message = "Nincs team!";
                    newTab = Szerepek;
                }
                else if (v.Fiuvezeto == null || v.Lanyvezeto == null || v.Zeneteamvezeto == null)
                {
                    message = "Jelöld ki a vezetőket!";
                    newTab = Szerepek;
                }
                else if (LanyokFiuk.Visibility == Visibility.Visible && k.Any(p => p.Nem == Nem.Undefined))
                {
                    message = "Még nem válogattad ki a lányokat és a fiúkat!";
                    newTab = LanyokFiuk;
                }
                if (message != null)
                {
                    MessageBox.Show(message);
                    ((TabControl)sender).SelectedItem = newTab;
                }
                else
                {
                    viewModel.InitKiscsoport();
                    var kcsn = viewModel.Kiscsoportvezetok.Cast<Person>().Count();
                    for (int i = 0; i < kcs.Count(); i++)
                    {
                        kcs[i].Visibility = i < kcsn ? Visibility.Visible : Visibility.Collapsed;
                        kcs[i].IsEnabled = i < kcsn;                        
                        if (i < kcsn)
                            BindingOperations.GetBindingExpression(kcs[i], ItemsControl.ItemsSourceProperty).UpdateTarget();
                    }                    
                    viewModel.Algorithm = new Algorithms(viewModel);
                }
            }
        }

        private async void Magic(object sender, RoutedEventArgs e)
        {
            viewModel.Status = "";
            if (viewModel.MaxAgeDifference < 8
                && MessageBox.Show("8-nál kisebb maximális korkülönbséget állítottál be. Végtelen sokáig fog tartani a beosztás. Akarod folytatni?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            LoadingAnimation2.Visibility = Visibility.Visible;
            var alg = viewModel.Algorithm;
            var btn = (Button)sender;
            btn.Click -= Magic;
            var oldContent = btn.Content;
            btn.Content = "Cancel";
            RoutedEventHandler handler;
            using (cts = new CancellationTokenSource())
            {
                handler = (ender, se) => cts.Cancel();
                btn.Click += handler;
                CancellationToken ct = cts.Token;
                try
                {
                    if (!await Task.Run(() => alg.NaiveFirstFit(ct), ct))
                        viewModel.Status = "Nem sikerült az automatikus beosztás!";
                }
                catch (AggregateException) { }
            }
            LoadingAnimation2.Visibility = Visibility.Collapsed;
            btn.Click -= handler;
            btn.Click += Magic;
            btn.Content = oldContent;            
        }
    }
}