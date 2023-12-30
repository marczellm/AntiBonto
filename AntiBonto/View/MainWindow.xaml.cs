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
using System.Windows.Threading;
using System.Xml.Serialization;

namespace AntiBonto
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
        private ViewModel.MainWindow viewModel => (ViewModel.MainWindow)DataContext;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendWindowFrame();
            LoadXML();
            DispatcherTimer dispatcherTimer = new()
            {
                Interval = TimeSpan.FromMinutes(4)
            };
            dispatcherTimer.Tick += new EventHandler((sender, e) => SaveXML());            
            dispatcherTimer.Start();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupColumnCount)));
        }

        public int GroupColumnCount => (int)this.ActualWidth / 150;

        private void ExtendWindowFrame()
        {
            try
            {
                IntPtr windowPtr = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(windowPtr).CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
                float rdpiy = System.Drawing.Graphics.FromHwnd(windowPtr).DpiY / 96;
                DwmAPI.Margins margins = new() { top = Convert.ToInt32(150 * rdpiy), left = 1, right = 1, bottom = 1 };
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
                using var file = new StreamReader(filepath);
                try { viewModel.AppData = (AppData)xs.Deserialize(file); }
                catch { } // If for example the XML is written by a previous version of this app, we shouldn't attempt to load it
            }

            int i = 1;
            foreach (TabItem tab in TabControl.Items)
            {
                if (tab.Tag != null && Int32.TryParse(tab.Tag as string, out int tag))
                {
                    tab.Header = (tab.Tag as string) + "HV";
                    tab.Visibility = ViewModel.MainWindow.WeekendNumber == tag ? Visibility.Visible : Visibility.Collapsed;
                }
                if (tab.Visibility == Visibility.Visible)
                    tab.Header = String.Format("{0}. {1}", i++, tab.Header);
            }
        }

        private void SaveXML()
        {
            if (viewModel.AppData.Persons.Any())
            {
                var xs = new XmlSerializer(typeof(AppData));
                using var file = new StreamWriter(filepath);
                xs.Serialize(file, viewModel.AppData);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveXML();
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
                viewModel.People.AddRange(await Task.Run(() =>
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

        private async void SaveXLS(object sender, RoutedEventArgs e)
        {
            if (Type.GetTypeFromProgID("Excel.Application") == null)
            {
                MessageBox.Show("Excel nincs telepítve!");
                return;
            }
            foreach (Person p in viewModel.PeopleToAssign)
            {
                foreach (Person q in p.includeEdges)
                {
                    if (p.SharingGroup != q.SharingGroup)
                    {
                        MessageBox.Show(String.Format("{0} és {1} együtt kéne legyenek kiscsoportban, de elmozgattad őket!", p, q));
                        return;
                    }
                }
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
            {
                try
                {
                    viewModel.SharingGroupExportOrdering();
                    await ExcelHelper.SaveXLS(dialog.FileName, viewModel);
                }
                catch (Exception ex)
                { 
                    MessageBox.Show("Hiba az Excel fájl írásakor" + Environment.NewLine + ex.Message ?? "" + Environment.NewLine + ex.InnerException?.Message ?? ""); 
                }
            }
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
            TextBox textBox = new() { MinWidth = 10, Tag = p };
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
            if (p[0].SharingGroupLeader && p[1].SharingGroupLeader)
                MessageBox.Show("Mindketten kiscsoportvezetők!");
            else if (p[0] != p[1])
            {
                viewModel.Edges.Add(edge);
                if (edge.Dislike && p[0].SharingGroup == p[1].SharingGroup)
                    p.First(q => !q.SharingGroupLeader).SharingGroup = -1;
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
                if (newTab != SharingGroupsTab && newTab != SleepingGroupsTab)
                    return;
                string message = null;
                var v = viewModel;
                if (v.People.Count == 0)
                {
                    message = "Nincsenek résztvevők!";
                    newTab = Participants;
                }
                else if (!v.SharingGroupLeaders.Any() && newTab == SharingGroupsTab)
                {
                    message = "Még nem jelölted ki a kiscsoportvezetőket!";
                    newTab = Roles;
                }
                else if (!v.SleepingGroupLeaders.Any() && newTab == SleepingGroupsTab)
                {
                    message = "Még nem jelölted ki az alvócsoportvezetőket!";
                    newTab = Roles;
                }
                else if (v.Newcomers.IsEmpty)
                {
                    message = "Nincsenek újoncok!";
                    newTab = Roles;
                }
                else if (v.Team.IsEmpty)
                {
                    message = "Nincs team!";
                    newTab = Roles;
                }
                else if (v.BoyLeader == null || v.GirlLeader == null)
                {
                    message = "Még nem jelölted ki a vezetőket!";
                    newTab = Roles;
                }
                else if (v.MusicLeader == null)
                {
                    message = "Még nem jelölted ki a zeneteamvezetőt!";
                    newTab = Roles;
                }
                else if (!viewModel.SexUndefined.IsEmpty)
                {
                    message = "Még nem osztottad be a lányokat és a fiúkat!";
                    newTab = Sexes;
                }
                if (message != null)
                {
                    viewModel.StatusText = message;
                    viewModel.MagicPossible = false;
                    SaveButton.IsEnabled = false;
                    // we don't activate newTab here anymore, because it caused weird behaviour
                }
                else if (newTab == SharingGroupsTab)
                {
                    viewModel.InitSharingGroups();

                    BindingOperations.GetBindingExpression(SharingGroups, ItemsControl.ItemsSourceProperty).UpdateTarget();
                    SharingGroups.Items.Refresh();

                    // TODO update all kcs views individually?

                    viewModel.Algorithm = new Algorithms(viewModel);
                    viewModel.MagicPossible = true;
                    BindingOperations.SetBinding(SaveButton, IsEnabledProperty, SaveButtonBinding);
                }
                else if (newTab == SleepingGroupsTab)
                {
                    viewModel.InitSharingGroups();
                    viewModel.InitSleepingGroups();

                    BindingOperations.GetBindingExpression(BoySleepingGroups, ItemsControl.ItemsSourceProperty).UpdateTarget();
                    BoySleepingGroups.Items.Refresh();

                    BindingOperations.GetBindingExpression(GirlSleepingGroups, ItemsControl.ItemsSourceProperty).UpdateTarget();
                    GirlSleepingGroups.Items.Refresh();

                    // TODO update all acs views individually?

                    BindingOperations.GetBindingExpression(SaveButton, IsEnabledProperty)?.UpdateTarget();
                }
                else if (newTab == Sexes)
                {
                    viewModel.SexUndefined.MoveCurrentToFirst();
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
                    var algorithmTask = Task.Run(() => alg.NaiveFirstFit(ct), ct);
                    DispatcherTimer timer = new()
                    {
                        Interval = TimeSpan.FromSeconds(10)
                    };
                    timer.Tick += (sender, e) =>
                    {
                        var dialogResult = MessageBox.Show("Nem találom a megoldást. Lehet, hogy túl sok a megkötés. Próbálkozzak még?", "AntiBonto", MessageBoxButton.YesNo);
                        if (dialogResult == MessageBoxResult.No)
                            cts.Cancel();
                    };
                    timer.Start();
                    bool result = await algorithmTask;
                    timer.Stop();
                    if (!result)
                        viewModel.StatusText = "Nem sikerült az automatikus beosztás!";
                }
                catch (AggregateException) { }
            }
            MagicAnimation.Visibility = Visibility.Collapsed;
            btn.Click -= handler;
            btn.Click += Magic;
            btn.Content = oldContent;
        }

        private void ClearSharingGroups(object sender, RoutedEventArgs e)
        {
            foreach (Person p in viewModel.PeopleToAssign)
                if (!p.SharingGroupLeader)
                    p.SharingGroup = -1;
        }

        private void ClearSleepingGroups(object sender, RoutedEventArgs e)
        {
            foreach (Person p in viewModel.PeopleToAssign)
                if (!p.SleepingGroupLeader)
                    p.SleepingGroup = -1;
        }

        private void Sexes_KeyUp(object sender, KeyEventArgs e)
        {
            Person p = (Person)viewModel.SexUndefined.CurrentItem;
            if (p != null)
            {
                if (e.Key == Key.Left)
                    p.Sex = Sex.Girl;
                else if (e.Key == Key.Right)
                    p.Sex = Sex.Boy;
            }
        }

        private void Recruiter_KeyUp(object sender, KeyEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            if (e.Key == Key.Delete && (string)dataGrid.CurrentColumn.Header == "Kinek az újonca")
                ((Person)dataGrid.CurrentItem).WhoseNewcomer = null;
        }

        private void WhoseNewcomer_Updated(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Person p = (Person)DataGrid.CurrentItem, q = (Person)e.AddedItems[0];
                if (q != null && p.SharingGroup == q.SharingGroup)
                    new Person[] { p, q }.First(r => !r.SharingGroupLeader).SharingGroup = -1;
            }
        }
    }
}