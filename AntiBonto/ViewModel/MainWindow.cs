using GongSolutions.Wpf.DragDrop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Diagnostics;

namespace AntiBonto.ViewModel
{
    /// <summary>
    /// ObservableCollection with added AddRange support
    /// </summary>
    public class ObservableCollection2<T> : ObservableCollection<T>
    {
        public ObservableCollection2() : base() { }
        public ObservableCollection2(T[] t) : base(t) { }
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection)
                Items.Add(i);            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void RemoveAll(Func<T, bool> cond)
        {
            Items.Where(cond).ToList().All(p => Items.Remove(p));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    /// <summary>
    /// All the data that's saved to XML so that it's there for the next app launch
    /// </summary>
    [Serializable]
    public class AppData
    {
        public Person[] Persons;
        public Edge[] Edges;
        public Person[][] MutuallyExclusiveGroups;

        #region Extras
        public Person[] Szentendre;
        #endregion
    }
    /// <summary>
    /// Because this is not an enterprise app, I didn't create the plumbing necessary to have separate ViewModels for each tab.
    /// Instead I dumped all of the application state in the below class.
    /// </summary>
    public class MainWindow: ViewModelBase, IDropTarget
    {
        /// <summary>
        /// Most tabs disable if this is false
        /// </summary>
        public bool PeopleNotEmpty
        {
            get { return People.Count() != 0; }
        }
        /// <summary>
        /// The Save button disables if this is false
        /// </summary>
        public bool BeosztasKesz
        {
            get { return !Kiscsoport(-1).Any() && !Alvocsoport(-1).Any(); }
        }
        public static int WeekendNumber
        {
            get { return 2 * DateTime.Now.Year - 4013 + DateTime.Now.Month / 7; }
        }
        /// <summary>
        /// So we need to keep them up to date
        /// </summary>
        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PeopleNotEmpty");
            RaisePropertyChanged("Zeneteamvezeto");
            RaisePropertyChanged("Fiuvezeto");
            RaisePropertyChanged("Lanyvezeto");
            RaisePropertyChanged("Kiscsoportok");
        }
        /// <summary>
        /// Set where drops are allowed
        /// </summary>
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = null;
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            if (!(dropInfo.Data is Person))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            var p = (Person)dropInfo.Data;
            if (p.Nem == Nem.Fiu && target.Name == "Lanyvezeto"
             || p.Nem == Nem.Lany && target.Name == "Fiuvezeto"
             || source.Name == "PeopleView" && target.Name != "PeopleView" && target.Name != "AddOrRemovePersonButton"
             || target.Name == "Kiscsoportvezetok" && kiscsoportvezetok.Cast<Person>().Count() >= 14
             || target.Name == "Alvocsoportvezetok" && alvocsoportvezetok.Cast<Person>().Count() >= 14)
            {
                dropInfo.Effects = DragDropEffects.None;
            }
            else if (target.Name.StartsWith("kcs"))
            {
                int kcsn = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                string message = null;
                dropInfo.Effects = (kcsn != p.Kiscsoport && Algorithm.Conflicts(p, kcsn, out message)) ? DragDropEffects.None : DragDropEffects.Move;
                StatusText = message;
            }
            else if (target.Name.Contains("kcs") && p.Kiscsoportvezeto)
            {
                dropInfo.Effects = DragDropEffects.None;
                StatusText = "A kiscsoportvezetők nem mozgathatók!";
            }
            else if (target.Name.Contains("acs") && p.Alvocsoportvezeto)
            {
                dropInfo.Effects = DragDropEffects.None;
                StatusText = "Az alvócsoportvezetők nem mozgathatók!";
            }
            else
                dropInfo.Effects = DragDropEffects.Move;            
        }
        /// <summary>
        /// Make the necessary data changes upon drop
        /// </summary>
        public void Drop(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            Person p = (Person)dropInfo.Data;
            switch (target.Name)
            {
                case "Fiuk": p.Nem = Nem.Fiu; break;
                case "Lanyok": p.Nem = Nem.Lany; break;
                case "Nullnemuek": p.Nem = Nem.Undefined; break;
                case "Ujoncok": p.Type = PersonType.Ujonc; break;
                case "Zeneteamvezeto": Zeneteamvezeto = p; break;
                case "Lanyvezeto": Lanyvezeto = p; break;
                case "Fiuvezeto": Fiuvezeto = p; break;
                case "Egyeb": p.Type = PersonType.Egyeb; break;

                case "Team":
                    if (source.Name != "Kiscsoportvezetok" && source.Name != "Alvocsoportvezetok")
                        p.Type = PersonType.Teamtag;
                    break;
                case "Zeneteam":
                    if (p.Type != PersonType.Fiuvezeto && p.Type != PersonType.Lanyvezeto)
                        p.Type = PersonType.Zeneteamtag;
                    break;
                case "Kiscsoportvezetok":
                    if (!p.Kiscsoportvezeto)
                    {
                        p.Kiscsoportvezeto = true;
                        p.Kiscsoport = Kiscsoportvezetok.Cast<Person>().Count();
                    }
                    break;
                case "Alvocsoportvezetok":
                    if (!p.Alvocsoportvezeto)
                    {
                        p.Alvocsoportvezeto = true;
                        p.Alvocsoport = Alvocsoportvezetok.Cast<Person>().Count();
                    }
                    break;
                case "AddOrRemovePersonButton":
                    People.Remove(p);
                    break;
            }
            if (source.Name == "Kiscsoportvezetok" && (target.Name == "Team" || target.Name == "Ujoncok" || target.Name == "Egyeb"))
            {
                p.Kiscsoportvezeto = false;
                int numKiscsoportok = kiscsoportvezetok.Cast<Person>().Count();
                SwapKiscsoports(p.Kiscsoport, numKiscsoportok - 1);
                foreach (Person q in Kiscsoport(numKiscsoportok - 1))
                    q.Kiscsoport = -1;
            }
            if (source.Name == "Alvocsoportvezetok" && (target.Name == "Team" || target.Name == "Ujoncok" || target.Name == "Egyeb"))
            {
                p.Alvocsoportvezeto = false;
                int numAlvocsoportok = alvocsoportvezetok.Cast<Person>().Count();
                SwapAlvocsoports(p.Alvocsoport, numAlvocsoportok - 1);
                foreach (Person q in Alvocsoport(numAlvocsoportok - 1))
                    q.Alvocsoport = -1;
            }
            if (target.Name.StartsWith("kcs"))
                p.Kiscsoport = Int32.Parse(target.Name.Remove(0, 3)) - 1;
            if (target.Name.StartsWith("acs"))
                p.Alvocsoport = Int32.Parse(target.Name.Remove(0, 3)) - 1;
            if (target.Name == "nokcs")
                p.Kiscsoport = -1;
            if (target.Name == "noacs")
                p.Alvocsoport = -1;
            if (target.Name == "Ujoncok" || target.Name == "Egyeb")
            {
                RaisePropertyChanged("Fiuvezeto");
                RaisePropertyChanged("Lanyvezeto");
                RaisePropertyChanged("Zeneteamvezeto");
            }

            ExtraDropCases(source, target, p);
        }
        
        private ObservableCollection2<Person> people;
        public ObservableCollection2<Person> People
        {
            get
            {
                if (people == null)
                {
                    people = new ObservableCollection2<Person>();
                    people.CollectionChanged += People_CollectionChanged;
                }
                return people;
            }
            private set
            {
                people = value;
                RaisePropertyChanged();
                RaisePropertyChanged("PeopleNotEmpty");
            }
        }
        private volatile bool kiscsoportInited = false;

        /// <summary>
        /// This method is called when the kiscsoportbeoszto tab is opened and all conditions have been met.
        /// </summary>
        internal void InitKiscsoport()
        {
            if (kiscsoportInited)
                return;
            kiscsoportok = Enumerable.Range(0, 15).Select(i => KiscsoportCollectionView(i)).ToList();
            
            nokiscsoport = KiscsoportCollectionView(-1);
            nokiscsoport.CollectionChanged -= EmptyEventHandler;
            nokiscsoport.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");

            kiscsoportInited = true;
            RaisePropertyChanged("Kiscsoportok");
            RaisePropertyChanged("NoKiscsoport");
        }

        private volatile bool alvocsoportInited = false;

        /// <summary>
        /// This method is called when the alvocsoportbeoszto tab is successfully opened and all conditions have been met.
        /// </summary>
        internal void InitAlvocsoport()
        {
            if (alvocsoportInited)
                return;
            alvocsoportok = Enumerable.Range(0, 15).Select(i => AlvocsoportCollectionView(i)).ToList();

            noalvocsoport = AlvocsoportCollectionView(-1);
            noalvocsoport.CollectionChanged -= EmptyEventHandler;
            noalvocsoport.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");

            alvocsoportInited = true;
            RaisePropertyChanged("Alvocsoportok");
            RaisePropertyChanged("NoAlvocsoport");
        }

        private ICollectionView fiuk, lanyok, nullnemuek, ujoncok, team, zeneteam, kiscsoportvezetok, alvocsoportvezetok, egyeb, csoportokbaosztando, nokiscsoport, noalvocsoport;
        public ICollectionView Fiuk
        {
            get
            {
                if (fiuk == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Nem" } };
                    cvs.View.Filter = p => ((Person)p).Nem == Nem.Fiu;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    fiuk = cvs.View;
                }
                return fiuk;
            }
        }
        public ICollectionView Lanyok
        {
            get
            {
                if (lanyok == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Nem" } };
                    cvs.View.Filter = p => ((Person)p).Nem == Nem.Lany;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    lanyok = cvs.View;
                }
                return lanyok;
            }
        }
        public ICollectionView Nullnemuek
        {
            get
            {
                if (nullnemuek == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Nem", "Type" } };
                    cvs.View.Filter = p => ((Person)p).Nem == Nem.Undefined && ((Person)p).Type != PersonType.Egyeb;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    nullnemuek = cvs.View;
                }
                return nullnemuek;
            }
        }
        public ICollectionView Ujoncok
        {
            get
            {
                if (ujoncok == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                    cvs.View.Filter = p => ((Person)p).Type == PersonType.Ujonc;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    ujoncok = cvs.View;
                }
                return ujoncok;
            }
        }     
        public ICollectionView Team
        {
            get
            {
                if (team == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                    cvs.View.Filter = p => ((Person)p).Type != PersonType.Egyeb && ((Person)p).Type != PersonType.Ujonc;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    team = cvs.View;
                }
                return team;
            }
        }
        public ICollectionView Egyeb
        {
            get
            {
                if (egyeb == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                    cvs.View.Filter = p => ((Person)p).Type == PersonType.Egyeb;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    egyeb = cvs.View;
                }
                return egyeb;
            }
        }
        public ICollectionView Kiscsoportvezetok
        {
            get
            {
                if (kiscsoportvezetok == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Kiscsoportvezeto" } };
                    cvs.View.Filter = p => ((Person)p).Kiscsoportvezeto;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    kiscsoportvezetok = cvs.View;
                }
                return kiscsoportvezetok;
            }
        }
        public ICollectionView Alvocsoportvezetok
        {
            get
            {
                if (alvocsoportvezetok == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Alvocsoportvezeto" } };
                    cvs.View.Filter = p => ((Person)p).Alvocsoportvezeto;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    alvocsoportvezetok = cvs.View;
                }
                return alvocsoportvezetok;
            }
        }
        public ICollectionView CsoportokbaOsztando
        {
            get
            {
                if (csoportokbaosztando == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                    cvs.View.Filter = p => ((Person)p).Type != PersonType.Egyeb;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    csoportokbaosztando = cvs.View;
                }
                return csoportokbaosztando;
            }
        }
        public ICollectionView Zeneteam
        {
            get
            {
                if (zeneteam == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                    cvs.View.Filter = p => ((Person)p).Type == PersonType.Zeneteamtag;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    zeneteam = cvs.View;
                }
                return zeneteam;
            }
        }
        public Person Zeneteamvezeto
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.Zeneteamvezeto);
            }
            set
            {
                if (Zeneteamvezeto != null)
                    Zeneteamvezeto.Type = PersonType.Teamtag;
                value.Type = PersonType.Zeneteamvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged("Fiuvezeto");
                RaisePropertyChanged("Lanyvezeto");
            }
        }
        public Person Fiuvezeto
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.Fiuvezeto);
            }
            set
            {
                if (Fiuvezeto != null)
                    Fiuvezeto.Type = PersonType.Teamtag;
                value.Type = PersonType.Fiuvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged("Zeneteamvezeto");
            }
        }
        public Person Lanyvezeto
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.Lanyvezeto);
            }
            set
            {
                if (Lanyvezeto != null)
                    Lanyvezeto.Type = PersonType.Teamtag;
                value.Type = PersonType.Lanyvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged("Zeneteamvezeto");
            }
        }
        private List<ICollectionView> kiscsoportok, alvocsoportok;
        private ICollectionView KiscsoportCollectionView(int i)
        {
            CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Kiscsoport", "Type" } };
            cvs.View.Filter = p => ((Person)p).Kiscsoport == i && ((Person)p).Type != PersonType.Egyeb;
            cvs.View.CollectionChanged += EmptyEventHandler;
            return cvs.View;
        }
        private ICollectionView AlvocsoportCollectionView(int i)
        {
            CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Alvocsoport", "Type" } };
            cvs.View.Filter = p => ((Person)p).Alvocsoport == i && ((Person)p).Type != PersonType.Egyeb;
            cvs.View.CollectionChanged += EmptyEventHandler;
            return cvs.View;
        }
        public List<ICollectionView> Kiscsoportok
        {
            get { return kiscsoportok; }
        }
        public List<ICollectionView> Alvocsoportok
        {
            get { return alvocsoportok; }
        }
        public IEnumerable<Person> Kiscsoport(int i)
        {
            return People.Where(p => p.Type != PersonType.Egyeb && p.Kiscsoport == i);
        }
        public IEnumerable<Person> Alvocsoport(int i)
        {
            return People.Where(p => p.Type != PersonType.Egyeb && p.Alvocsoport == i);
        }
        public ICollectionView NoKiscsoport { get { return nokiscsoport; } }
        public ICollectionView NoAlvocsoport { get { return noalvocsoport; } }
        private ObservableCollection2<Edge> edges;
        public ObservableCollection2<Edge> Edges
        {
            get { return edges ?? (edges = new ObservableCollection2<Edge>()); }
            private set { edges = value; RaisePropertyChanged(); }
        }
        private Edge edge;
        public Edge Edge
        {
            get { return edge ?? (edge = new Edge()); }
            set { edge = value; RaisePropertyChanged(); }
        }
        private int maxAgeDifference = 8;
        public int MaxAgeDifference
        {
            get { return maxAgeDifference; }
            set { maxAgeDifference = value; RaisePropertyChanged(); }
        }
        public Algorithms Algorithm { get; set; }
        private string statusText = "";
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// Adding this seems to fix a bug (see http://stackoverflow.com/questions/37394151), although I have no idea why
        /// </summary>
        private void EmptyEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        { }

        /// <summary>
        /// Represents groups in which no two persons should get assigned to the same sharing group.
        /// </summary>
        public ObservableCollection2<ObservableCollection2<Person>> MutuallyExclusiveGroups { get; } = new ObservableCollection2<ObservableCollection2<Person>> { new ObservableCollection2<Person>() };

        internal AppData AppData
        {
            get
            {
                return new AppData
                {
                    Persons = People.ToArray(),
                    Edges = Edges.ToArray(),
                    MutuallyExclusiveGroups = MutuallyExclusiveGroups.Select(g => g.ToArray()).ToArray(),
                    Szentendre = Szentendre.ToArray()
                };
            }
            set
            {
                People.AddRange(value.Persons);
                Edges.AddRange(value.Edges);
                // The XML serializer doesn't handle object references, so we replace Person copies with references
                foreach (Edge edge in Edges)
                    for (int i = 0; i < edge.Persons.Count(); i++)
                        edge.Persons[i] = People.Single(p => p.Name == edge.Persons[i].Name);
                foreach (Person person in People)
                    if (person.KinekAzUjonca != null)
                        person.KinekAzUjonca = People.Single(p => p.Name == person.KinekAzUjonca.Name);
                foreach (var group in value.MutuallyExclusiveGroups)
                {
                    var og = new ViewModel.ObservableCollection2<Person>();
                    og.AddRange(group.Select(p => People.Single(q => q.Name == p.Name)));
                    MutuallyExclusiveGroups.Add(og);
                }
                MutuallyExclusiveGroups.RemoveAll(g => !g.Any());
                if (!MutuallyExclusiveGroups.Any())
                    MutuallyExclusiveGroups.Add(new ObservableCollection2<Person>());
                RaisePropertyChanged("MutuallyExclusiveGroups");

                if (WeekendNumber == 20)
                    Szentendre.AddRange(value.Szentendre.Select(p => People.Single(q => q.Name == p.Name)));
            }
        }

        public void SwapKiscsoports(int i, int j)
        {
            Debug.Assert(i != -100);
            Debug.Assert(j != -100);
            if (i == j) return;
            foreach (Person p in Kiscsoport(i).ToList())
                p.Kiscsoport = -100;
            foreach (Person p in Kiscsoport(j).ToList())
                p.Kiscsoport = i;
            foreach (Person p in Kiscsoport(-100).ToList())
                p.Kiscsoport = j;
        }

        public void SwapAlvocsoports(int i, int j)
        {
            Debug.Assert(i != -100);
            Debug.Assert(j != -100);
            if (i == j) return;
            foreach (Person p in Alvocsoport(i).ToList())
                p.Alvocsoport = -100;
            foreach (Person p in Alvocsoport(j).ToList())
                p.Alvocsoport = i;
            foreach (Person p in Alvocsoport(-100).ToList())
                p.Alvocsoport = j;
        }

        #region Extras
        // 20HV: Minden szentendrei újonc mellett legyen szentendrei régenc
        public ObservableCollection2<Person> Szentendre { get; } = new ObservableCollection2<Person>();
        private void ExtraDropCases(FrameworkElement source, FrameworkElement target, Person p)
        {
            if (WeekendNumber != 20)
                return;
            if (target.Name == "Zugliget" || target.Name == "Szentendre")
            {
                Szentendre.Remove(p);
                MutuallyExclusiveGroups[0].Remove(p);
                var list = (ObservableCollection<Person>)((ItemsControl)target).ItemsSource;
                if (!list.Contains(p))
                    list.Add(p);
            }
            if ((source.Name == "Zugliget" || source.Name == "Szentendre") && source != target)
                ((ObservableCollection<Person>)((ItemsControl)source).ItemsSource).Remove(p);
        }
        #endregion
    }
}