using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AntiBonto
{
    public class Algorithms
    {
        private readonly ViewModel.MainWindow d;
        private readonly List<Person> Newcomers, Team, PeopleToAssign, SharingGroupLeaders;
        private readonly int n, k, u, t, tpk, upk, fpk, lpk;
        private int m;
        private readonly bool consideringSexes;
        private readonly static Random rng = new();
        public Algorithms(ViewModel.MainWindow data)
        {
            d = data;
            Newcomers = d.Newcomers.Cast<Person>().ToList();
            Team = d.Team.Cast<Person>().ToList();
            PeopleToAssign = d.PeopleToAssign.Cast<Person>().ToList();
            SharingGroupLeaders = d.SharingGroupLeaders.ToList();

            m = SharingGroupLeaders.Count; // kiscsoportok száma
            n = PeopleToAssign.Count; // kiscsoportba osztandók száma
            u = Newcomers.Count; // újoncok száma
            t = Team.Count; // team létszáma
            k = (int)Math.Ceiling(n / (double)m); // kiscsoportok létszáma
            int f = PeopleToAssign.Where(p => p.Sex == Sex.Boy).Count();
            int l = PeopleToAssign.Where(p => p.Sex == Sex.Girl).Count();
            consideringSexes = d.SexUndefined.IsEmpty;
            upk = (int)Math.Ceiling(u / (double)m); // újonc per kiscsoport
            tpk = (int)Math.Ceiling(t / (double)m); // teamtag per kiscsoport
            fpk = (int)Math.Ceiling(f / (double)m); // fiú per kiscsoport
            lpk = (int)Math.Ceiling(l / (double)m); // lány per kiscsoport

            ConvertEdges();
        }

        /// <summary>
        /// Convert the standalone Edge representation of constraints
        /// to one that lists incompatible and must-go-together people in properties of the Person object.
        /// 
        /// This new representation also includes additional inferred constraints from Ujoncok and MutuallyExclusiveGroups.
        /// </summary>
        private void ConvertEdges()
        {
            foreach(Person p in d.People)
            {
                p.includeEdges.Clear();
                p.excludeEdges.Clear();
            }
            foreach (Person p in Newcomers)
                if (p.WhoseNewcomer != null)
                {
                    p.excludeEdges.Add(p.WhoseNewcomer);
                    p.WhoseNewcomer.excludeEdges.Add(p);
                }
            d.BoyLeader.excludeEdges.Add(d.GirlLeader);
            d.GirlLeader.excludeEdges.Add(d.BoyLeader);
            // Split up the MutuallyExclusiveGroups to groups no bigger than m
            List<List<Person>> mutuallyExclusiveGroups = new();
            foreach (IList<Person> group in d.MutuallyExclusiveGroups)
                for (int i=0, j=i; i<group.Count; i++, j=i%m)
                {
                    if (j==0)
                        mutuallyExclusiveGroups.Add(new List<Person>());
                    mutuallyExclusiveGroups.Last().Add(group[i]);
                }
            foreach (ICollection<Person> group in mutuallyExclusiveGroups)
                foreach (Person p in group)
                    foreach (Person q in group)
                        if (p != q)
                        {
                            p.excludeEdges.Add(q);
                            q.excludeEdges.Add(p);
                        }            
            foreach (Edge e in d.Edges)
            {
                if (e.Dislike)
                {
                    e.Persons[0].excludeEdges.Add(e.Persons[1]);
                    e.Persons[1].excludeEdges.Add(e.Persons[0]);
                }
                else
                {
                    // We can safely leave out edges pointing towards group leaders,
                    // because the first step of the algorithm calls RecursiveSet on them.
                    // If we don't exclude these edges, group leaders may get reassigned later.
                    if (!e.Persons[1].SharingGroupLeader)
                        e.Persons[0].includeEdges.Add(e.Persons[1]);
                    if (!e.Persons[0].SharingGroupLeader)
                        e.Persons[1].includeEdges.Add(e.Persons[0]);
                }
            }
            foreach (Person p in PeopleToAssign)
                p.CollectRecursiveEdges();
        }

        /// <summary>
        /// Assign the given group number to the person and all of their BFFs.
        /// </summary>
        private void AssignToSharingGroup(Person p, int sharingGroup)
        {
            p.SharingGroup = sharingGroup;
            foreach (Person q in p.includeEdges)
                q.SharingGroup = sharingGroup;
        }

        public bool Conflicts(Person p, int sharingGroup)
        {
            var kcs = d.SharingGroup(sharingGroup);
            bool ret = p.SharingGroupLeader || kcs.Count() + p.includeEdges.Count + 1 > k
                || (p.Type == PersonType.Newcomer && kcs.Count(q => q.Type == PersonType.Newcomer) >= upk)
                || (p.Type == PersonType.Team && kcs.Count(q => q.Type == PersonType.Team) >= tpk)
                || (consideringSexes && p.Sex == Sex.Girl && kcs.Count(q => q.Sex == Sex.Girl) >= lpk)
                || (consideringSexes && p.Sex == Sex.Boy  && kcs.Count(q => q.Sex == Sex.Boy) >= fpk)
                || kcs.Any(q => q.excludeEdges.Contains(p) || Math.Abs(q.Age - p.Age) > d.MaxAgeDifference);
            return ret;
            }

        public bool Conflicts(Person p, int sharingGroup, out string message)
        {
            var kcs = d.SharingGroup(sharingGroup);
            message = null;
            if (p.SharingGroupLeader)
                message = "Nem lehet egy csoportban két kiscsoportvezető!";
            else if (kcs.Count() + p.includeEdges.Count + 1 > k)
                message = "Nem lehet a kiscsoportban több ember";
            else if (p.Type == PersonType.Newcomer && kcs.Count(q => q.Type == PersonType.Newcomer) >= upk)
                message = "Nem lehet a kiscsoportban több újonc";
            else if (p.Type == PersonType.Team && kcs.Count(q => q.Type == PersonType.Team) >= tpk)
                message = "Nem lehet a kiscsoportban több teamtag";
            
            else
            {
                Person r = kcs.FirstOrDefault(q => q.excludeEdges.Contains(p));
                if (r != null)
                {
                    Edge edge = d.Edges.FirstOrDefault(e => e.Dislike && e.Persons.Contains(p) && e.Persons.Contains(r)) ?? new Edge
                    {
                        Persons = new Person[] { p, r },
                        Dislike = true,
                        Reason = p.WhoseNewcomer == r || r.WhoseNewcomer == p ? "az újonca" : "ezt kérted"
                    };
                    message = edge.ToString();
                }
                else
                {
                    r = kcs.FirstOrDefault(q => Math.Abs(q.Age - p.Age) > d.MaxAgeDifference);
                    if (r != null)
                    {
                        Edge edge = new() { Persons = new Person[] { p, r }, Dislike = true, Reason = "a korkülönbség nagyobb, mint " + d.MaxAgeDifference };
                        message = edge.ToString();
                    }
                }
            }
            if (message != null)
            {
                return true;
            }
            if (consideringSexes && p.Sex == Sex.Girl && kcs.Count(q => q.Sex == Sex.Girl) >= lpk)
            {
                message = "Elvileg nem lehet a kiscsoportban több lány";
            }
            else if (consideringSexes && p.Sex == Sex.Boy && kcs.Count(q => q.Sex == Sex.Boy) >= fpk)
            {
                message = "Elvileg nem lehet a kiscsoportban több fiú";
            }
            return false;
        }

        /// <summary>
        /// Randomly shuffles a list using the Fisher-Yates shuffle.
        /// </summary>
        private static IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
            return list;
        }
        /// <summary>
        /// Generates all possible permutations of an enumerable
        /// </summary>
        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Runs a naive first fit algorithm to determine a proper "graph coloring".
        /// The success of such an algorithm depends only on the given ordering of nodes.
        /// This implementation randomly shuffles the nodes when backtracking.
        /// </summary>
        /// <returns>whether the algorithm was successful</returns>
        public bool NaiveFirstFit(CancellationToken? ct = null)
        {
            m = 0; // kiscsoportok száma
            foreach (Person p in PeopleToAssign)
                if (!p.Pinned)
                    p.SharingGroup = -1;
            foreach (Person p in SharingGroupLeaders)
                AssignToSharingGroup(p, m++);
            PeopleToAssign.RemoveAll((Person p) => p.SharingGroup != -1);

            bool kesz = false;
            while (!kesz && ct?.IsCancellationRequested != true) // generate random orderings of People and run the first-fit coloring until it is complete or cancelled
            {
                kesz = true;
                Shuffle(PeopleToAssign);
                foreach (Person p in PeopleToAssign)
                {
                    if (!p.SharingGroupLeader)
                    {
                        var options = Enumerable.Range(0, m).Where(i => !Conflicts(p, i));
                        if (options.Any())
                        {   
                            if (p.Type == PersonType.Newcomer) // ha újonc, akkor próbáljuk olyan helyre tenni, ahol még kevés újonc van
                                AssignToSharingGroup(p, options.MinBy(i => d.SharingGroup(i).Count(q => q.Type == PersonType.Newcomer)));                            
                            else // különben ahol kevés ember van
                                AssignToSharingGroup(p, options.MinBy(i => d.SharingGroup(i).Count()));
                        }
                        else // Nincs olyan kiscsoport, ahova be lehetne tenni => elölről kezdjük
                        {
                            foreach (Person q in PeopleToAssign)
                                if (!q.SharingGroupLeader)
                                    q.SharingGroup = -1;
                            foreach (Person q in SharingGroupLeaders)
                                AssignToSharingGroup(q, q.SharingGroup);
                            kesz = false;
                            break;
                        }
                    }
                }
            }
            return kesz;
        }
    }
}