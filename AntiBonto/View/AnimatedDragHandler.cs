using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Windows.Media.Animation;
using System;

namespace AntiBonto.View
{
    /// <summary>
    /// In order to be a DependencyObject, this uses composition instead of inheritance to delegate to DefaultDragHandler
    /// </summary>
    public class AnimatedDragHandler : FrameworkElement, IDragSource
    {
        public Storyboard Storyboard { get; set; }

        private DefaultDragHandler _base = new DefaultDragHandler();

        public static readonly DependencyProperty StoryboardProperty =
            DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(AnimatedDragHandler));

        public void DragCancelled()
        {
            _base.DragCancelled();
            Storyboard?.Begin();
        }
        public void Dropped(IDropInfo dropInfo)
        {
            _base.Dropped(dropInfo);
            Storyboard?.Begin();
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            _base.StartDrag(dragInfo);
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return _base.CanStartDrag(dragInfo);
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return _base.TryCatchOccurredException(exception);
        }
    }
}
