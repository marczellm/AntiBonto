﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiBonto
{
    class Algorithms
    {
        private ViewModel.MainWindow d;
        private List<Person> Ujoncok, Team, Beosztando, Kiscsoportvezetok;
        private int n, m, k, u, t, tpk, upk, fpk, lpk;
        private static Random rng = new Random();
        public Algorithms(ViewModel.MainWindow data)
        {
            d = data;
            Ujoncok = d.Ujoncok.Cast<Person>().ToList();
            Team = d.Team.Cast<Person>().ToList();
            Beosztando = d.KiscsoportbaOsztando.Cast<Person>().ToList();
            Kiscsoportvezetok = d.Kiscsoportvezetok.Cast<Person>().ToList();
            UpdateEdges();
        }

        private void UpdateEdges()
        {
            foreach(Person p in d.People)
            {
                p.Kiscsoport = -1;
                p.kivelIgen.Clear();
                p.kivelNem.Clear();
            }
            foreach (Person p in Ujoncok)
            {
                p.kivelNem.Add(p.KinekAzUjonca);
                p.KinekAzUjonca.kivelNem.Add(p);
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
                    e.Persons[0].kivelIgen.Add(e.Persons[1]);
                    e.Persons[1].kivelIgen.Add(e.Persons[0]);
                }
            }            
        }

        private void RecursiveSet(Person p, int kiscsoport)
        {
            p.Kiscsoport = kiscsoport;
            foreach (Person q in p.kivelIgen)
                if (q.Kiscsoport != kiscsoport)
                    RecursiveSet(q, kiscsoport);
        }

        private bool Conflicts(Person p, int kiscsoport)
        {
            var kcs = d.Kiscsoport(kiscsoport).Cast<Person>();
            return kcs.Count() >= k 
                || kcs.Count(q => q.Type == PersonType.Ujonc) >= upk
                || kcs.Count(q => q.Type == PersonType.Teamtag) >= tpk
//                || kcs.Count(q => q.Nem == Nem.Lany) >= lpk
//                || kcs.Count(q => q.Nem == Nem.Fiu) >= fpk
                || kcs.Any(q => q.kivelNem.Contains(p) || Math.Abs(q.Age - p.Age) > d.MaxAgeDifference);
        }

        /// <summary>
        /// Randomly shuffles a list using the Fisher-Yates shuffle.
        /// </summary>
        private static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        /// <summary>
        /// Generates all possible permutations of an enumerable
        /// </summary>
        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public void NaiveFirstFit()
        {
            m = 0; // kiscsoportok száma
            foreach (Person p in Kiscsoportvezetok)
                RecursiveSet(p, m++);
            n = Beosztando.Count(); // kiscsoportba osztandók száma
            u = Ujoncok.Count(); // újoncok száma
            t = Team.Count(); // team létszáma
            k = (int)Math.Ceiling(n / (double)m); // kiscsoportok létszáma
            int f = Beosztando.Where(p => p.Nem == Nem.Fiu).Count();
            int l = Beosztando.Where(p => p.Nem == Nem.Lany).Count();
            upk = (int)Math.Ceiling(u / (double)m); // újonc per kiscsoport
            tpk = (int)Math.Ceiling(t / (double)m); // teamtag per kiscsoport
            fpk = (int)Math.Ceiling(f / (double)m); // fiú per kiscsoport
            lpk = (int)Math.Ceiling(f / (double)l); // lány per kiscsoport

            while (true) // generate random orderings of People and run the first-fit coloring until it is complete
            {
                try
                {
                    foreach (Person p in Beosztando)
                        if (!p.Kiscsoportvezeto)
                            RecursiveSet(p, Enumerable.Range(0, m).First(i => !Conflicts(p, i)));
                    break;
                }
                catch
                {
                    foreach (Person p in Beosztando)
                        if (!p.Kiscsoportvezeto)
                            p.Kiscsoport = -1;
                    Shuffle(Beosztando);
                }
            }
        }
    }
}
