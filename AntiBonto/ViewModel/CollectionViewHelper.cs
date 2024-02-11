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
        public static List<string> AccessedProperties<T>(Expression expression)
        {
            if (expression is MemberExpression mem && mem.Expression.Type == typeof(T))
                return new List<string> { mem.Member.Name };
            else if (expression is BinaryExpression bin)
                return AccessedProperties<T>(bin.Left).Concat(AccessedProperties<T>(bin.Right)).ToList();
            else if (expression is UnaryExpression un)
                return AccessedProperties<T>(un.Operand);
            else return new List<string> { };
        }

        private static readonly Dictionary<string, ICollectionView> collectionViewCache = new();

        /// <summary>
        /// Returns a newly created CollectionView that live filters the given collection by the given filter expression.
        /// </summary>
        public static ICollectionView Get<T> (object source, 
            Expression<Func<T, bool>> filter,
            SortDescription? sortDescription = null)
        {
            var cvs = new CollectionViewSource { Source = source, IsLiveFilteringRequested = true, IsLiveSortingRequested = true };
            foreach (string prop in AccessedProperties<T>(filter.Body))
                cvs.LiveFilteringProperties.Add(prop);
            if (sortDescription != null)
                cvs.LiveSortingProperties.Add(sortDescription?.PropertyName);

            cvs.View.Filter = (object obj) => filter.Compile().Invoke((T)obj);
            cvs.View.CollectionChanged += EmptyEventHandler;
            if (sortDescription != null)
                cvs.View.SortDescriptions.Add(sortDescription.Value);
            return cvs.View;
        }

        /// <summary>
        /// Returns the existing CollectionView for the given property name, or a newly created one if there is none
        /// </summary>
        public static ICollectionView Lazy<T>(object source, 
            Expression<Func<T, bool>> filter, 
            SortDescription? sortDescription = null, 
            [CallerMemberName] string name = "")
        {
            if (!collectionViewCache.ContainsKey(name))
                collectionViewCache.Add(name, Get(source, filter, sortDescription));
            return collectionViewCache[name];
        }

        /// <summary>
        /// Adding this seems to fix a bug (see http://stackoverflow.com/questions/37394151), although I have no idea why
        /// </summary>
        private static void EmptyEventHandler(object sender, NotifyCollectionChangedEventArgs e) { }
    }
}
