using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace AntiBonto.ViewModel
{
    class TitledCollectionView : ListCollectionView
    {
        private string title;
        public string Title
        {
            get => title; 
            set
            {
                title = value;
                TitleChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        public event PropertyChangedEventHandler TitleChanged;

        public TitledCollectionView(IList list) : base(list)
        {
        }
    }

    /// <summary>
    /// ObservableCollection with added AddRange support
    /// </summary>
    public class ObservableCollection2<T> : ObservableCollection<T>, ICollectionViewFactory
    {
        public ObservableCollection2() : base() { }
        public ObservableCollection2(T[] t) : base(t) { }
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection)
                Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public ICollectionView CreateView()
        {
            return new TitledCollectionView(this);
        }

        public void RemoveAll(Func<T, bool> cond)
        {
            Items.Where(cond).ToList().ForEach(p => Items.Remove(p));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
