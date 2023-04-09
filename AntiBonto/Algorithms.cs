using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AntiBonto
{
    public class Algorithms
    {
        private readonly ViewModel.MainWindow d;
        private readonly List<Person> Ujoncok, Team, Beosztando, Kiscsoportvezetok;
        private readonly int n, k, u, t, tpk, upk, fpk, lpk;
        private int m;
        private readonly bool consideringSexes;
        private readonly static Random rng = new Random();
        public Algorithms(ViewModel.MainWindow data)
        {
            d = data;
            Ujoncok = d.Ujoncok.Cast<Person>().ToList();
            Team = d.Team.Cast<Person>().ToList();
            Beosztando = d.CsoportokbaOsztando.Cast<Person>().ToList();
            Kiscsoportvezetok = d.Kiscsoportvezetok.ToList();

            m = Kiscsoportvezetok.Count(); // kiscsoportok száma
            n = Beosztando.Count(); // kiscsoportba osztandók száma
            u = Ujoncok.Count(); // újoncok száma
            t = Team.Count(); // team létszáma
            k = (int)Math.Ceiling(n / (double)m); // kiscsoportok létszáma
            int f = Beosztando.Where(p => p.Nem == Nem.Fiu).Count();
            int l = Beosztando.Where(p => p.Nem == Nem.Lany).Count();
            consideringSexes = d.Nullnemuek.IsEmpty;
            upk = (int)Math.Ceiling(u / (double)m); // újonc per kiscsoport
            tpk = (int)Math.Ceiling(t / (double)m); // teamtag per kiscsoport
            fpk = (int)Math.Ceiling(f / (double)m); // fiú per kiscsoport
            lpk = (int)Math.Ceiling(l / (double)m); // lány per kiscsoport

            ExtraInitialization();
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
                p.kivelIgen.Clear();
                p.kivelNem.Clear();
            }
            foreach (Person p in Ujoncok)
                if (p.KinekAzUjonca != null)
                {
                    p.kivelNem.Add(p.KinekAzUjonca);
                    p.KinekAzUjonca.kivelNem.Add(p);
                }
            d.Fiuvezeto.kivelNem.Add(d.Lanyvezeto);
            d.Lanyvezeto.kivelNem.Add(d.Fiuvezeto);
            // Split up the MutuallyExclusiveGroups to groups no bigger than m
            List<List<Person>> mutuallyExclusiveGroups = new List<List<Person>>();
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
                            p.kivelNem.Add(q);
                            q.kivelNem.Add(p);
                        }            
            foreach (Edge e in d.Edges)
            {
                if (e.Dislike)
                {
                    e.Persons[0].kivelNem.Add(e.Persons[1]);
                    e.Persons[1].kivelNem.Add(e.Persons[0]);
                }
                else
                {
                    // We can safely leave out edges pointing towards group leaders,
                    // because the first step of the algorithm calls RecursiveSet on them.
                    // If we don't exclude these edges, group leaders may get reassigned later.
                    if (!e.Persons[1].Kiscsoportvezeto)
                        e.Persons[0].kivelIgen.Add(e.Persons[1]);
                    if (!e.Persons[0].Kiscsoportvezeto)
                        e.Persons[1].kivelIgen.Add(e.Persons[0]);
                }
            }
            ExtraEdges();
            foreach (Person p in Beosztando)
                p.CollectRecursiveEdges();
        }

        /// <summary>
        /// Assign the given group number to the person and all of their BFFs.
        /// </summary>
        private void AssignToKiscsoport(Person p, int kiscsoport)
        {
            p.Kiscsoport = kiscsoport;
            foreach (Person q in p.kivelIgen)
                q.Kiscsoport = kiscsoport;
        }

        public bool Conflicts(Person p, int kiscsoport)
        {
            var kcs = d.Kiscsoport(kiscsoport);
            bool ret = p.Kiscsoportvezeto || kcs.Count() + p.kivelIgen.Count + 1 > k
                || (p.Type == PersonType.Ujonc && kcs.Count(q => q.Type == PersonType.Ujonc) >= upk)
                || (p.Type == PersonType.Teamtag && kcs.Count(q => q.Type == PersonType.Teamtag) >= tpk)
                || (consideringSexes && p.Nem == Nem.Lany && kcs.Count(q => q.Nem == Nem.Lany) >= lpk)
                || (consideringSexes && p.Nem == Nem.Fiu  && kcs.Count(q => q.Nem == Nem.Fiu) >= fpk)
                || kcs.Any(q => q.kivelNem.Contains(p) || Math.Abs(q.Age - p.Age) > d.MaxAgeDifference);
            return ret;
            }

        public bool Conflicts(Person p, int kiscsoport, out string message)
        {
            var kcs = d.Kiscsoport(kiscsoport);
            message = null;
            if (p.Kiscsoportvezeto)
                message = "Nem lehet egy csoportban két kiscsoportvezető!";
            else if (kcs.Count() + p.kivelIgen.Count + 1 > k)
                message = "Nem lehet a kiscsoportban több ember";
            else if (p.Type == PersonType.Ujonc && kcs.Count(q => q.Type == PersonType.Ujonc) >= upk)
                message = "Nem lehet a kiscsoportban több újonc";
            else if (p.Type == PersonType.Teamtag && kcs.Count(q => q.Type == PersonType.Teamtag) >= tpk)
                message = "Nem lehet a kiscsoportban több teamtag";
            else if (consideringSexes && p.Nem == Nem.Lany && kcs.Count(q => q.Nem == Nem.Lany) >= lpk)
            {
                message = "Elvileg nem lehet a kiscsoportban több lány";
                return false;
            }
            else if (consideringSexes && p.Nem == Nem.Fiu && kcs.Count(q => q.Nem == Nem.Fiu) >= fpk)
            {
                message = "Elvileg nem lehet a kiscsoportban több fiú";
                return false;
            }
            else
            {
                Person r = kcs.FirstOrDefault(q => q.kivelNem.Contains(p));
                if (r != null)
                {
                    Edge edge = d.Edges.FirstOrDefault(e => e.Dislike && e.Persons.Contains(p) && e.Persons.Contains(r)) ?? new Edge
                    {
                        Persons = new Person[] { p, r },
                        Dislike = true,
                        Reason = p.KinekAzUjonca == r || r.KinekAzUjonca == p ? "az újonca" : "ezt kérted"
                    };
                    message = edge.ToString();
                }
                else
                {
                    r = kcs.FirstOrDefault(q => Math.Abs(q.Age - p.Age) > d.MaxAgeDifference);
                    if (r != null)
                    {
                        Edge edge = new Edge { Persons = new Person[] { p, r }, Dislike = true, Reason = "a korkülönbség nagyobb, mint " + d.MaxAgeDifference };
                        message = edge.ToString();
                    }
                }
            }
            return message != null;
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
            foreach (Person p in Beosztando)
                if (!p.Pinned)
                    p.Kiscsoport = -1;
            foreach (Person p in Kiscsoportvezetok)
                AssignToKiscsoport(p, m++);
            Beosztando.RemoveAll((Person p) => p.Kiscsoport != -1);

            bool kesz = false;
            while (!kesz && ct?.IsCancellationRequested != true) // generate random orderings of People and run the first-fit coloring until it is complete or cancelled
            {
                kesz = true;
                Shuffle(Beosztando);
                ExtraPreparation();
                foreach (Person p in Beosztando)
                {
                    if (!p.Kiscsoportvezeto)
                    {
                        var options = Enumerable.Range(0, m).Where(i => !Conflicts(p, i));
                        if (options.Any())
                        {   
                            if (p.Type == PersonType.Ujonc) // ha újonc, akkor próbáljuk olyan helyre tenni, ahol még kevés újonc van
                                AssignToKiscsoport(p, options.MinBy(i => d.Kiscsoport(i).Count(q => q.Type == PersonType.Ujonc)));                            
                            else // különben ahol kevés ember van
                                AssignToKiscsoport(p, options.MinBy(i => d.Kiscsoport(i).Count()));
                        }
                        else // Nincs olyan kiscsoport, ahova be lehetne tenni => elölről kezdjük
                        {
                            foreach (Person q in Beosztando)
                                if (!q.Kiscsoportvezeto)
                                    q.Kiscsoport = -1;
                            foreach (Person q in Kiscsoportvezetok)
                                AssignToKiscsoport(q, q.Kiscsoport);
                            kesz = false;
                            break;
                        }
                    }
                }
            }
            return kesz;
        }

        #region Extras
        // 20. HV: Minden szentendrei újonc mellett legyen szentendrei régenc
        private List<Person> szentendreiUjoncok, szentendreiRegencek;
        private void ExtraInitialization()
        {
            if (ViewModel.MainWindow.WeekendNumber != 20)
                return;
            szentendreiUjoncok = d.Szentendre.Intersect(Ujoncok).ToList();
            szentendreiRegencek = d.Szentendre.Except(Ujoncok).ToList();
        }
        private void ExtraEdges()
        {
            if (szentendreiRegencek?.Any() != true && szentendreiUjoncok?.Any() != true)
                return;
            Shuffle(szentendreiRegencek);
            Shuffle(szentendreiUjoncok);
            for (int i = 0; i < szentendreiUjoncok.Count; i++)
            {
                var p = szentendreiUjoncok[i];
                var regencek = szentendreiRegencek.Except(new Person[] { p.KinekAzUjonca }).ToList();
                var q = regencek[i % regencek.Count];
                p.kivelIgen.Add(q);
                q.kivelIgen.Add(p);
            }
        }
        private void ExtraPreparation()
        {
            if (szentendreiRegencek?.Any() == true && szentendreiUjoncok?.Any() == true)
                ConvertEdges();
        }       
        #endregion
    }
}