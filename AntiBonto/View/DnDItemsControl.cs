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
    }
}
