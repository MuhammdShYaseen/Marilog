namespace Marilog.Shared.UI.Framework.Enums
{
    // =========================
    // LAYOUT
    // =========================
    public enum WrapMode
    {
        /// <summary>
        /// Default wrapping behavior (normal line wrapping).
        /// </summary>
        Wrap,

        /// <summary>
        /// No wrapping — text stays on a single line.
        /// Equivalent to WPF TextWrapping.NoWrap.
        /// </summary>
        NoWrap,

        /// <summary>
        /// Wrap text and allow breaking long words if needed.
        /// Similar to overflow-wrap:anywhere in CSS.
        /// </summary>
        WrapWithOverflow
    }
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
    // ALIGNMENT
    // =========================

    /// <summary>Horizontal alignment — mirrors WPF HorizontalAlignment.</summary>
    public enum HAlign
    {
        Left,
        Center,
        Right,
        Stretch
    }

    /// <summary>Vertical alignment — mirrors WPF VerticalAlignment.</summary>
    public enum VAlign
    {
        Top,
        Center,
        Bottom,
        Stretch
    }

    // =========================
    // TYPOGRAPHY
    // =========================

    public enum FontSize
    {
        Tiny,       // 10px — fine print
        Small,      // 12px — captions
        Regular,    // 14px — body default
        Medium,     // 16px — slightly emphasized
        Large,      // 20px — sub-headings
        XLarge,     // 24px — section headings
        XXLarge,    // 32px — page titles
        Display     // 48px — hero text
    }

    public enum FontWeight
    {
        Thin,
        Light,
        Normal,
        Medium,
        SemiBold,
        Bold
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    /// <summary>
    /// Text wrapping — unified enum (was split between TextWrapping and WrapMode).
    /// Use this everywhere in UI.Framework.
    /// </summary>
    public enum TextWrap
    {
        NoWrap,
        Wrap,
        WrapWithOverflow
    }

    // =========================
    // SPACING & SIZING
    // =========================

    public enum SpacingSize
    {
        None,   // 0px
        Tiny,   // 4px
        Small,  // 8px
        Medium, // 16px
        Large,  // 24px
        XLarge  // 32px
    }

    public enum Stretch
    {
        None,
        Fill,
        Uniform,
        UniformToFill
    }

    // =========================
    // VISIBILITY
    // =========================

    public enum VisibilityMode
    {
        Visible,
        Hidden,     // occupies space but invisible
        Collapsed   // removed from layout entirely
    }

    // =========================
    // GRID
    // =========================

    public enum GridUnitType
    {
        Pixel,  // fixed px
        Star,   // proportional (1fr)
        Auto    // size to content
    }

    // =========================
    // CURSOR
    // =========================

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

    // =========================
    // BUTTONS & INTERACTION
    // =========================

    public enum ButtonVariant
    {
        Filled,     // solid background
        Outlined,   // border only
        Text,
        Ghost,
        Subtle// no background or border
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
    // COLORS & THEMING
    // =========================

    /// <summary>
    /// Semantic color intent.
    /// Maps to Tailwind utility classes via TailwindHelper — no MudBlazor dependency.
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
        Light,
        Inherit,
        Transparent
    }

    // =========================
    // CARDS & SURFACES
    // =========================

    public enum ElevationLevel
    {
        None = 0,
        Low = 1,
        Medium = 4,
        High = 8,
        Floating = 16
    }

    // =========================
    // INPUT & FORM CONTROLS
    // =========================

    public enum InputVariant
    {
        Outlined,
        Filled,
        Underline
    }

    public enum SelectionMode
    {
        None,
        Single,
        Multiple,
        Extended
    }

    public enum ValidationMode
    {
        None,
        OnChange,
        OnBlur,
        Explicit
    }

    // =========================
    // SCROLLING
    // =========================

    public enum ScrollBarVisibility
    {
        Auto,
        Always,
        Hidden,
        Disabled
    }

    // =========================
    // SORTING
    // =========================

    public enum SortDirection
    {
        None,
        Ascending,
        Descending
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
        TopEnd
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

    // =========================
    // IMAGES
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
    // PROGRESS
    // =========================

    public enum ProgressType
    {
        Linear,
        Circular
    }
}
