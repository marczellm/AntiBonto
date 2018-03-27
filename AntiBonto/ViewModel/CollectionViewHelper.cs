using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace AntiBonto.ViewModel
{
    /// <summary>
    /// This class exists to reduce the boilerplate needed to create lazily initialized live filtering collection views.
    /// </summary>
    public class CollectionViewHelper
    {
        /// <summary>
        /// Recursively enumerates all member accesses
        /// </summary>
        public static List<string> AccessedProperties(Expression expression)
        {
            if (expression is MemberExpression && (expression as MemberExpression).Expression.Type == typeof(Person))
                return new List<string> { (expression as MemberExpression).Member.Name };
            else if (expression is BinaryExpression)
            {
                var bin = expression as BinaryExpression;
                return AccessedProperties(bin.Left).Concat(AccessedProperties(bin.Right)).ToList();
            }
            else if (expression is UnaryExpression)
                return AccessedProperties((expression as UnaryExpression).Operand);
            else return new List<string> { };
        }

        private static Dictionary<string, ICollectionView> collectionViewCache = new Dictionary<string, ICollectionView>();

        /// <summary>
        /// Returns a newly created CollectionView that live filters the given collection by the given filter expression.
        /// </summary>
        public static ICollectionView Get(object source, Expression<Func<object, bool>> filter)
        {
            CollectionViewSource cvs = new CollectionViewSource { Source = source, IsLiveFilteringRequested = true, IsLiveSortingRequested = true };
            foreach (string prop in AccessedProperties(filter.Body))
                cvs.LiveFilteringProperties.Add(prop);
            cvs.LiveSortingProperties.Add("Name");
            cvs.View.Filter = filter.Compile().Invoke;
            cvs.View.CollectionChanged += EmptyEventHandler;
            cvs.View.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            return cvs.View;
        }

        /// <summary>
        /// Returns the existing CollectionView for the given property name, or a newly created one if there is none
        /// </summary>
        public static ICollectionView Lazy(object source, Expression<Func<object, bool>> filter, [CallerMemberName] String name = "")
        {
            if (!collectionViewCache.ContainsKey(name))
                collectionViewCache.Add(name, Get(source, filter));
            return collectionViewCache[name];
        }

        /// <summary>
        /// Adding this seems to fix a bug (see http://stackoverflow.com/questions/37394151), although I have no idea why
        /// </summary>
        private static void EmptyEventHandler(object sender, NotifyCollectionChangedEventArgs e) { }
    }
}
