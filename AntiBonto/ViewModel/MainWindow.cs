using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System;
using System.Diagnostics;

namespace AntiBonto.ViewModel
{
    /// <summary>
    /// Because this is not an enterprise app, I didn't create the plumbing necessary to have separate ViewModels for each tab.
    /// Instead I dumped all of the application state in the below class.
    /// </summary>
    public class MainWindow: ViewModelBase
    {
        /// <summary>
        /// Most tabs disable if this is false
        /// </summary>
        public bool PeopleNotEmpty { get { return People.Count() != 0; } }
        
        /// <summary>
        /// The Save button disables if this is false
        /// </summary>
        public bool BeosztasKesz { get { return !Kiscsoport(-1).Any() && !Alvocsoport(-1).Any(); } }

        public static int WeekendNumber { get { return 2 * DateTime.Now.Year - 4013 + DateTime.Now.Month / 7; } }

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PeopleNotEmpty");
            RaisePropertyChanged("Zeneteamvezeto");
            RaisePropertyChanged("Fiuvezeto");
            RaisePropertyChanged("Lanyvezeto");
            RaisePropertyChanged("Kiscsoportok");
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
        private volatile bool kiscsoportInited = false, alvocsoportInited = false;

        /// <summary>
        /// This method is called when the kiscsoportbeoszto tab is opened and all conditions have been met.
        /// </summary>
        internal void InitKiscsoport()
        {
            if (kiscsoportInited)
                return;
            kiscsoportok = Enumerable.Range(0, 15).Select(i => KiscsoportCollectionView(i)).ToList();
            
            nokiscsoport = KiscsoportCollectionView(-1);
            nokiscsoport.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");

            kiscsoportInited = true;
            RaisePropertyChanged("Kiscsoportok");
            RaisePropertyChanged("NoKiscsoport");
        }       

        /// <summary>
        /// This method is called when the alvocsoportbeoszto tab is successfully opened and all conditions have been met.
        /// </summary>
        internal void InitAlvocsoport()
        {
            if (alvocsoportInited)
                return;
            alvocsoportok = Enumerable.Range(0, 15).Select(i => AlvocsoportCollectionView(i)).ToList();

            noalvocsoport = AlvocsoportCollectionView(-1);
            noalvocsoport.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");

            alvocsoportInited = true;
            RaisePropertyChanged("Alvocsoportok");
            RaisePropertyChanged("NoAlvocsoport");
        }

        public ICollectionView Fiuk { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Nem == Nem.Fiu); } }
        public ICollectionView Lanyok { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Nem == Nem.Lany); } }
        public ICollectionView Nullnemuek { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Nem == Nem.Undefined && ((Person)p).Type != PersonType.Egyeb); } }
        public ICollectionView Ujoncok { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Type == PersonType.Ujonc); } }
        public ICollectionView Team { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Type != PersonType.Egyeb && ((Person)p).Type != PersonType.Ujonc); } }
        public ICollectionView Egyeb { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Type == PersonType.Egyeb); } }
        public ICollectionView Kiscsoportvezetok { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Kiscsoportvezeto); } }       
        public ICollectionView Alvocsoportvezetok { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Alvocsoportvezeto); } }
        public ICollectionView CsoportokbaOsztando { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Type != PersonType.Egyeb); } }
        public ICollectionView Zeneteam { get { return CollectionViewHelper.Lazy(People, p => ((Person)p).Type == PersonType.Zeneteamtag); } }
        private ICollectionView KiscsoportCollectionView(int i)
        {
            return CollectionViewHelper.Get(People, p => ((Person)p).Kiscsoport == i && ((Person)p).Type != PersonType.Egyeb);
        }
        private ICollectionView AlvocsoportCollectionView(int i)
        {
            return CollectionViewHelper.Get(People, p => ((Person)p).Alvocsoport == i && ((Person)p).Type != PersonType.Egyeb);
        }
        public IEnumerable<Person> Kiscsoport(int i)
        {
            return People.Where(p => p.Type != PersonType.Egyeb && p.Kiscsoport == i);
        }
        public IEnumerable<Person> Alvocsoport(int i)
        {
            return People.Where(p => p.Type != PersonType.Egyeb && p.Alvocsoport == i);
        }
        private ICollectionView nokiscsoport, noalvocsoport;
        public ICollectionView NoKiscsoport { get { return nokiscsoport; } }
        public ICollectionView NoAlvocsoport { get { return noalvocsoport; } }

        private List<ICollectionView> kiscsoportok, alvocsoportok;
        public List<ICollectionView> Kiscsoportok { get { return kiscsoportok; } }
        public List<ICollectionView> Alvocsoportok { get { return alvocsoportok; } }
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
        public ObservableCollection2<Person> Szentendre { get; } = new ObservableCollection2<Person>();
        #endregion
    }
}