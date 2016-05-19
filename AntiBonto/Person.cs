using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class Person
    {
        public String name;
        public int birthyear;
        public PersonType type;
    }
}
