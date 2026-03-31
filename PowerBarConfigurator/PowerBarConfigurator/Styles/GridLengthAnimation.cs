using System.Windows;
using System.Windows.Media.Animation;

namespace PowerBarConfigurator.Styles
{
    /// <summary>
    /// Animates a GridLength value over time, enabling smooth transitions of grid column or row sizes in WPF.
    /// 
    /// The GridLengthAnimation class extends AnimationTimeline to interpolate between two GridLength values
    /// (From and To) using a linear progression. It is commonly used for animated resizing of grid columns
    /// or rows, such as when expanding or collapsing sidebars or panels.
    /// 
    /// This animation supports pixel-based GridLengths and integrates seamlessly with WPF's animation system.
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        // Target property type
        public override Type TargetPropertyType => typeof(GridLength);

        // Create a new instance of the animation
        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

        // DependencyProperty for From
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        // DependencyProperty for To
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        // Calculate the current value of the animation
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            // Ensure From and To values are set
            double from = From.Value;
            double to = To.Value;

            // Calculate the current value based on the animation progress
            return new GridLength(from + (to - from) * animationClock.CurrentProgress!.Value, GridUnitType.Pixel);
        }

        // From property
        public GridLength From
        {
            get => (GridLength)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        // To property
        public GridLength To
        {
            get => (GridLength)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }
    }
}