using System;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AntiBonto.View
{
    class DropHandler : FrameworkElement, IDropTarget
    {
        private ViewModel.MainWindow d
        {
            get { return (ViewModel.MainWindow)DataContext; }
        }
        /// <summary>
        /// Set where drops are allowed
        /// </summary>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = null;
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            if (!(dropInfo.Data is Person))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            var p = (Person)dropInfo.Data;
            if (p.Nem == Nem.Fiu && target.Name == "Lanyvezeto"
             || p.Nem == Nem.Lany && target.Name == "Fiuvezeto"
             || source.Name == "PeopleView" && target.Name != "PeopleView" && target.Name != "AddOrRemovePersonButton"
             || target.Name == "Kiscsoportvezetok" && d.Kiscsoportvezetok.Cast<Person>().Count() >= 14
             || target.Name == "Alvocsoportvezetok" && d.Alvocsoportvezetok.Cast<Person>().Count() >= 14)
            {
                dropInfo.Effects = DragDropEffects.None;
            }
            else if (target.Name.Contains("kcs") && p.Kiscsoportvezeto)
            {
                dropInfo.Effects = DragDropEffects.None;
                d.StatusText = "A kiscsoportvezetők nem mozgathatók!";
            }
            else if (target.Name.StartsWith("kcs"))
            {
                int kcsn = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                string message = null;
                dropInfo.Effects = (kcsn != p.Kiscsoport && d.Algorithm.Conflicts(p, kcsn, out message)) ? DragDropEffects.None : DragDropEffects.Move;
                d.StatusText = message;
            }            
            else if (target.Name.Contains("acs") && p.Alvocsoportvezeto)
            {
                dropInfo.Effects = DragDropEffects.None;
                d.StatusText = "Az alvócsoportvezetők nem mozgathatók!";
            }
            else if (target.Name.StartsWith("acs"))
            {
                int acsn = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                var acsvez = d.Alvocsoportvezetok.Cast<Person>().Single(q => q.Alvocsoport == acsn);
                dropInfo.Effects = (p.Nem != Nem.Undefined && p.Nem != acsvez.Nem) ? DragDropEffects.None : DragDropEffects.Move;
            }
            else
                dropInfo.Effects = DragDropEffects.Move;
        }
        /// <summary>
        /// Make the necessary data changes upon drop
        /// </summary>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            Person p = (Person)dropInfo.Data;
            switch (target.Name)
            {
                case "Fiuk": p.Nem = Nem.Fiu; break;
                case "Lanyok": p.Nem = Nem.Lany; break;
                case "Nullnemuek": p.Nem = Nem.Undefined; break;
                case "Ujoncok": p.Type = PersonType.Ujonc; break;
                case "Zeneteamvezeto": d.Zeneteamvezeto = p; break;
                case "Lanyvezeto": d.Lanyvezeto = p; break;
                case "Fiuvezeto": d.Fiuvezeto = p; break;
                case "Egyeb": p.Type = PersonType.Egyeb; break;

                case "Team":
                    if (source.Name != "Kiscsoportvezetok" && source.Name != "Alvocsoportvezetok")
                        p.Type = PersonType.Teamtag;
                    break;
                case "Zeneteam":
                    if (p.Type != PersonType.Fiuvezeto && p.Type != PersonType.Lanyvezeto)
                        p.Type = PersonType.Zeneteamtag;
                    break;
                case "Kiscsoportvezetok":
                    if (!p.Kiscsoportvezeto)
                    {
                        p.Kiscsoportvezeto = true;
                        p.Kiscsoport = d.Kiscsoportvezetok.Cast<Person>().Count();
                    }
                    break;
                case "Alvocsoportvezetok":
                    if (!p.Alvocsoportvezeto)
                    {
                        p.Alvocsoportvezeto = true;
                        p.Alvocsoport = d.Alvocsoportvezetok.Cast<Person>().Count();
                    }
                    break;
                case "AddOrRemovePersonButton":
                    d.People.Remove(p);
                    break;
            }
            if (source.Name == "Kiscsoportvezetok" && (target.Name == "Team" || target.Name == "Ujoncok" || target.Name == "Egyeb"))
            {
                p.Kiscsoportvezeto = false;
                int numKiscsoportok = d.Kiscsoportvezetok.Cast<Person>().Count();
                d.SwapKiscsoports(p.Kiscsoport, numKiscsoportok - 1);
                foreach (Person q in d.Kiscsoport(numKiscsoportok - 1))
                    q.Kiscsoport = -1;
            }
            if (source.Name == "Alvocsoportvezetok" && (target.Name == "Team" || target.Name == "Ujoncok" || target.Name == "Egyeb"))
            {
                p.Alvocsoportvezeto = false;
                int numAlvocsoportok = d.Alvocsoportvezetok.Cast<Person>().Count();
                d.SwapAlvocsoports(p.Alvocsoport, numAlvocsoportok - 1);
                foreach (Person q in d.Alvocsoport(numAlvocsoportok - 1))
                    q.Alvocsoport = -1;
            }
            if (target.Name.StartsWith("kcs"))
                p.Kiscsoport = Int32.Parse(target.Name.Remove(0, 3)) - 1;
            if (target.Name.StartsWith("acs"))
                p.Alvocsoport = Int32.Parse(target.Name.Remove(0, 3)) - 1;
            if (target.Name == "nokcs")
                p.Kiscsoport = -1;
            if (target.Name == "noacs")
                p.Alvocsoport = -1;            

            ExtraDropCases(source, target, p);
        }

        #region Extras
        // 20HV: Minden szentendrei újonc mellett legyen szentendrei régenc
        private void ExtraDropCases(FrameworkElement source, FrameworkElement target, Person p)
        {
            if (AntiBonto.ViewModel.MainWindow.WeekendNumber != 20)
                return;
            if (target.Name == "Zugliget" || target.Name == "Szentendre")
            {
                d.Szentendre.Remove(p);
                d.MutuallyExclusiveGroups[0].Remove(p);
                var list = (ObservableCollection<Person>)((ItemsControl)target).ItemsSource;
                if (!list.Contains(p))
                    list.Add(p);
            }
            if ((source.Name == "Zugliget" || source.Name == "Szentendre") && source != target)
                ((ObservableCollection<Person>)((ItemsControl)source).ItemsSource).Remove(p);
        }
        #endregion
    }
}
