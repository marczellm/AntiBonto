using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System;
using System.Diagnostics;
using System.Windows;
using AntiBonto.View;
using System.Windows.Data;

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
        public bool PeopleNotEmpty => People.Count != 0;

        /// <summary>
        /// The Save button disables if this is false
        /// </summary>
        public bool BeosztasKesz => !Kiscsoport(-1).Any() && !Alvocsoport(-1).Any();

        private bool magicAllowed = false;
        private bool magicPossible = false;
        public bool MagicAllowed  { get { return magicAllowed; }  set { magicAllowed = value;  RaisePropertyChanged(nameof(MagicEnabled)); } }
        public bool MagicPossible { get { return magicPossible; } set { magicPossible = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(MagicEnabled)); } }
        public bool MagicEnabled => MagicAllowed && MagicPossible;

        public static int WeekendNumber => 2 * DateTime.Now.Year - 4013 + DateTime.Now.Month / 7;

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(PeopleNotEmpty));
            RaisePropertyChanged(nameof(Zeneteamvezeto));
            RaisePropertyChanged(nameof(Fiuvezeto));
            RaisePropertyChanged(nameof(Lanyvezeto));
            RaisePropertyChanged(nameof(Kiscsoportok));
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
                RaisePropertyChanged(nameof(PeopleNotEmpty));
            }
        }

        /// <summary>
        /// This method is called when the kiscsoportbeoszto tab is opened and all conditions have been met.
        /// </summary>
        internal void InitKiscsoport()
        {
            kiscsoportok = Enumerable.Range(0, Kiscsoportvezetok.Count()).Select(i => KiscsoportCollectionView(i)).ToList();

            RaisePropertyChanged(nameof(Kiscsoportok));
            RaisePropertyChanged(nameof(NoKiscsoport));
        }       

        /// <summary>
        /// This method is called when the alvocsoportbeoszto tab is opened and all conditions have been met.
        /// </summary>
        internal void InitAlvocsoport()
        {
            for (int i = 0; i < Alvocsoportvezetok.Count(); i++)
            {
                var vez = Alvocsoportvezetok.ElementAt(i);
                int prevNum = vez.Alvocsoport;
                foreach (var tag in AlvocsoportCollectionView(prevNum).Cast<Person>()) 
                {
                    tag.Alvocsoport = i;
                }
            }
            alvocsoportok = Alvocsoportvezetok.Select((v, i) => AlvocsoportCollectionView(i)).ToList();
            alvocsoportokFiu = Alvocsoportvezetok.Select((leader, index) => new { leader, index }).Where(item => item.leader.Nem == Nem.Fiu).Select(item => AlvocsoportCollectionView(item.index)).ToList();
            alvocsoportokLany = Alvocsoportvezetok.Select((leader, index) => new { leader, index }).Where(item => item.leader.Nem == Nem.Lany).Select(item => AlvocsoportCollectionView(item.index)).ToList();

            RaisePropertyChanged(nameof(Alvocsoportok));
            RaisePropertyChanged(nameof(AlvocsoportokFiu));
            RaisePropertyChanged(nameof(AlvocsoportokLany));
            RaisePropertyChanged(nameof(NoAlvocsoportFiu));
            RaisePropertyChanged(nameof(NoAlvocsoportLany));
        }

        /// <summary>
        /// Renumbers the share groups so that the weekend leaders and the music team leader are in the groups with the highest number
        /// </summary>
        internal void KiscsoportExportOrdering()
        {
            int l = Lanyvezeto.Kiscsoport, f = Fiuvezeto.Kiscsoport, z = Zeneteamvezeto.Kiscsoport, m = Kiscsoportvezetok.Count();
            SwapKiscsoports(Lanyvezeto.Kiscsoport, m - 1);
            if (f != l)
                SwapKiscsoports(Fiuvezeto.Kiscsoport, m - 2);
            if (z != l && z != f)
                SwapKiscsoports(Zeneteamvezeto.Kiscsoport, m - 3);
        }

        public ICollectionView Fiuk => CollectionViewHelper.Lazy<Person>(People, p => p.Nem == Nem.Fiu && p.Type != PersonType.Egyeb);
        public ICollectionView Lanyok => CollectionViewHelper.Lazy<Person>(People, p => p.Nem == Nem.Lany && p.Type != PersonType.Egyeb);
        public ICollectionView Nullnemuek
        {
            get
            {
                var ret = CollectionViewHelper.Lazy<Person>(People, p => p.Nem == Nem.Undefined && p.Type != PersonType.Egyeb);
                ret.CollectionChanged += (sender, e) => ret.MoveCurrentToFirst();
                ret.MoveCurrentToFirst();
                return ret;
            }
        }

        private readonly SortDescription orderByName = new("Name", ListSortDirection.Ascending);

        public ICollectionView Ujoncok => CollectionViewHelper.Lazy<Person>(People, p => p.Type == PersonType.Ujonc, orderByName);
        public ICollectionView Team => CollectionViewHelper.Lazy<Person>(People, p => p.Type != PersonType.Egyeb && p.Type != PersonType.Ujonc, orderByName);
        public ICollectionView Egyeb => CollectionViewHelper.Lazy<Person>(People, p => p.Type == PersonType.Egyeb, orderByName);

        public ICollectionView KiscsoportvezetokCollectionView => CollectionViewHelper.Lazy<Person>(People, p => p.Kiscsoportvezeto);
        public ICollectionView AlvocsoportvezetokCollectionView => CollectionViewHelper.Lazy<Person>(People, p => p.Alvocsoportvezeto);
        public IEnumerable<Person> Kiscsoportvezetok => KiscsoportvezetokCollectionView.Cast<Person>();
        public IEnumerable<Person> Alvocsoportvezetok => AlvocsoportvezetokCollectionView.Cast<Person>();
        public ICollectionView CsoportokbaOsztando => CollectionViewHelper.Lazy<Person>(People, p => p.Type != PersonType.Egyeb, orderByName);
        public ICollectionView Zeneteam => CollectionViewHelper.Lazy<Person>(People, p => p.Type == PersonType.Zeneteamtag);
        private ICollectionView KiscsoportCollectionView(int i)
        {
            return CollectionViewHelper.Get<Person>(People, p => p.Kiscsoport == i && p.Type != PersonType.Egyeb, new SortDescription(nameof(Person.Kiscsoportvezeto), ListSortDirection.Descending));
        }
        private ICollectionView AlvocsoportCollectionView(int i)
        {
            return CollectionViewHelper.Get<Person>(People, p => p.Alvocsoport == i && p.Type != PersonType.Egyeb, new SortDescription(nameof(Person.Alvocsoportvezeto), ListSortDirection.Descending));
        }
        public IEnumerable<Person> Kiscsoport(int i)
        {
            return People.Where(p => p.Type != PersonType.Egyeb && p.Kiscsoport == i);
        }
        public IEnumerable<Person> Alvocsoport(int i)
        {
            return People.Where(p => p.Type != PersonType.Egyeb && p.Alvocsoport == i);
        }
        public ICollectionView NoKiscsoport => CollectionViewHelper.Lazy<Person>(People, p => p.Kiscsoport == -1 && p.Type != PersonType.Egyeb);
        public ICollectionView NoAlvocsoportFiu => CollectionViewHelper.Lazy<Person>(People, p => p.Alvocsoport == -1 && p.Type != PersonType.Egyeb && p.Nem == Nem.Fiu);
        public ICollectionView NoAlvocsoportLany => CollectionViewHelper.Lazy<Person>(People, p => p.Alvocsoport == -1 && p.Type != PersonType.Egyeb && p.Nem == Nem.Lany);

        private List<ICollectionView> kiscsoportok, alvocsoportok, alvocsoportokFiu, alvocsoportokLany;
        public List<ICollectionView> Kiscsoportok => kiscsoportok;
        public List<ICollectionView> Alvocsoportok => alvocsoportok;
        public List<ICollectionView> AlvocsoportokFiu => alvocsoportokFiu;
        public List<ICollectionView> AlvocsoportokLany => alvocsoportokLany;

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
                if (value != null)
                    value.Type = PersonType.Zeneteamvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Fiuvezeto));
                RaisePropertyChanged(nameof(Lanyvezeto));
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
                if (value != null)
                    value.Type = PersonType.Fiuvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Zeneteamvezeto));
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
                if (value != null)
                    value.Type = PersonType.Lanyvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Zeneteamvezeto));
            }
        }
        
        private ObservableCollection2<Edge> edges;
        public ObservableCollection2<Edge> Edges
        {
            get { return edges ??= new ObservableCollection2<Edge>(); }
            private set { edges = value; RaisePropertyChanged(); }
        }
        private Edge edge;
        public Edge Edge
        {
            get { return edge ??= new Edge(); }
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

        public MainWindow()
        {
            NoKiscsoport.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(BeosztasKesz));
            NoAlvocsoportFiu.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(BeosztasKesz));
            NoAlvocsoportLany.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(BeosztasKesz));
        }

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
                    for (int i = 0; i < edge.Persons.Length; i++)
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
                RaisePropertyChanged(nameof(MutuallyExclusiveGroups));

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

            //(alvocsoportNevek[j], alvocsoportNevek[i]) = (alvocsoportNevek[i], alvocsoportNevek[j]);
        }

        public DragOverCallback DragOver_AlwaysAllow => (person, source, target) => new() { effect = DragDropEffects.Move };
        public DragOverCallback Ujoncok_DragOver => (person, source, target) =>
        {
            if (person.Type == PersonType.Fiuvezeto || person.Type == PersonType.Lanyvezeto || person.Type == PersonType.Zeneteamvezeto)
            {
                return new()
                {
                    effect = DragDropEffects.None
                };
            }
            return new() { effect = DragDropEffects.Move };
        };
        public DragOverCallback NoKcs_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.Kiscsoportvezeto)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "A kiscsoportvezetők nem mozgathatók!"
                };
            }

            return new() { effect = DragDropEffects.Move };
        };
        public DragOverCallback Kcs_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.Kiscsoportvezeto)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "A kiscsoportvezetők nem mozgathatók!"
                };
            }
            else
            {
                int kcsn = Kiscsoportok.IndexOf((target as DnDItemsControl).ItemsSource as ICollectionView);
                string message = null;

                var ret = new DragOverResult
                {
                    effect = (kcsn == person.Kiscsoport || Algorithm.Conflicts(person, kcsn, out message)) ? DragDropEffects.None : DragDropEffects.Move,
                    message = message
                };
                return ret;
            }
        };
        public DragOverCallback NoAcs_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.Alvocsoportvezeto)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "Az alvócsoportvezetők nem mozgathatók!"
                };
            }

            return new() { effect = DragDropEffects.Move };
        };
        public DragOverCallback Acs_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.Alvocsoportvezeto)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "A kiscsoportvezetők nem mozgathatók!"
                };
            }
            else
            {
                int acsn = ((target as DnDItemsControl).ItemsSource as CollectionView).Cast<Person>().First().Alvocsoport;
                var acsvez = Alvocsoportvezetok.Single(q => q.Alvocsoport == acsn);
                return new()
                {
                    effect = (person.Nem != Nem.Undefined && person.Nem != acsvez.Nem) ? DragDropEffects.None : DragDropEffects.Move
                };
            }
        };

        #region Extras
        public ObservableCollection2<Person> Szentendre { get; } = new ObservableCollection2<Person>();
        #endregion
    }
}