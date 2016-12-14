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
        public bool ColorUjoncs { get; set; } = false;
        public static readonly DependencyProperty ColorUjoncsProperty =
            DependencyProperty.Register("ColorUjoncs", typeof(bool), typeof(DnDItemsControl));

        public bool BoldKiscsoportvezetok { get; set; } = false;
        public static readonly DependencyProperty BoldKiscsoportvezetokProperty =
            DependencyProperty.Register("BoldKiscsoportvezetok", typeof(bool), typeof(DnDItemsControl));

        public bool BoldAlvocsoportvezetok { get; set; } = false;
        public static readonly DependencyProperty BoldAlvocsoportvezetokProperty =
            DependencyProperty.Register("BoldAlvocsoportvezetok", typeof(bool), typeof(DnDItemsControl));
    }
}
