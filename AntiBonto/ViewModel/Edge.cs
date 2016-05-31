using System;

namespace AntiBonto
{
    [Serializable]
    public class Edge
    {
        public Person[] Persons { get; set; } = new Person[2];
        public bool Dislike { get; set; }
        public string Reason { get; set; }
        public override string ToString()
        {
            return Persons[0] + " és " + Persons[1] + (Dislike? " nem lehetnek együtt, mert ": " együtt kell legyenek, mert ") + Reason;
        }
    }
}
