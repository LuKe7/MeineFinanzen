// 27.11.2018  MeineFinanzen.Helpers.Extended.cs
using System.Windows;
using System.Windows.Controls;
namespace MeineFinanzen.Helpers {
    public class ColumnDefinitionExtended : ColumnDefinition {
        // Variables
        public static DependencyProperty VisibleProperty;
        // Properties
        public bool Visible { get { return (bool)GetValue(VisibleProperty); } set { SetValue(VisibleProperty, value); } }
        // Constructors
        static ColumnDefinitionExtended() {
            VisibleProperty = DependencyProperty.Register("Visible", typeof(bool),
                typeof(ColumnDefinitionExtended), new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));
            WidthProperty.OverrideMetadata(typeof(ColumnDefinitionExtended),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null, new CoerceValueCallback(CoerceWidth)));
            MinWidthProperty.OverrideMetadata(typeof(ColumnDefinitionExtended),
                new FrameworkPropertyMetadata((double)0, null, new CoerceValueCallback(CoerceMinWidth)));
        }
        // Get/Set
        public static void SetVisible(DependencyObject obj, bool nVisible) {
            obj.SetValue(VisibleProperty, nVisible);
        }
        public static bool GetVisible(DependencyObject obj) {
            return (bool)obj.GetValue(VisibleProperty);
        }
        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            obj.CoerceValue(ColumnDefinition.WidthProperty);
            obj.CoerceValue(ColumnDefinition.MinWidthProperty);
        }
        static object CoerceWidth(DependencyObject obj, object nValue) {
            return (((ColumnDefinitionExtended)obj).Visible) ? nValue : new GridLength(0);
        }
        static object CoerceMinWidth(DependencyObject obj, object nValue) {
            return (((ColumnDefinitionExtended)obj).Visible) ? nValue : (double)0;
        }
    }
    public class RowDefinitionExtended : RowDefinition {
        public static DependencyProperty VisibleProperty;
        public bool Visible { get { return (bool)GetValue(VisibleProperty); } set { SetValue(VisibleProperty, value); } }
        static RowDefinitionExtended() {
            VisibleProperty = DependencyProperty.Register("Visible", typeof(bool), typeof(RowDefinitionExtended),
                new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));
            HeightProperty.OverrideMetadata(typeof(RowDefinitionExtended),
            new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null, new CoerceValueCallback(CoerceHeight)));
            MinHeightProperty.OverrideMetadata(typeof(RowDefinitionExtended),
                new FrameworkPropertyMetadata((double)0, null, new CoerceValueCallback(CoerceMinHeight)));
        }
        // Get/Set
        public static void SetVisible(DependencyObject obj, bool nVisible) {
            obj.SetValue(VisibleProperty, nVisible);
        }
        public static bool GetVisible(DependencyObject obj) {
            return (bool)obj.GetValue(VisibleProperty);
        }
        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            obj.CoerceValue(HeightProperty);
            obj.CoerceValue(MinHeightProperty);
        }
        static object CoerceHeight(DependencyObject obj, object nValue) {
            return (((RowDefinitionExtended)obj).Visible) ? nValue : new GridLength(0);
        }
        static object CoerceMinHeight(DependencyObject obj, object nValue) {
            return (((RowDefinitionExtended)obj).Visible) ? nValue : (double)0;
        }
    }
}