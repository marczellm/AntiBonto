using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AntiBonto.ViewModel.Tests
{
    [TestClass()]
    public class CollectionViewHelperTests
    {
        [TestMethod()]
        public void AccessedPropertiesTest()
        {
            List<Expression<Func<object, bool>>> exps = new List<Expression<Func<object, bool>>>
            {
                 p => ((Person)p).Nem == Nem.Fiu,
                 p => ((Person)p).Type != PersonType.Egyeb && ((Person)p).Type != PersonType.Ujonc,
                 p => ((Person)p).Kiscsoportvezeto,
                 p => ((Person)p).Kiscsoport == 1 && ((Person)p).Type != PersonType.Egyeb,
                 p => ((Person)p).Kiscsoport == 1 || ((Person)p).Kiscsoport == 2 || ((Person)p).Kiscsoport == 3
            };
            List<string> expected = new List<string>
            {
                nameof(Person.Nem),
                nameof(Person.Type),
                nameof(Person.Type),
                nameof(Person.Kiscsoportvezeto),
                nameof(Person.Kiscsoport),
                nameof(Person.Type),
                nameof(Person.Kiscsoport),
                nameof(Person.Kiscsoport),
                nameof(Person.Kiscsoport)
            };
            var actual = new List<string>();
            foreach (var exp in exps)
                actual.AddRange(CollectionViewHelper.AccessedProperties<Person>(exp));
            foreach (Tuple<string, string> tup in Enumerable.Zip(expected, actual, (x, y) => Tuple.Create(x,y)))
                Assert.AreEqual(tup.Item1, tup.Item2);
        }
    }
}