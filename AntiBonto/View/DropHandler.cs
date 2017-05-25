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
        private ViewModel.MainWindow d => (ViewModel.MainWindow)DataContext;

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
             || target.Name == "Kiscsoportvezetok" && d.Kiscsoportvezetok.Count() >= 14
             || target.Name == "Alvocsoportvezetok" && d.Alvocsoportvezetok.Count() >= 14)
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
                var acsvez = d.Alvocsoportvezetok.Single(q => q.Alvocsoport == acsn);
                dropInfo.Effects = (p.Nem != Nem.Undefined && p.Nem != acsvez.Nem) ? DragDropEffects.None : DragDropEffects.Move;
            }
            else
                dropInfo.Effects = DragDropEffects.Move;
        }

        /// <summary>
        /// Delete the groups of which p was the leader of
        /// </summary>
        private void Degroup(Person p)
        {
            if (p.Kiscsoportvezeto)
            {
                int numKiscsoportok = d.Kiscsoportvezetok.Count();
                d.SwapKiscsoports(p.Kiscsoport, numKiscsoportok - 1);
                d.Kiscsoport(numKiscsoportok - 1).ToList().ForEach(q => { q.Kiscsoport = -1; });
                p.Kiscsoportvezeto = false;
            }
            if (p.Alvocsoportvezeto)
            {
                if (d.Alvocsoportvezetok.Any(q => q.Nem == Nem.Undefined))
                {
                    int numAlvocsoportok = d.Alvocsoportvezetok.Count();
                    d.SwapAlvocsoports(p.Alvocsoport, numAlvocsoportok - 1);
                    d.Alvocsoport(numAlvocsoportok - 1).ToList().ForEach(q => { q.Alvocsoport = -1; });
                }
                else
                    d.Alvocsoport(p.Alvocsoport).ToList().ForEach(q => { q.Alvocsoport = -1; });
                p.Alvocsoportvezeto = false;
            }
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
                case "Zeneteamvezeto": d.Zeneteamvezeto = p; break;
                case "Lanyvezeto": d.Lanyvezeto = p; break;
                case "Fiuvezeto": d.Fiuvezeto = p; break;
                case "Ujoncok":
                    p.Type = PersonType.Ujonc;
                    Degroup(p);
                    break;
                case "Egyeb":
                    p.Type = PersonType.Egyeb;
                    Degroup(p);
                    break;
                case "AddOrRemovePersonButton":
                    d.People.Remove(p);
                    d.Edges.RemoveAll(e => e.Persons.Contains(p));
                    Degroup(p);
                    break;
                case "Team":
                    if (source.Name != "Kiscsoportvezetok" && source.Name != "Alvocsoportvezetok")
                        p.Type = PersonType.Teamtag;
                    break;
                case "Zeneteam":
                    if (p.Type != PersonType.Fiuvezeto && p.Type != PersonType.Lanyvezeto)
                        p.Type = PersonType.Zeneteamtag;
                    break;
                case "Kiscsoportvezetok":
                    Edge edge = d.Edges.FirstOrDefault(e => e.Persons.Contains(p) && e.Persons.First(q => q != p).Kiscsoportvezeto);
                    if (edge == null || MessageBox.Show(String.Format("Ez a megszorítás törlődni fog:\n\n{0}\n\nAkarod folytatni?", edge.ToString()), "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (!p.Kiscsoportvezeto)
                        {
                            p.Kiscsoportvezeto = true;
                            p.Kiscsoport = d.Kiscsoportvezetok.Count();
                        }
                        if (edge != null)
                            d.Edges.Remove(edge);
                    }
                    break;
                case "Alvocsoportvezetok":
                    if (!p.Alvocsoportvezeto)
                    {
                        p.Alvocsoportvezeto = true;
                        p.Alvocsoport = d.Alvocsoportvezetok.Select(q => q.Alvocsoport).DefaultIfEmpty(-1).Max() + 1;
                    }
                    break;                
            }
            if (source.Name == "Kiscsoportvezetok" && (target.Name == "Team" || target.Name == "Ujoncok" || target.Name == "Egyeb"))
            {
                p.Kiscsoportvezeto = false;
                int numKiscsoportok = d.Kiscsoportvezetok.Count();
                d.SwapKiscsoports(p.Kiscsoport, numKiscsoportok - 1);
                foreach (Person q in d.Kiscsoport(numKiscsoportok - 1))
                    q.Kiscsoport = -1;
            }
            if (source.Name == "Alvocsoportvezetok" && (target.Name == "Team" || target.Name == "Ujoncok" || target.Name == "Egyeb"))
            {
                p.Alvocsoportvezeto = false;
                if (d.Alvocsoportvezetok.Any(q => q.Nem == Nem.Undefined))
                {
                    int numAlvocsoportok = d.Alvocsoportvezetok.Count();
                    d.SwapAlvocsoports(p.Alvocsoport, numAlvocsoportok - 1);
                    foreach (Person q in d.Alvocsoport(numAlvocsoportok - 1))
                        q.Alvocsoport = -1;
                }
                else
                {
                    // No swapping here, because we reorder the sleeping groups anyway on opening of their tab
                    foreach (Person q in d.Alvocsoport(p.Alvocsoport))
                        q.Alvocsoport = -1;
                }
            }
            if (target.Name.StartsWith("kcs"))
                p.Kiscsoport = Int32.Parse(target.Name.Remove(0, 3)) - 1;
            if (target.Name.StartsWith("acs"))
            {
                p.Alvocsoport = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                ((ItemsControl)source).Items.Refresh(); // This updates the visualizing decorations for all others in the source
                ((ItemsControl)target).Items.Refresh(); // and target sleeping groups
            }
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
