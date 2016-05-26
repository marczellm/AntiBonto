using System.Windows;
using System.Windows.Controls;

namespace AntiBonto.View
{
    public class DnDItemsControl : ItemsControl
    {
        static DnDItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DnDItemsControl), new FrameworkPropertyMetadata(typeof(DnDItemsControl)));
        }
        public bool KiscsoportView { get; set; } = false;
        public static readonly DependencyProperty KiscsoportViewProperty =
            DependencyProperty.Register("KiscsoportView", typeof(bool), typeof(DnDItemsControl));
    }
}
