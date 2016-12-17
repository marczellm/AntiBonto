using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Windows.Media.Animation;

namespace AntiBonto.View
{
    public class AnimatedDragHandler : DefaultDragHandler
    {
        public Storyboard Animation { get; set; }

        public static readonly DependencyProperty AnimationProperty =
            DependencyProperty.Register("Animation", typeof(Storyboard), typeof(AnimatedDragHandler));

        public override void DragCancelled()
        {
            base.DragCancelled();
            Animation.Begin();
        }
        public override void Dropped(IDropInfo dropInfo)
        {
            base.Dropped(dropInfo);
            Animation.Begin();
        }
    }
}
