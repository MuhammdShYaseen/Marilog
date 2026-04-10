namespace Marilog.Shared.UI.Framework.Enums
{
    // =========================
    // BASIC LAYOUT
    // =========================

    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public enum Dock
    {
        Left,
        Top,
        Right,
        Bottom
    }

    // =========================
    // TEXT & TYPOGRAPHY
    // =========================

    /// <summary>
    /// Semantic font size scale — mirrors WPF/XAML named sizes.
    /// </summary>
    public enum FontSize
    {
        /// <summary>10px equivalent — fine print, labels</summary>
        Tiny,
        /// <summary>12px — captions, secondary text</summary>
        Small,
        /// <summary>14px — default body copy</summary>
        Regular,
        /// <summary>16px — slightly emphasized body</summary>
        Medium,
        /// <summary>20px — sub-headings</summary>
        Large,
        /// <summary>24px — section headings</summary>
        XLarge,
        /// <summary>32px — page titles</summary>
        XXLarge,
        /// <summary>48px — display / hero text</summary>
        Display
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    public enum TextWrapping
    {
        NoWrap,
        Wrap,
        WrapWithOverflow
    }

    public enum FontWeight
    {
        Normal,
        Bold,
        SemiBold,
        Light,
        Medium,
        Thin
    }

    // =========================
    // ALIGNMENT
    // =========================

    /// <summary>
    /// Horizontal alignment — mirrors WPF HorizontalAlignment.
    /// </summary>
    public enum HAlign
    {
        Left,
        Center,
        Right,
        Stretch
    }

    /// <summary>
    /// Vertical alignment — mirrors WPF VerticalAlignment.
    /// </summary>
    public enum VAlign
    {
        Top,
        Center,
        Bottom,
        Stretch
    }

    // =========================
    // SPACING & SIZING
    // =========================

    public enum Spacing
    {
        None,
        Tiny,
        Small,
        Medium,
        Large,
        XLarge
    }

    public enum Visibility
    {
        Visible,
        Hidden,
        Collapsed
    }

    public enum Stretch
    {
        None,
        Fill,
        Uniform,
        UniformToFill
    }

    // =========================
    // GRID SYSTEM
    // =========================

    public enum GridUnitType
    {
        /// <summary>Fixed pixel size</summary>
        Pixel,
        /// <summary>Star sizing — proportional remaining space</summary>
        Star,
        /// <summary>Size to content</summary>
        Auto
    }

    // =========================
    // BUTTONS & INTERACTION
    // =========================

    public enum ButtonVariant
    {
        /// <summary>Solid filled background</summary>
        Filled,
        /// <summary>Outlined border only</summary>
        Outlined,
        /// <summary>Text-only, no background or border</summary>
        Text
    }

    public enum ButtonSize
    {
        Small,
        Medium,
        Large
    }

    public enum IconPlacement
    {
        Left,
        Right
    }

    // =========================
    // CARDS & SURFACES
    // =========================

    /// <summary>
    /// Card elevation levels — mirrors WPF/Material depth.
    /// </summary>
    public enum ElevationLevel
    {
        None = 0,
        Low = 1,
        Medium = 4,
        High = 8,
        Floating = 16
    }

    // =========================
    // COLORS & THEMING
    // =========================

    /// <summary>
    /// Semantic theme colour — maps to MudBlazor.Color internally.
    /// </summary>
    public enum ThemeColor
    {
        Default,
        Primary,
        Secondary,
        Success,
        Warning,
        Error,
        Info,
        Dark,
        Inherit,
        Transparent
    }

    // =========================
    // INPUT & FORM CONTROLS
    // =========================

    public enum InputState
    {
        Enabled,
        Disabled,
        ReadOnly
    }

    public enum ControlState
    {
        Normal,
        Hover,
        Focused,
        Pressed,
        Disabled
    }

    public enum CursorType
    {
        Default,
        Pointer,
        Text,
        NotAllowed,
        Wait,
        Move,
        Resize
    }

    public enum SelectionMode
    {
        Single,
        Multiple,
        Extended,
        None
    }

    // =========================
    // SCROLLING
    // =========================

    public enum ScrollBarVisibility
    {
        Disabled,
        Auto,
        Hidden,
        Visible
    }

    // =========================
    // POPUPS & OVERLAYS
    // =========================

    public enum PopupPlacement
    {
        Bottom,
        Top,
        Left,
        Right,
        BottomStart,
        BottomEnd,
        TopStart,
        TopEnd,
        LeftStart,
        LeftEnd,
        RightStart,
        RightEnd
    }

    // =========================
    // DATA BINDING
    // =========================

    public enum BindingMode
    {
        OneWay,
        TwoWay,
        OneTime,
        OneWayToSource,
        Default
    }

    public enum UpdateSourceTrigger
    {
        Default,
        PropertyChanged,
        LostFocus,
        Explicit
    }

    public enum ValidationMode
    {
        None,
        OnPropertyChanged,
        OnLostFocus,
        Explicit
    }

    // =========================
    // SORTING & GROUPING
    // =========================

    public enum SortDirection
    {
        None,
        Ascending,
        Descending
    }

    // =========================
    // IMAGES & MEDIA
    // =========================

    public enum ImageFit
    {
        Contain,
        Cover,
        Fill,
        ScaleDown,
        None
    }

    // =========================
    // PROGRESS & STATUS
    // =========================

    public enum ProgressType
    {
        Linear,
        Circular
    }

    public enum ProgressSize
    {
        Small,
        Medium,
        Large
    }

    // =========================
    // NAVIGATION
    // =========================

    public enum NavMenuMode
    {
        Vertical,
        Horizontal,
        Compact,
        Mini
    }

    // =========================
    // DIALOGS
    // =========================

    public enum DialogSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge,
        FullScreen
    }

    public enum DialogPosition
    {
        Center,
        Top,
        Bottom,
        Custom
    }

    public enum WrapMode
    {
        NoWrap,

        /// <summary>
        /// Normal wrapping.
        /// </summary>
        Wrap,

        /// <summary>
        /// Wrap with overflow breaking.
        /// </summary>
        WrapWithOverflow
    }
}
