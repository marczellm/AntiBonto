using System;

namespace AntiBonto
{
    [Serializable]
    public class Edge
    {
        public Person[] Persons { get; set; } = new Person[2];
        public bool Dislike { get; set; }
        public string Reason { get; set; }
    }
}
