using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
        public MainWindow()
        {
            ocp.CollectionChanged += Ocp_CollectionChanged;
        }

        private void Ocp_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PeopleNotEmpty");
        }

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
                RaisePropertyChanged("PeopleNotEmpty");
            }
        }
        public bool PeopleNotEmpty
        {
            get { return People.Count() != 0; }
        }
        public ICollectionView Fiuk
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People };
                cvs.View.Filter = p => ((Person)p).Nem == Nem.Fiú;
                return cvs.View;
            }
        }
        public ICollectionView Lanyok
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People };
                cvs.View.Filter = p => ((Person)p).Nem == Nem.Lány;
                return cvs.View;
            }
        }
        public ICollectionView Ujoncok
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People };
                cvs.View.Filter = p => ((Person)p).Type == PersonType.Újonc;
                return cvs.View;
            }
        }
        public ICollectionView Teamtagok
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People };                
                cvs.View.Filter = p => ((Person)p).Type != PersonType.Egyéb || ((Person)p).Type != PersonType.Újonc;
                return cvs.View;
            }
        }
    }
}
