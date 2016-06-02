using GongSolutions.Wpf.DragDrop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows;
using System;

namespace AntiBonto.ViewModel
{
    /// <summary>
    /// ObservableCollection with added AddRange support
    /// </summary>
    class ObservableCollection2<T> : ObservableCollection<T>
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
    /// Because this is not an enterprise app, I didn't create the plumbing necessary to have separate ViewModels for each tab.
    /// Instead I dumped all of the application state in the below class.
    /// </summary>
    class MainWindow: ViewModelBase, IDropTarget
    {
        /// <summary>
        /// Some UI elements disable based on these
        /// </summary>
        public bool PeopleNotEmpty
        {
            get { return People.Count() != 0; }
        }
        public bool BeosztasKesz
        {
            get { return !Kiscsoport(-1).Any(); }
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
            var target = (FrameworkElement) dropInfo.VisualTarget;
            if (!(dropInfo.Data is Person))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            var p = (Person)dropInfo.Data;
            if (p.Nem == Nem.Fiu && target.Name == "Lanyvezeto" || p.Nem == Nem.Lany && target.Name == "Fiuvezeto")
                dropInfo.Effects = DragDropEffects.None;
            else if (target.Name.StartsWith("kcs"))
            {
                int kcsn = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                string message = null;
                dropInfo.Effects = (kcsn != p.Kiscsoport && Algorithm.Conflicts(p, kcsn, out message)) ? DragDropEffects.None : DragDropEffects.Move;
                Status = message;
            }
            else if (target.Name.Contains("kcs") && p.Kiscsoportvezeto)
            {
                dropInfo.Effects = DragDropEffects.None;
                Status = "A kiscsoportvezetők nem mozgathatók!";
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
            switch(target.Name)
            {
                case "Fiuk": p.Nem = Nem.Fiu; break;
                case "Lanyok": p.Nem = Nem.Lany; break;
                case "Nullnemuek": p.Nem = Nem.Undefined; break;
                case "Team":
                    if (source.Name != "Kiscsoportvezetok")
                        p.Type = PersonType.Teamtag;
                break;
                case "Zeneteam": if (p.Type != PersonType.Fiuvezeto && p.Type != PersonType.Lanyvezeto) p.Type = PersonType.Zeneteamtag; break;
                case "Ujoncok": p.Type = PersonType.Ujonc; break;
                case "Zeneteamvezeto": Zeneteamvezeto = p; break;
                case "Lanyvezeto": Lanyvezeto = p; break;
                case "Fiuvezeto": Fiuvezeto = p; break;
                case "Kiscsoportvezetok": p.Kiscsoportvezeto = true; break;
                case "Egyeb": p.Type = PersonType.Egyeb; break;             
            }
            if (source.Name == "Kiscsoportvezetok" && (target.Name == "Team" || target.Name == "Ujoncok" || target.Name=="Egyeb"))
                p.Kiscsoportvezeto = false;
            if (target.Name.StartsWith("kcs"))
                p.Kiscsoport = Int32.Parse(target.Name.Remove(0, 3)) - 1;
            if (target.Name == "nokcs")
                p.Kiscsoport = -1;
            if (target.Name == "Ujoncok" || target.Name == "Egyeb")
            {
                RaisePropertyChanged("Fiuvezeto");
                RaisePropertyChanged("Lanyvezeto");
                RaisePropertyChanged("Zeneteamvezeto");
            }
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
        private ICollectionView fiuk, lanyok, nullnemuek, ujoncok, team, zeneteam, kiscsoportvezetok, egyeb, kiscsoportbaosztando, nokiscsoport;
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
        public ICollectionView KiscsoportbaOsztando
        {
            get
            {
                if (kiscsoportbaosztando == null)
                {
                    CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                    cvs.View.Filter = p => ((Person)p).Type != PersonType.Egyeb;
                    cvs.View.CollectionChanged += EmptyEventHandler;
                    kiscsoportbaosztando = cvs.View;
                }
                return kiscsoportbaosztando;
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
        private List<ICollectionView> kiscsoportok;
        private ICollectionView KiscsoportCollectionView(int i)
        {
            CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Kiscsoport", "Type" } };
            cvs.View.Filter = p => ((Person)p).Kiscsoport == i && ((Person)p).Type != PersonType.Egyeb;
            cvs.View.CollectionChanged += EmptyEventHandler;
            return cvs.View;
        }
        public List<ICollectionView> Kiscsoportok
        {
            get { return kiscsoportok; }
        }
        public IEnumerable<Person> Kiscsoport(int i)
        {
            return People.Where(p => p.Type != PersonType.Egyeb && p.Kiscsoport == i);
        }
        public ICollectionView NoKiscsoport { get { return nokiscsoport; } }
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
        private string status = "";
        public string Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// Adding this seems to fix a bug (see http://stackoverflow.com/questions/37394151), although I have no idea why
        /// </summary>
        private void EmptyEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        { }

    }
}