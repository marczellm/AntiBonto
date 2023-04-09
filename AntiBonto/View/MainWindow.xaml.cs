using AntiBonto.View;
using AntiBonto.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AntiBonto
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiBonto");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            filepath = Path.Combine(folder, "state.xml");
        }
        private readonly string filepath;
        private CancellationTokenSource cts;

        private ViewModel.MainWindow viewModel => (ViewModel.MainWindow)DataContext;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendWindowFrame();
            LoadXML();
        }

        private void ExtendWindowFrame()
        {
            try
            {
                IntPtr windowPtr = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(windowPtr).CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
                float rdpiy = System.Drawing.Graphics.FromHwnd(windowPtr).DpiY / 96;
                DwmAPI.Margins margins = new DwmAPI.Margins { top = Convert.ToInt32(150 * rdpiy), left = 1, right = 1, bottom = 1 };
                if (DwmAPI.DwmExtendFrameIntoClientArea(windowPtr, ref margins) < 0)
                    Background = SystemColors.WindowBrush;
            }
            catch (DllNotFoundException)
            {
                Background = SystemColors.WindowBrush;
            }
        }

        /// <summary>
        /// Load the app state from AppData\...\AntiBonto\state.xml
        /// </summary>
        private void LoadXML()
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

            int i = 1;
            foreach (TabItem tab in TabControl.Items)
            {
                if (tab.Tag != null && Int32.TryParse(tab.Tag as string, out int tag))
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
            foreach (Person p in viewModel.CsoportokbaOsztando)
                foreach (Person q in p.kivelIgen)
                    if (p.Kiscsoport != q.Kiscsoport)
                    {
                        MessageBox.Show(String.Format("{0} és {1} együtt kéne legyenek kiscsoportban, de elmozgattad őket!", p, q));
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
                    viewModel.AlvocsoportDisplayOrdering();
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
            TextBox textBox = new TextBox { MinWidth = 10, Tag = p };
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
            var edge = viewModel.Edge;
            if (edge.Persons.Contains(null))
                return;
            var p = edge.Persons;
            if (p[0].Kiscsoportvezeto && p[1].Kiscsoportvezeto)
                MessageBox.Show("Mindketten kiscsoportvezetők!");
            else if (p[0] != p[1])
            {
                viewModel.Edges.Add(edge);
                if (edge.Dislike && p[0].Kiscsoport == p[1].Kiscsoport)
                    p.First(q => !q.Kiscsoportvezeto).Kiscsoport = -1;
                viewModel.Edge = new Edge { Dislike = edge.Dislike };
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
                if (v.People.Count() == 0)
                {
                    message = "Nincsenek résztvevők!";
                    newTab = Resztvevok;
                }
                else if (!v.Kiscsoportvezetok.Any() && newTab == Kiscsoportbeoszto)
                {
                    message = "Még nem jelölted ki a kiscsoportvezetőket!";
                    newTab = Szerepek;
                }
                else if (!v.Alvocsoportvezetok.Any() && newTab == Alvocsoportbeoszto)
                {
                    message = "Még nem jelölted ki az alvócsoportvezetőket!";
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
                    message = "Még nem jelölted ki a vezetőket!";
                    newTab = Szerepek;
                }
                else if (v.Zeneteamvezeto == null)
                {
                    message = "Még nem jelölted ki a zeneteamvezetőt!";
                    newTab = Szerepek;
                }
                else if (!viewModel.Nullnemuek.IsEmpty)
                {
                    message = "Még nem osztottad be a lányokat és a fiúkat!";
                    newTab = LanyokFiuk;
                }
                if (message != null)
                {
                    viewModel.StatusText = message;
                    viewModel.MagicPossible = false;
                    SaveButton.IsEnabled = false;
                    // we don't activate newTab here anymore, because it caused weird behaviour
                }
                else if (newTab == Kiscsoportbeoszto)
                {
                    viewModel.InitKiscsoport();
                    // TODO for all kcs views
                    // BindingOperations.GetBindingExpression(kcs[i], ItemsControl.ItemsSourceProperty).UpdateTarget();                    
                    viewModel.Algorithm = new Algorithms(viewModel);
                    viewModel.MagicPossible = true;
                    BindingOperations.SetBinding(SaveButton, IsEnabledProperty, SaveButtonBinding);
                }
                else if (newTab == Alvocsoportbeoszto)
                {
                    viewModel.InitKiscsoport();
                    viewModel.InitAlvocsoport();
                    viewModel.AlvocsoportDisplayOrdering();
                    // TODO for all acs views
                    // BindingOperations.GetBindingExpression(acs[j], ItemsControl.ItemsSourceProperty).UpdateTarget();
                    // acs[j].Items.Refresh();
                    BindingOperations.GetBindingExpression(SaveButton, IsEnabledProperty)?.UpdateTarget();                 
                }
                else if (newTab == LanyokFiuk)
                    viewModel.Nullnemuek.MoveCurrentToFirst();
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

        private void LanyokFiuk_KeyUp(object sender, KeyEventArgs e)
        {
            Person p = (Person)viewModel.Nullnemuek.CurrentItem;
            if (p != null)
            {
                if (e.Key == Key.Left)
                    p.Nem = Nem.Lany;
                else if (e.Key == Key.Right)
                    p.Nem = Nem.Fiu;
            }
        }

        private void Recruiter_KeyUp(object sender, KeyEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            if (e.Key == Key.Delete && (string)dataGrid.CurrentColumn.Header == "Kinek az újonca")
                ((Person)dataGrid.CurrentItem).KinekAzUjonca = null;
        }

        private void KinekAzUjonca_Updated(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Person p = (Person)DataGrid.CurrentItem, q = (Person)e.AddedItems[0];
                if (q != null && p.Kiscsoport == q.Kiscsoport)
                    new Person[] { p, q }.First(r => !r.Kiscsoportvezeto).Kiscsoport = -1;
            }
        }
    }
}