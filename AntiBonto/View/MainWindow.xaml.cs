using AntiBonto.View;
using AntiBonto.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            kcs = new DnDItemsControl[] { kcs1, kcs2, kcs3, kcs4, kcs5, kcs6, kcs7, kcs8, kcs9, kcs10, kcs11, kcs12, kcs13, kcs14 };
            acs = new DnDItemsControl[] { acs1, acs2, acs3, acs4, acs5, acs6, acs7, acs8, acs9, acs10, acs11, acs12, acs13, acs14 };
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiBonto");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            filepath = Path.Combine(folder, "state.xml");
        }
        private DnDItemsControl[] kcs, acs;
        private string filepath;
        private CancellationTokenSource cts;

        private ViewModel.MainWindow viewModel { get { return (ViewModel.MainWindow)DataContext; } }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var xs = new XmlSerializer(typeof(AppData));
            if (File.Exists(filepath))
            {
                using (var file = new StreamReader(filepath))
                {
                    try { viewModel.AppData = (AppData)xs.Deserialize(file); }
                    catch { } // If for example the XML is written by a previous version of this app, we shouldn't attempt to load it
                }
            }

            int i = 1, tag;
            foreach (TabItem tab in TabControl.Items)
            {
                if (tab.Tag != null && Int32.TryParse(tab.Tag as string, out tag))
                {
                    tab.Header = tab.Tag as string + "HV";
                    tab.Visibility = ViewModel.MainWindow.WeekendNumber == tag ? Visibility.Visible : Visibility.Collapsed;
                }
                if (tab.Visibility == Visibility.Visible)
                    tab.Header = String.Format("{0}. {1}", i++, tab.Header);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var xs = new XmlSerializer(typeof(AppData));
            using (var file = new StreamWriter(filepath))
            {
                xs.Serialize(file, viewModel.AppData);
            }
        }

        private async void LoadXLS(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            btn.Click -= LoadXLS;
            if (Type.GetTypeFromProgID("Excel.Application") == null)
            {
                MessageBox.Show("Excel nincs telepítve!");
                return;
            }
            XLSLoadingAnimation.Visibility = Visibility.Visible;
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
                viewModel.Edges.Clear();
                viewModel.People.Clear();
                viewModel.People.AddRange(await Task.Run<List<Person>>(() =>
                {
                    try { return ExcelHelper.LoadXLS(dialog.FileName); }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hiba az Excel fájl olvasásakor" + Environment.NewLine + ex.Message ?? "" + Environment.NewLine + ex.InnerException?.Message ?? "");
                        return new List<Person>();
                    }
                }));
            }
            XLSLoadingAnimation.Visibility = Visibility.Hidden;
            btn.Click += LoadXLS;
        }

        private void SaveXLS(object sender, RoutedEventArgs e)
        {
            if (Type.GetTypeFromProgID("Excel.Application") == null)
            {
                MessageBox.Show("Excel nincs telepítve!");
                return;
            }
            XLSSavingAnimation.Visibility = Visibility.Visible;
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xlsm",
                Filter = "Excel|*.xls;*.xlsx;*.xlsm",
                DereferenceLinks = true,
                AddExtension = true,
                CheckPathExists = true
            };
            if (dialog.ShowDialog(this) == true)
                try
                {
                    viewModel.AlvocsoportExportOrdering();
                    viewModel.KiscsoportExportOrdering();
                    ExcelHelper.SaveXLS(dialog.FileName, viewModel);
                    viewModel.AlvocsoportDisplayOrdering(acs.Count() - acs.Count() / 2);
                }
                catch (Exception ex) { MessageBox.Show("Hiba az Excel fájl írásakor" + Environment.NewLine + ex.Message ?? "" + Environment.NewLine + ex.InnerException?.Message ?? ""); }
            XLSSavingAnimation.Visibility = Visibility.Hidden;
        }

        private void AddPerson(object sender, RoutedEventArgs e)
        {
            viewModel.People.CollectionChanged += People_CollectionChanged;
            viewModel.People.Add(new Person());
        }

        private void People_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            viewModel.People.CollectionChanged -= People_CollectionChanged;
            Person p = (Person)e.NewItems[0];
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
                if (textBox.Text.Contains(' '))
                {
                    ((Person)textBox.Tag).Name = textBox.Text;
                    ((ContentControl)textBox.Parent).Content = textBox.Text;
                }
                else
                    MessageBox.Show("Kell legyen vezetékneve és keresztneve is!");
            }
        }

        private void AddEdge(object sender, RoutedEventArgs e)
        {
            if (viewModel.Edge.Persons.Contains(null))
                return;
            if (viewModel.Edge.Persons[0].Kiscsoportvezeto && viewModel.Edge.Persons[1].Kiscsoportvezeto)
                MessageBox.Show("Mindketten kiscsoportvezetők!");
            else if (viewModel.Edge.Persons[0] != viewModel.Edge.Persons[1])
            {
                viewModel.Edges.Add(viewModel.Edge);
                viewModel.Edge = new Edge { Dislike = viewModel.Edge.Dislike };
            }
        }

        private void RemoveEdge(object sender, RoutedEventArgs e)
        {
            Edge edge = (Edge)((FrameworkElement)sender).DataContext;
            viewModel.Edges.Remove(edge);
        }

        private void Edge_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                RemoveEdge(sender, null);
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            viewModel.People.Clear();
            viewModel.Edges.Clear();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                viewModel.StatusText = "";
                var newTab = e.AddedItems[0];
                if (newTab != Kiscsoportbeoszto && newTab != Alvocsoportbeoszto)
                    return;
                string message = null;
                var v = viewModel;
                var k = viewModel.CsoportokbaOsztando.Cast<Person>().ToList();
                if (v.People.Count() == 0)
                {
                    message = "Nincsenek résztvevők!";
                    newTab = Resztvevok;
                }
                else if (!v.Kiscsoportvezetok.Any() && newTab == Kiscsoportbeoszto)
                {
                    message = "Jelöld ki a kiscsoportvezetőket!";
                    newTab = Szerepek;
                }
                else if (!v.Alvocsoportvezetok.Any() && newTab == Alvocsoportbeoszto)
                {
                    message = "Jelöld ki az alvócsoportvezetőket!";
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
                else if (v.Fiuvezeto == null || v.Lanyvezeto == null)
                {
                    message = "Jelöld ki a vezetőket!";
                    newTab = Szerepek;
                }
                else if (v.Zeneteamvezeto == null)
                {
                    message = "Jelöld ki a zeneteamvezetőt!";
                    newTab = Szerepek;
                }
                if (message != null)
                {
                    viewModel.StatusText = message;
                    MagicButton.IsEnabled = false;
                    SaveButton.IsEnabled = false;
                    // we don't activate newTab here anymore, because it caused weird behaviour
                }
                else if (newTab == Kiscsoportbeoszto)
                {
                    viewModel.InitKiscsoport();
                    var kcsn = viewModel.Kiscsoportvezetok.Count();
                    for (int i = 0; i < kcs.Count(); i++)
                    {
                        kcs[i].Visibility = i < kcsn ? Visibility.Visible : Visibility.Collapsed;
                        kcs[i].IsEnabled = i < kcsn;
                        if (i < kcsn)
                            BindingOperations.GetBindingExpression(kcs[i], ItemsControl.ItemsSourceProperty).UpdateTarget();
                    }
                    viewModel.Algorithm = new Algorithms(viewModel);
                    MagicButton.IsEnabled = true;
                    BindingOperations.SetBinding(SaveButton, IsEnabledProperty, SaveButtonBinding);
                }
                else if (newTab == Alvocsoportbeoszto)
                {
                    viewModel.InitAlvocsoport();
                    viewModel.AlvocsoportDisplayOrdering(acs.Count() - acs.Count() / 2);
                    for (int j = 0; j < acs.Count(); j++)
                    {
                        bool b = viewModel.Alvocsoport(j).Any();
                        acs[j].Visibility = b ? Visibility.Visible : Visibility.Collapsed;
                        acs[j].IsEnabled = b;
                        if (b)
                            BindingOperations.GetBindingExpression(acs[j], ItemsControl.ItemsSourceProperty).UpdateTarget();
                    }
                    BindingOperations.GetBindingExpression(SaveButton, IsEnabledProperty)?.UpdateTarget();
                }
            }
        }

        private async void Magic(object sender, RoutedEventArgs e)
        {
            viewModel.StatusText = "";
            MagicAnimation.Visibility = Visibility.Visible;
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
                        viewModel.StatusText = "Nem sikerült az automatikus beosztás!";
                }
                catch (AggregateException) { }
            }
            MagicAnimation.Visibility = Visibility.Collapsed;
            btn.Click -= handler;
            btn.Click += Magic;
            btn.Content = oldContent;
        }

        private void ClearKiscsoportok(object sender, RoutedEventArgs e)
        {
            foreach (Person p in viewModel.CsoportokbaOsztando)
                if (!p.Kiscsoportvezeto)
                    p.Kiscsoport = -1;
        }

        private void ClearAlvocsoportok(object sender, RoutedEventArgs e)
        {
            foreach (Person p in viewModel.CsoportokbaOsztando)
                if (!p.Alvocsoportvezeto)
                    p.Alvocsoport = -1;
        }

        private void Recruiter_KeyDown(object sender, KeyEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            if (e.Key == Key.Delete && (string)dataGrid.CurrentColumn.Header == "Kinek az újonca")
                ((Person)dataGrid.CurrentItem).KinekAzUjonca = null;
        }
    }
}