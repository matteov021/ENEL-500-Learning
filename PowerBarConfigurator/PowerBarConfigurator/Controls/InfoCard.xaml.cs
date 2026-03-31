using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PowerBarConfigurator.Controls
{
    /// <summary>
    /// A reusable WPF UserControl that represents an informational card UI component.
    /// 
    /// The InfoCard displays a title, a value, and a status indicator color,
    /// all of which are exposed as dependency properties for data binding and styling.
    /// 
    /// This control is intended for use in dashboards or configuration panels
    /// where concise, visually distinct pieces of information need to be presented.
    /// </summary>
    public partial class InfoCard : UserControl
    {
        // Constructor
        public InfoCard()
        {
            InitializeComponent();
        }

        // Title Dependency Property
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(InfoCard), new PropertyMetadata(string.Empty));

        // Value Dependency Property
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(InfoCard), new PropertyMetadata(string.Empty));

        // Status Color Dependency Property
        public static readonly DependencyProperty StatusColorProperty =
            DependencyProperty.Register(nameof(StatusColor), typeof(Brush), typeof(InfoCard), new PropertyMetadata(Brushes.Gray));

        // Title Getter and Setter
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // Value Getter and Setter
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        // Status Color Getter and Setter
        public Brush StatusColor
        {
            get => (Brush)GetValue(StatusColorProperty);
            set => SetValue(StatusColorProperty, value);
        }
    }
}