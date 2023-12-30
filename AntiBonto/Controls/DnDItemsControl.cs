/// <summary>
/// the second and third arg are the controls acting as drag source and target respectively
/// </summary>
global using DragOverCallback = System.Func<AntiBonto.Person, System.Windows.FrameworkElement, System.Windows.FrameworkElement, AntiBonto.View.DragOverResult>;

using System;
using System.Windows;
using System.Windows.Controls;

namespace AntiBonto.View
{

    public struct DragOverResult
    {
        public string message;
        public DragDropEffects effect;
    }

    public class DnDItemsControl : HeaderedItemsControl
    {
        static DnDItemsControl()
        {
            // Metadata needs to be overriden in static constructor to indicate that the style is declared under Themes/Generic.xaml.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DnDItemsControl), new FrameworkPropertyMetadata(typeof(DnDItemsControl)));
        }

        public bool ColorNewcomers { get; set; } = false;
        public static readonly DependencyProperty ColorNewcomersProperty =
            DependencyProperty.Register("ColorNewcomers", typeof(bool), typeof(DnDItemsControl));

        public bool ColorLeaders { get; set; } = false;
        public static readonly DependencyProperty ColorLeadersProperty =
            DependencyProperty.Register("ColorLeaders", typeof(bool), typeof(DnDItemsControl));

        public bool ColorSharingGroups { get; set; } = false;
        public static readonly DependencyProperty ColorSharingGroupsProperty =
            DependencyProperty.Register("ColorSharingGroups", typeof(bool), typeof(DnDItemsControl));

        public bool VisualizeConflicts { get; set; } = false;
        public static readonly DependencyProperty VisualizeConflictsProperty =
            DependencyProperty.Register("VisualizeConflicts", typeof(bool), typeof(DnDItemsControl));

        public bool BoldSharingGroupLeaders { get; set; } = false;
        public static readonly DependencyProperty BoldSharingGroupLeadersProperty =
            DependencyProperty.Register("BoldSharingGroupLeaders", typeof(bool), typeof(DnDItemsControl));

        public bool BoldSleepingGroupLeaders { get; set; } = false;
        public static readonly DependencyProperty BoldSleepingGroupLeadersProperty =
            DependencyProperty.Register("BoldSleepingGroupLeaders", typeof(bool), typeof(DnDItemsControl));

        public bool Pinnable { get; set; } = false;
        public static readonly DependencyProperty PinnableProperty =
            DependencyProperty.Register("Pinnable", typeof(bool), typeof(DnDItemsControl));

        public bool Scrollable { get; set; } = false;
        public static readonly DependencyProperty ScrollableProperty =
            DependencyProperty.Register("Scrollable", typeof(bool), typeof(DnDItemsControl));

        public static readonly DependencyProperty DragOver2Property =
            DependencyProperty.Register("DragOver2", typeof(DragOverCallback), typeof(DnDItemsControl));
        public DragOverCallback DragOver2
        {
            get => (DragOverCallback)GetValue(DragOver2Property);
            set => SetValue(DragOver2Property, value);
        }
    }
}
