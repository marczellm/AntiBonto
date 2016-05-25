using System;
using System.Linq;

namespace AntiBonto
{
    class Algorithms
    {
        private ViewModel.MainWindow d;
        public Algorithms(ViewModel.MainWindow data)
        {
            d = data;
            UpdateEdges();
        }

        private void UpdateEdges()
        {
            foreach(Person p in d.People)
            {
                p.kivelIgen.Clear();
                p.kivelNem.Clear();
            }
            foreach (Person p in d.Ujoncok)
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
                RecursiveSet(q, kiscsoport);
        }

        private bool Conflicts(Person p, int kiscsoport)
        {
            return d.Kiscsoport(kiscsoport).Cast<Person>().Any(q => q.kivelNem.Contains(p) || Math.Abs(q.Age - p.Age) > d.MaxAgeDifference);
        }

        public void Naive()
        {
            int m = 0; // kiscsoportok száma
            foreach (Person p in d.Kiscsoportvezetok)
                RecursiveSet(p, m++);
            int n = d.KiscsoportbaOsztando.Cast<Person>().Count(); // kiscsoportba osztandók száma
            int u = d.Ujoncok.Cast<Person>().Count(); // újoncok száma
            int t = d.Team.Cast<Person>().Count(); // team létszáma
            int k = (int) Math.Ceiling(n / (double)m); // kiscsoportok létszáma
            int upk = (int)Math.Ceiling(u / (double)m); // újonc per kiscsoport
            int tpk = (int)Math.Ceiling(t / (double)m); // teamtag per kiscsoport
        }
    }
}
