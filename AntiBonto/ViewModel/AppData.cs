using System;

namespace AntiBonto.ViewModel
{
    /// <summary>
    /// All the data that's saved to XML so that it's there for the next app launch
    /// </summary>
    [Serializable]
    public class AppData
    {
        public Person[] Persons;
        public Edge[] Edges;
        public Person[][] MutuallyExclusiveGroups;
    }
}
