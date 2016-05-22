using AntiBonto.ViewModel;
using System;

namespace AntiBonto
{
    /// <summary>
    /// A Kiss Laci féle "Hétvége kezelő" Excel+VBA által használt kódolás
    /// </summary>
    public enum PersonType
    {
        Teamtag = 0,
        Fiú_vezető = 1,
        Lány_vezető = 2,
        Zeneteamvezető = 3,
        Zeneteamtag = 4,
        Kiscsoportvezető = 5, // ezt én vettem hozzá
        Egyéb = 10,
        Újonc = 11
    }
    public enum Nem
    {
        Lány, Fiú
    }
    public class Person: ViewModelBase
    {
        public string Name { get; set; }
        private int _birthYear;
        public int BirthYear
        {
            get { return _birthYear; }
            set { _birthYear = value; RaisePropertyChanged(); RaisePropertyChanged("Age"); }
        }
        public int Age {
            get { return DateTime.Now.Year - BirthYear; }
            set { BirthYear = DateTime.Now.Year - value; RaisePropertyChanged(); RaisePropertyChanged("BirthYear"); }
        }
        private PersonType _type;
        private Nem? _nem;
        public Nem? Nem
        {
            get { return _nem; }
            set { _nem = value; RaisePropertyChanged(); }
        }

        public PersonType Type
        {
            get { return _type; }
            set { _type = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
