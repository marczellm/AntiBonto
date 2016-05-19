using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace AntiBonto.ViewModel
{
    class MainWindow
    {
        private ObservableCollection<Person> ocp;
        public List<Person> ppl;
        public ObservableCollection<Person> people
        {
            get
            {
                return ocp ?? (ocp = new ObservableCollection<Person>(ppl));
            }
        }
    }
}
