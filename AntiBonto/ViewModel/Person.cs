using AntiBonto.ViewModel;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AntiBonto
{
    /// <summary>
    /// A Kiss Laci féle "Hétvége kezelő" Excel+VBA által használt kódolás
    /// </summary>
    public enum PersonType
    {
        Team = 0,
        BoyLeader = 1,
        GirlLeader = 2,
        MusicLeader = 3,
        MusicTeam = 4,
        Others = 10,
        Newcomer = 11
    }
    public enum Sex
    {
        Girl, Boy, Undefined
    }

    [Serializable]
    public class Person: ViewModelBase
    {
        public string Name { get; set; }
        public string Nickname { get; set; }

        private bool pinned = false;
        public bool Pinned
        {
            get { return pinned; }
            set
            {
                pinned = value;
                RaisePropertyChanged();
                foreach (Person p in includeEdges)
                    if (p.Pinned != value)
                        p.Pinned = value;
            }
        }


        private int birthYear = DateTime.Now.Year;
        public int BirthYear
        {
            get { return birthYear; }
            set { birthYear = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(Age)); }
        }
        [XmlIgnore]
        public int Age
        {
            get { return DateTime.Now.Year - BirthYear; }
            set { BirthYear = DateTime.Now.Year - value; RaisePropertyChanged(); RaisePropertyChanged(nameof(BirthYear)); }
        }
        private Sex sex = Sex.Undefined;
        public Sex Sex
        {
            get { return sex; }
            set { sex = value; RaisePropertyChanged(); }
        }
        private PersonType type;
        public PersonType Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged(); }
        }
        private bool sharinggroupleader = false, sleepinggroupleader = false;
        public bool SharingGroupLeader
        {
            get { return sharinggroupleader; }
            set
            {
                sharinggroupleader = value;
                RaisePropertyChanged();
                if (value && (Type == PersonType.Newcomer || Type == PersonType.Others))
                    Type = PersonType.Team;
            }
        }
        public bool SleepingGroupLeader
        {
            get { return sleepinggroupleader; }
            set
            {
                sleepinggroupleader = value;
                RaisePropertyChanged();
                if (value && (Type == PersonType.Newcomer || Type == PersonType.Others))
                    Type = PersonType.Team;
            }
        }
        private int sharingGroup = -1;
        
        /// <summary>Zero-based</summary>
        public int SharingGroup
        {
            get { return sharingGroup; }
            set { sharingGroup = value; RaisePropertyChanged(); }
        }
        private int sleepingGroup = -1;

        /// <summary>Zero-based</summary>
        public int SleepingGroup
        {
            get { return sleepingGroup; }
            set { sleepingGroup = value; RaisePropertyChanged(); }
        }
        public override string ToString()
        {
            return Name;
        }
        private Person whoseNewcomer;
        public Person WhoseNewcomer
        {
            get { return whoseNewcomer; }
            set { whoseNewcomer = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// These will be filled out by <see cref="Algorithms.ConvertEdges"/> 
        /// </summary>
        internal HashSet<Person> includeEdges = new(), excludeEdges = new();
        
        /// <summary>
        /// Traverse the graphs defined by kivelIgen and kivelNem.
        /// Collect the transitively related nodes into these sets so that no further recursive traversal is needed during the algorithm.
        /// </summary>
        internal void CollectRecursiveEdges()
        {
            var visitedSet = new HashSet<Person>();
            var queue = new Queue<Person>();
            foreach (Person p in includeEdges)
                queue.Enqueue(p);            
            while (queue.Count > 0)
            {
                Person p = queue.Dequeue();
                includeEdges.Add(p);
                visitedSet.Add(p);
                foreach (Person q in p.includeEdges)
                    if (!visitedSet.Contains(q))
                        queue.Enqueue(q);
                foreach (Person q in p.excludeEdges)
                    excludeEdges.Add(q);
            }
        }
    }
}
