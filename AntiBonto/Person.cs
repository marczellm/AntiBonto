using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiBonto
{
    enum PersonType
    {
        Teamtag = 0,
        Fiu_vezeto = 1,
        Lany_vezeto = 2,
        Zeneteam_vezeto = 3,
        Zeneteam_tag = 4,
        Egyeb = 10,
        Ujonc = 11
    }
    class Person
    {
        public String name;
        public int birthyear;
        public PersonType type;
    }
}
