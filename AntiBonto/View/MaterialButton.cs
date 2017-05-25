using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AntiBonto.View
{
    public class MaterialButton : Button
    {
        static MaterialButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialButton), new FrameworkPropertyMetadata(typeof(MaterialButton)));
        }

        public Storyboard ButtonRotateAnimation => (Storyboard)Template.Resources["ButtonRotateAnimation"];
        public Storyboard ButtonRotateBackAnimation => (Storyboard)Template.Resources["ButtonRotateBackAnimation"];
    }
}
