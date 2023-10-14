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
        Teamtag = 0,
        Fiuvezeto = 1,
        Lanyvezeto = 2,
        Zeneteamvezeto = 3,
        Zeneteamtag = 4,
        Egyeb = 10,
        Ujonc = 11
    }
    public enum Nem
    {
        Lany, Fiu, Undefined
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
                foreach (Person p in kivelIgen)
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
        private Nem nem = Nem.Undefined;
        public Nem Nem
        {
            get { return nem; }
            set { nem = value; RaisePropertyChanged(); }
        }
        private PersonType type;
        public PersonType Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged(); }
        }
        private bool kcsvez = false, acsvez = false;
        public bool Kiscsoportvezeto
        {
            get { return kcsvez; }
            set
            {
                kcsvez = value;
                RaisePropertyChanged();
                if (value && (Type == PersonType.Ujonc || Type == PersonType.Egyeb))
                    Type = PersonType.Teamtag;
            }
        }
        public bool Alvocsoportvezeto
        {
            get { return acsvez; }
            set
            {
                acsvez = value;
                RaisePropertyChanged();
                if (value && (Type == PersonType.Ujonc || Type == PersonType.Egyeb))
                    Type = PersonType.Teamtag;
            }
        }
        private int kcs = -1;
        
        /// <summary>Zero-based</summary>
        public int Kiscsoport
        {
            get { return kcs; }
            set { kcs = value; RaisePropertyChanged(); }
        }
        private int acs = -1;

        /// <summary>Zero-based</summary>
        public int Alvocsoport
        {
            get { return acs; }
            set { acs = value; RaisePropertyChanged(); }
        }
        public override string ToString()
        {
            return Name;
        }
        private Person kinekAzUjonca;
        public Person KinekAzUjonca
        {
            get { return kinekAzUjonca; }
            set { kinekAzUjonca = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// These will be filled out by <see cref="Algorithms.ConvertEdges"/> 
        /// </summary>
        internal HashSet<Person> kivelIgen = new(), kivelNem = new();
        
        /// <summary>
        /// Traverse the graphs defined by kivelIgen and kivelNem.
        /// Collect the transitively related nodes into these sets so that no further recursive traversal is needed during the algorithm.
        /// </summary>
        internal void CollectRecursiveEdges()
        {
            var visitedSet = new HashSet<Person>();
            var queue = new Queue<Person>();
            foreach (Person p in kivelIgen)
                queue.Enqueue(p);            
            while (queue.Count > 0)
            {
                Person p = queue.Dequeue();
                kivelIgen.Add(p);
                visitedSet.Add(p);
                foreach (Person q in p.kivelIgen)
                    if (!visitedSet.Contains(q))
                        queue.Enqueue(q);
                foreach (Person q in p.kivelNem)
                    kivelNem.Add(q);
            }
        }
    }
}
