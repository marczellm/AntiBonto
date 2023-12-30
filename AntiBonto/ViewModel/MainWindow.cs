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
        public bool AssignmentsComplete => !SharingGroup(-1).Any() && !SleepingGroup(-1).Any();

        private bool magicAllowed = false;
        private bool magicPossible = false;
        public bool MagicAllowed  { get { return magicAllowed; }  set { magicAllowed = value;  RaisePropertyChanged(nameof(MagicEnabled)); } }
        public bool MagicPossible { get { return magicPossible; } set { magicPossible = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(MagicEnabled)); } }
        public bool MagicEnabled => MagicAllowed && MagicPossible;

        public static int WeekendNumber => 2 * DateTime.Now.Year - 4013 + DateTime.Now.Month / 7;

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(PeopleNotEmpty));
            RaisePropertyChanged(nameof(MusicLeader));
            RaisePropertyChanged(nameof(BoyLeader));
            RaisePropertyChanged(nameof(GirlLeader));
            RaisePropertyChanged(nameof(SharingGroups));
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
        /// This method is called when the sharing groups tab is opened and all conditions have been met.
        /// </summary>
        internal void InitSharingGroups()
        {
            sharingGroups = SharingGroupLeaders.Select((v, i) => SharingGroupCollectionView(i)).ToList();

            RaisePropertyChanged(nameof(SharingGroups));
            RaisePropertyChanged(nameof(SharingGroupless));
        }       

        /// <summary>
        /// This method is called when the sleeping groups tab is opened and all conditions have been met.
        /// </summary>
        internal void InitSleepingGroups()
        {
            // Renumber sleeping groups: girls first
            int i = 0;
            foreach (var person in SleepingGroupLeaders.Where(p => p.Sex == Sex.Girl))
            {
                if (person.SleepingGroup != i)
                {
                    SwapSleepingGroups(person.SleepingGroup, i);
                }
                i++;
            }
            foreach (var person in SleepingGroupLeaders.Where(p => p.Sex == Sex.Boy))
            {
                if (person.SleepingGroup != i)
                {
                    SwapSleepingGroups(person.SleepingGroup, i);
                }
                i++;
            }

            sleepingGroups = SleepingGroupLeaders.Select((v, i) => SleepingGroupCollectionView(i)).ToList();
            boySleepingGroups = SleepingGroupLeaders.Where(item => item.Sex == Sex.Boy).Select(item => SleepingGroupCollectionView(item.SleepingGroup)).ToList();
            girlSleepingGroups = SleepingGroupLeaders.Where(item => item.Sex == Sex.Girl).Select(item => SleepingGroupCollectionView(item.SleepingGroup)).ToList();

            RaisePropertyChanged(nameof(SleepingGroups));
            RaisePropertyChanged(nameof(BoySleepingGroups));
            RaisePropertyChanged(nameof(GirlSleepingGroups));
            RaisePropertyChanged(nameof(SleepingGrouplessBoys));
            RaisePropertyChanged(nameof(SleepingGrouplessGirls));
        }

        /// <summary>
        /// Renumbers the share groups so that the weekend leaders and the music team leader are in the groups with the highest number
        /// </summary>
        internal void SharingGroupExportOrdering()
        {
            int l = GirlLeader.SharingGroup, f = BoyLeader.SharingGroup, z = MusicLeader.SharingGroup, m = SharingGroupLeaders.Count();
            SwapSharingGroups(GirlLeader.SharingGroup, m - 1);
            if (f != l)
                SwapSharingGroups(BoyLeader.SharingGroup, m - 2);
            if (z != l && z != f)
                SwapSharingGroups(MusicLeader.SharingGroup, m - 3);
        }

        public ICollectionView Boys => CollectionViewHelper.Lazy<Person>(People, p => p.Sex == Sex.Boy && p.Type != PersonType.Others);
        public ICollectionView Girls => CollectionViewHelper.Lazy<Person>(People, p => p.Sex == Sex.Girl && p.Type != PersonType.Others);
        public ICollectionView SexUndefined
        {
            get
            {
                var ret = CollectionViewHelper.Lazy<Person>(People, p => p.Sex == Sex.Undefined && p.Type != PersonType.Others);
                ret.CollectionChanged += (sender, e) => ret.MoveCurrentToFirst();
                ret.MoveCurrentToFirst();
                return ret;
            }
        }

        private readonly SortDescription orderByName = new("Name", ListSortDirection.Ascending);

        public ICollectionView Newcomers => CollectionViewHelper.Lazy<Person>(People, p => p.Type == PersonType.Newcomer, orderByName);
        public ICollectionView Team => CollectionViewHelper.Lazy<Person>(People, p => p.Type != PersonType.Others && p.Type != PersonType.Newcomer, orderByName);
        public ICollectionView Others => CollectionViewHelper.Lazy<Person>(People, p => p.Type == PersonType.Others, orderByName);

        public ICollectionView SharingGroupLeadersCollectionView => CollectionViewHelper.Lazy<Person>(People, p => p.SharingGroupLeader);
        public ICollectionView SleepingGroupLeadersCollectionView => CollectionViewHelper.Lazy<Person>(People, p => p.SleepingGroupLeader);
        public IEnumerable<Person> SharingGroupLeaders => SharingGroupLeadersCollectionView.Cast<Person>();
        public IEnumerable<Person> SleepingGroupLeaders => SleepingGroupLeadersCollectionView.Cast<Person>();
        public ICollectionView PeopleToAssign => CollectionViewHelper.Lazy<Person>(People, p => p.Type != PersonType.Others, orderByName);
        public ICollectionView MusicTeam => CollectionViewHelper.Lazy<Person>(People, p => p.Type == PersonType.MusicTeam);
        private ICollectionView SharingGroupCollectionView(int i)
        {
            return CollectionViewHelper.Get<Person>(People, p => p.SharingGroup == i && p.Type != PersonType.Others, new SortDescription(nameof(Person.SharingGroupLeader), ListSortDirection.Descending));
        }
        private ICollectionView SleepingGroupCollectionView(int i)
        {
            return CollectionViewHelper.Get<Person>(People, p => p.SleepingGroup == i && p.Type != PersonType.Others, new SortDescription(nameof(Person.SleepingGroupLeader), ListSortDirection.Descending));
        }
        public IEnumerable<Person> SharingGroup(int i)
        {
            return People.Where(p => p.Type != PersonType.Others && p.SharingGroup == i);
        }
        public IEnumerable<Person> SleepingGroup(int i)
        {
            return People.Where(p => p.Type != PersonType.Others && p.SleepingGroup == i);
        }
        public ICollectionView SharingGroupless => CollectionViewHelper.Lazy<Person>(People, p => p.SharingGroup == -1 && p.Type != PersonType.Others);
        public ICollectionView SleepingGrouplessBoys => CollectionViewHelper.Lazy<Person>(People, p => p.SleepingGroup == -1 && p.Type != PersonType.Others && p.Sex == Sex.Boy);
        public ICollectionView SleepingGrouplessGirls => CollectionViewHelper.Lazy<Person>(People, p => p.SleepingGroup == -1 && p.Type != PersonType.Others && p.Sex == Sex.Girl);

        private List<ICollectionView> sharingGroups, sleepingGroups, boySleepingGroups, girlSleepingGroups;
        public List<ICollectionView> SharingGroups => sharingGroups;
        public List<ICollectionView> SleepingGroups => sleepingGroups;
        public List<ICollectionView> BoySleepingGroups => boySleepingGroups;
        public List<ICollectionView> GirlSleepingGroups => girlSleepingGroups;

        public Person MusicLeader
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.MusicLeader);
            }
            set
            {
                if (MusicLeader != null)
                    MusicLeader.Type = PersonType.Team;
                if (value != null)
                    value.Type = PersonType.MusicLeader;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(BoyLeader));
                RaisePropertyChanged(nameof(GirlLeader));
            }
        }
        public Person BoyLeader
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.BoyLeader);
            }
            set
            {
                if (BoyLeader != null)
                    BoyLeader.Type = PersonType.Team;
                if (value != null)
                    value.Type = PersonType.BoyLeader;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(MusicLeader));
            }
        }
        public Person GirlLeader
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.GirlLeader);
            }
            set
            {
                if (GirlLeader != null)
                    GirlLeader.Type = PersonType.Team;
                if (value != null)
                    value.Type = PersonType.GirlLeader;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(MusicLeader));
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

        public MainWindow()
        {
            SharingGroupless.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(AssignmentsComplete));
            SleepingGrouplessBoys.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(AssignmentsComplete));
            SleepingGrouplessGirls.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(AssignmentsComplete));
        }

        private string statusText = "";
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Represents groups in which no two persons should get assigned to the same sharing group.
        /// </summary>
        public ObservableCollection2<ObservableCollection2<Person>> MutuallyExclusiveGroups { get; } = new ObservableCollection2<ObservableCollection2<Person>> { new() };

        internal AppData AppData
        {
            get
            {
                return new AppData
                {
                    Persons = People.ToArray(),
                    Edges = Edges.ToArray(),
                    MutuallyExclusiveGroups = MutuallyExclusiveGroups.Select(g => g.ToArray()).ToArray()
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
                    if (person.WhoseNewcomer != null)
                        person.WhoseNewcomer = People.Single(p => p.Name == person.WhoseNewcomer.Name);
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
            }
        }

        public void SwapSharingGroups(int i, int j)
        {
            Debug.Assert(i != -100);
            Debug.Assert(j != -100);
            if (i == j) return;
            foreach (Person p in SharingGroup(i).ToList())
                p.SharingGroup = -100;
            foreach (Person p in SharingGroup(j).ToList())
                p.SharingGroup = i;
            foreach (Person p in SharingGroup(-100).ToList())
                p.SharingGroup = j;
        }

        public void SwapSleepingGroups(int i, int j)
        {
            Debug.Assert(i != -100);
            Debug.Assert(j != -100);
            if (i == j) return;
            foreach (Person p in SleepingGroup(i).ToList())
                p.SleepingGroup = -100;
            foreach (Person p in SleepingGroup(j).ToList())
                p.SleepingGroup = i;
            foreach (Person p in SleepingGroup(-100).ToList())
                p.SleepingGroup = j;
        }

        public DragOverCallback DragOver_AlwaysAllow => (person, source, target) => new() { effect = DragDropEffects.Move };
        public DragOverCallback Newcomers_DragOver => (person, source, target) =>
        {
            if (person.Type == PersonType.BoyLeader || person.Type == PersonType.GirlLeader || person.Type == PersonType.MusicLeader)
            {
                return new()
                {
                    effect = DragDropEffects.None
                };
            }
            return new() { effect = DragDropEffects.Move };
        };
        public DragOverCallback SharingGroupless_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.SharingGroupLeader)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "A kiscsoportvezetők nem mozgathatók!"
                };
            }

            return new() { effect = DragDropEffects.Move };
        };
        public DragOverCallback SharingGroup_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.SharingGroupLeader)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "A kiscsoportvezetők nem mozgathatók!"
                };
            }
            else
            {
                int kcsn = SharingGroups.IndexOf((target as DnDItemsControl).ItemsSource as ICollectionView);
                string message = null;

                var ret = new DragOverResult
                {
                    effect = (kcsn == person.SharingGroup || Algorithm.Conflicts(person, kcsn, out message)) ? DragDropEffects.None : DragDropEffects.Move,
                    message = message
                };
                return ret;
            }
        };
        public DragOverCallback SleepingGroupless_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.SleepingGroupLeader)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "Az alvócsoportvezetők nem mozgathatók!"
                };
            }

            return new() { effect = DragDropEffects.Move };
        };
        public DragOverCallback SleepingGroup_DragOver => (person, source, target) =>
        {
            if (person.Pinned)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = person + " le van rögzítve!"
                };
            }
            else if (person.SleepingGroupLeader)
            {
                return new()
                {
                    effect = DragDropEffects.None,
                    message = "A kiscsoportvezetők nem mozgathatók!"
                };
            }
            else
            {
                int acsn = ((target as DnDItemsControl).ItemsSource as CollectionView).Cast<Person>().First().SleepingGroup;
                var acsvez = SleepingGroupLeaders.Single(q => q.SleepingGroup == acsn);
                return new()
                {
                    effect = (person.Sex != Sex.Undefined && person.Sex != acsvez.Sex) ? DragDropEffects.None : DragDropEffects.Move
                };
            }
        };

    }
}