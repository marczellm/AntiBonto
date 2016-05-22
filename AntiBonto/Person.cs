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
        Fiuvezeto = 1,
        Lanyvezeto = 2,
        Zeneteamvezeto = 3,
        Zeneteamtag = 4,
        Egyeb = 10,
        Ujonc = 11
    }
    public enum Nem
    {
        Lany, Fiu
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
        private PersonType type;
        private Nem? nem;
        private bool kcsvez = false;
        public Nem? Nem
        {
            get { return nem; }
            set { nem = value; RaisePropertyChanged(); }
        }

        public PersonType Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged(); }
        }

        public bool Kiscsoportvezeto
        {
            get { return kcsvez; }
            set { kcsvez = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
