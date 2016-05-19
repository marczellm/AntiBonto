using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;

namespace AntiBonto.ViewModel
{
    class ObservableCollection2<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection)
            {
                Items.Add(i);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    class MainWindow: ViewModelBase
    {
        private ObservableCollection2<Person> ocp = new ObservableCollection2<Person>();
        public ObservableCollection2<Person> People
        {
            get
            {
                return ocp;
            }
            set
            {
                ocp = value;
                RaisePropertyChanged();
            }
        }
    }
}
