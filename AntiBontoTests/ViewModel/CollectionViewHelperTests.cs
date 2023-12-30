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
            List<Expression<Func<object, bool>>> exps = new()
            {
                 p => ((Person)p).Sex == Sex.Boy,
                 p => ((Person)p).Type != PersonType.Others && ((Person)p).Type != PersonType.Newcomer,
                 p => ((Person)p).SharingGroupLeader,
                 p => ((Person)p).SharingGroup == 1 && ((Person)p).Type != PersonType.Others,
                 p => ((Person)p).SharingGroup == 1 || ((Person)p).SharingGroup == 2 || ((Person)p).SharingGroup == 3
            };
            List<string> expected = new()
            {
                nameof(Person.Sex),
                nameof(Person.Type),
                nameof(Person.Type),
                nameof(Person.SharingGroupLeader),
                nameof(Person.SharingGroup),
                nameof(Person.Type),
                nameof(Person.SharingGroup),
                nameof(Person.SharingGroup),
                nameof(Person.SharingGroup)
            };
            var actual = new List<string>();
            foreach (var exp in exps)
                actual.AddRange(CollectionViewHelper.AccessedProperties<Person>(exp));
            foreach (Tuple<string, string> tup in Enumerable.Zip(expected, actual, (x, y) => Tuple.Create(x,y)))
                Assert.AreEqual(tup.Item1, tup.Item2);
        }
    }
}