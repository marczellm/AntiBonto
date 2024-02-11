using System;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.ComponentModel;

namespace AntiBonto.View
{
    class DropHandler : FrameworkElement, IDropTarget
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
        private ViewModel.MainWindow d => (ViewModel.MainWindow)DataContext;
        /// <summary>
        /// Set where drops are allowed
        /// </summary>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = null;
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            if (dropInfo.Data is not Person)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            var p = (Person)dropInfo.Data;

            if (source.Name == "PeopleView" && target.Name == "AddOrRemovePersonButton")
            {
                dropInfo.Effects = DragDropEffects.Move;
                return;
            }
            else if (p.Type != PersonType.Newcomer
                     && p.Type != PersonType.Others
                     && (p.Sex == Sex.Girl && target.Name == "GirlLeader" || p.Sex == Sex.Boy && target.Name == "BoyLeader" || target.Name == "MusicLeader"))
            {
                dropInfo.Effects = DragDropEffects.Move;
                return;
            }
            else if (target is DnDItemsControl dnd && dnd.DragOver2 != null)
            {
                var res = dnd.DragOver2(p, source, target);
                dropInfo.Effects = res.effect;
                d.StatusText = res.message;
                return;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Delete the groups of which p was the leader of
        /// </summary>
        private void Degroup(Person p)
        {
            if (p.SharingGroupLeader)
            {
                int numSharingGroups = d.SharingGroupLeaders.Count();
                d.SwapSharingGroups(p.SharingGroup, numSharingGroups - 1);
                d.SharingGroup(numSharingGroups - 1).ToList().ForEach(q => { q.SharingGroup = -1; });
                p.SharingGroupLeader = false;
            }
            if (p.SleepingGroupLeader)
            {
                if (d.SleepingGroupLeaders.Any(q => q.Sex == Sex.Undefined))
                {
                    int numSleepingGroups = d.SleepingGroupLeaders.Count();
                    d.SwapSleepingGroups(p.SleepingGroup, numSleepingGroups - 1);
                    d.SleepingGroup(numSleepingGroups - 1).ToList().ForEach(q => { q.SleepingGroup = -1; });
                }
                else
                    d.SleepingGroup(p.SleepingGroup).ToList().ForEach(q => { q.SleepingGroup = -1; });
                p.SleepingGroupLeader = false;
            }
        }

        /// <summary>
        /// Make the necessary data changes upon drop
        /// </summary>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            if (source == target)
                return;
            Person p = (Person)dropInfo.Data;
            switch (target.Name)
            {
                case "Boys": p.Sex = Sex.Boy; break;
                case "Girls": p.Sex = Sex.Girl; break;
                case "SexUndefined": p.Sex = Sex.Undefined; break;                
                case "MusicLeader": d.MusicLeader = p; break;
                case "GirlLeader": d.GirlLeader = p; break;
                case "BoyLeader": d.BoyLeader = p; break;
                case "Newcomers":
                    p.Type = PersonType.Newcomer;
                    Degroup(p);
                    break;
                case "Others":
                    p.Type = PersonType.Others;
                    Degroup(p);
                    break;
                case "AddOrRemovePersonButton":
                    d.People.Remove(p);
                    d.Edges.RemoveAll(e => e.Persons.Contains(p));
                    Degroup(p);
                    break;
                case "Team":
                    if (p.Type == PersonType.GirlLeader && source.Name == "GirlLeader")
                        d.GirlLeader = null;
                    else if (p.Type == PersonType.BoyLeader && source.Name == "BoyLeader")
                        d.BoyLeader = null;
                    else if (p.Type == PersonType.MusicLeader && source.Name == "MusicLeader")
                        d.MusicLeader = null;
                    else if (source.Name != "SharingGroupLeaders" && source.Name != "SleepingGroupLeaders")
                        p.Type = PersonType.Team;                    
                    break;
                case "MusicTeam":
                    if (p.Type != PersonType.GirlLeader && p.Type != PersonType.BoyLeader && p.Type != PersonType.MusicLeader)
                        p.Type = PersonType.MusicTeam;
                    break;
                case "SharingGroupLeaders":
                    Edge edge = d.Edges.FirstOrDefault(e => e.Persons.Contains(p) && e.Persons.First(q => q != p).SharingGroupLeader);
                    if (edge == null || MessageBox.Show(String.Format("Ez a megszorítás törlődni fog:\n\n{0}\n\nAkarod folytatni?", edge.ToString()), "AntiBonto", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (!p.SharingGroupLeader)
                        {
                            p.SharingGroupLeader = true;
                            p.SharingGroup = d.SharingGroupLeaders.Count();
                        }
                        if (edge != null)
                            d.Edges.Remove(edge);
                    }
                    break;
                case "SleepingGroupLeaders":
                    if (!p.SleepingGroupLeader)
                    {
                        p.SleepingGroupLeader = true;
                        p.SleepingGroup = d.SleepingGroupLeaders.Select(q => q.SleepingGroup).DefaultIfEmpty(-1).Max() + 1;
                    }
                    break;                
            }
            if (source.Name == "SharingGroupLeaders" && (target.Name == "Team" || target.Name == "Newcomers" || target.Name == "Others"))
            {
                p.SharingGroupLeader = false;
                int numSharingGroups = d.SharingGroupLeaders.Count();
                d.SwapSharingGroups(p.SharingGroup, numSharingGroups - 1);
                foreach (Person q in d.SharingGroup(numSharingGroups - 1))
                    q.SharingGroup = -1;
            }
            if (source.Name == "SleepingGroupLeaders" && (target.Name == "Team" || target.Name == "Newcomers" || target.Name == "Others"))
            {
                p.SleepingGroupLeader = false;
                if (d.SleepingGroupLeaders.Any(q => q.Sex == Sex.Undefined))
                {
                    int numSleepingGroups = d.SleepingGroupLeaders.Count();
                    d.SwapSleepingGroups(p.SleepingGroup, numSleepingGroups - 1);
                    foreach (Person q in d.SleepingGroup(numSleepingGroups - 1))
                        q.SleepingGroup = -1;
                }
                else
                {
                    // No swapping here, because we reorder the sleeping groups anyway on opening of their tab
                    foreach (Person q in d.SleepingGroup(p.SleepingGroup))
                        q.SleepingGroup = -1;
                }
            }
            if (target is DnDItemsControl temp && d.SharingGroups?.Contains(temp.ItemsSource) == true)
            {
                p.SharingGroup = d.SharingGroups.IndexOf(temp.ItemsSource as ICollectionView);
            }                
            if (target is DnDItemsControl temp2 && (d.BoySleepingGroups?.Contains(temp2.ItemsSource) == true || d.GirlSleepingGroups?.Contains(temp2.ItemsSource) == true))
            {
                p.SleepingGroup = temp2.ItemsSource.Cast<Person>().First().SleepingGroup;
                ((ItemsControl)source).Items.Refresh(); // This updates the visualizing decorations for all others in the source
                ((ItemsControl)target).Items.Refresh(); // and target sleeping groups
            }
            if (target.Name == "sharingGroupless")
            {
                p.SharingGroup = -1;
                ((ItemsControl)source).Items.Refresh();
            } 
            else if (target.Name.StartsWith("sleepingGroupless"))
            {
                p.SleepingGroup = -1;
                ((ItemsControl)source).Items.Refresh();
            }
        }
    }
}
