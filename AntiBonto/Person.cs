using AntiBonto.ViewModel;

namespace AntiBonto
{
    public enum PersonType
    {
        Teamtag = 0,
        Fiú_vezető = 1,
        Lány_vezető = 2,
        Zeneteamvezető = 3,
        Zeneteamtag = 4,
        Egyéb = 10,
        Újonc = 11
    }
    public class Person: ViewModelBase
    {
        public string Name { get; set; }
        public int BirthYear;
        public PersonType Type;
    }
}
