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

        private int _birthYear = DateTime.Now.Year;
        public int BirthYear
        {
            get { return _birthYear; }
            set { _birthYear = value; RaisePropertyChanged(); RaisePropertyChanged("Age"); }
        }
        [XmlIgnore]
        public int Age
        {
            get { return DateTime.Now.Year - BirthYear; }
            set { BirthYear = DateTime.Now.Year - value; RaisePropertyChanged(); RaisePropertyChanged("BirthYear"); }
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
        private bool kcsvez = false;
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
        private int kcs = -1;
        public int Kiscsoport
        {
            get { return kcs; }
            set { kcs = value; RaisePropertyChanged(); }
        }
        public override string ToString()
        {
            return Name;
        }
        private Person kinekAzUjonca;
        public Person KinekAzUjonca
        {
            get { return kinekAzUjonca; }
            set { kinekAzUjonca = value;  RaisePropertyChanged(); }
        }

        internal List<Person> kivelIgen = new List<Person>(), kivelNem = new List<Person>();        
    }
}
