using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace AntiBonto.ViewModel
{
    /// <summary>
    /// ObservableCollection with added AddRange support
    /// </summary>
    public class ObservableCollection2<T> : ObservableCollection<T>
    {
        public ObservableCollection2() : base() { }
        public ObservableCollection2(T[] t) : base(t) { }
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection)
                Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void RemoveAll(Func<T, bool> cond)
        {
            Items.Where(cond).ToList().All(p => Items.Remove(p));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
