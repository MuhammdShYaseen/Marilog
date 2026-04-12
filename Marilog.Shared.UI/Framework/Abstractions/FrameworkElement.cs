using System.Globalization;
using System.Text;

namespace Marilog.Shared.UI.Framework.Abstractions
{
    // =========================
    // SUPPORTING TYPES
    // =========================

    public struct Size
    {
        public double Width { get; }
        public double Height { get; }
        public static Size Zero => new(0, 0);
        public Size(double w, double h) => (Width, Height) = (w, h);
    }

    // =========================
    // MAIN FRAMEWORKELEMENT
    // =========================

    /// <summary>
    /// Base class for all UI.Framework components — mirrors WPF FrameworkElement.
    ///
    /// Changes from previous version:
    ///   - Removed all MudBlazor references (ToMudColor, ResolveMudColor, using MudBlazor)
    ///   - ThemeColor now maps to Tailwind classes via TailwindHelper
    ///   - VisibilityMode moved to Enums as VisibilityMode (already in FrameworkEnums)
    ///   - BuildBaseClass() now produces Tailwind classes
    ///   - BuildBaseStyle() kept for width/height/margin — Tailwind handles the rest
    /// </summary>
    public abstract class FrameworkElement : ComponentBase, IDisposable
    {
        // =========================
        // 1. VISIBILITY
        // =========================

        private VisibilityMode _lastVisibility;
        [Parameter] public VisibilityMode Visibility { get; set; } = VisibilityMode.Visible;

        // =========================
        // 2. LAYOUT
        // =========================

        private Thickness _lastMargin;
        [Parameter] public Thickness Margin { get; set; } = Thickness.Zero;

        private Thickness _lastPadding;
        [Parameter] public Thickness Padding { get; set; } = Thickness.Zero;

        private double? _lastWidth;
        [Parameter] public double? Width { get; set; }

        private double? _lastHeight;
        [Parameter] public double? Height { get; set; }

        [Parameter] public double? MinWidth { get; set; }
        [Parameter] public double? MaxWidth { get; set; }
        [Parameter] public double? MinHeight { get; set; }
        [Parameter] public double? MaxHeight { get; set; }

        [Parameter] public HAlign HorizontalAlignment { get; set; } = HAlign.Stretch;
        [Parameter] public VAlign VerticalAlignment { get; set; } = VAlign.Top;
        [Parameter] public int? ZIndex { get; set; }

        // =========================
        // 3. INTERACTION STATE
        // =========================

        private bool _lastIsEnabled;
        [Parameter] public bool IsEnabled { get; set; } = true;
        [Parameter] public CursorType Cursor { get; set; } = CursorType.Default;
        [Parameter] public int TabIndex { get; set; }

        // =========================
        // 4. STYLING
        // =========================

        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string? ElementId { get; set; }
        [Parameter] public ThemeColor Color { get; set; } = ThemeColor.Default;
        [Parameter] public string? ColorHex { get; set; }

        [Parameter] public Dictionary<string, object>? DataAttributes { get; set; }

        // =========================
        // 5. EVENTS
        // =========================

        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

        //--Command------------------
        [CascadingParameter]public CommandContext? CommandContext { get; set; }

        // =========================
        // 6. RENDER BATCHING
        // =========================

        private bool _isRenderingSuspended;
        private int _pendingRenderRequests;
        private CancellationTokenSource? _renderThrottleCts;

        protected IDisposable SuspendRendering() => new RenderSuspensionToken(this);

        private sealed class RenderSuspensionToken : IDisposable
        {
            private readonly FrameworkElement _owner;
            public RenderSuspensionToken(FrameworkElement owner)
            {
                _owner = owner;
                _owner._isRenderingSuspended = true;
            }
            public void Dispose() => _owner.ResumeRendering();
        }

        private void ResumeRendering()
        {
            _isRenderingSuspended = false;
            if (_pendingRenderRequests > 0) StateHasChanged();
            _pendingRenderRequests = 0;
        }

        protected void RequestRender(TimeSpan? throttle = null)
        {
            if (_isRenderingSuspended) { _pendingRenderRequests++; return; }

            if (throttle.HasValue)
            {
                _renderThrottleCts?.Cancel();
                _renderThrottleCts = new CancellationTokenSource();
                Task.Delay(throttle.Value, _renderThrottleCts.Token)
                    .ContinueWith(
                        _ => InvokeAsync(StateHasChanged),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.Default);
            }
            else
            {
                StateHasChanged();
            }
        }

        protected override bool ShouldRender()
            => !_isRenderingSuspended && base.ShouldRender();

        // =========================
        // 7. CLASS / STYLE CACHING
        // =========================

        private string? _cachedClass;
        private int _classVersion = 0;
        private int _lastClassVersion = -1;

        private string? _cachedStyle;
        private int _styleVersion = 0;
        private int _lastStyleVersion = -1;

        protected void InvalidateVisual()
        {
            _classVersion++;
            _styleVersion++;
            InvalidateLayout();
        }

        /// <summary>
        /// Returns cached Tailwind class string — rebuilt only when parameters change.
        /// Call this in every component's class attribute.
        /// </summary>
        protected virtual string GetCachedClass()
        {
            if (_classVersion != _lastClassVersion || _cachedClass is null)
            {
                _cachedClass = BuildBaseClass();
                _lastClassVersion = _classVersion;
            }
            return _cachedClass;
        }

        /// <summary>
        /// Returns cached inline style string — rebuilt only when parameters change.
        /// Only used for values that Tailwind cannot express (arbitrary px sizes).
        /// </summary>
        protected string GetCachedStyle()
        {
            if (_styleVersion != _lastStyleVersion || _cachedStyle is null)
            {
                _cachedStyle = BuildBaseStyle();
                _lastStyleVersion = _styleVersion;
            }
            return _cachedStyle;
        }

        protected virtual Dictionary<string, object> GetDataAttributes()
        {
            return new();
        }

        // =========================
        // 8. LAYOUT (Measure / Arrange)
        // =========================

        private Size _desiredSize = Size.Zero;
        protected bool IsMeasureValid { get; private set; }
        protected bool IsArrangeValid { get; private set; }
        public Size DesiredSize => _desiredSize;

        protected virtual Size Measure(Size available)
        {
            if (Visibility == VisibilityMode.Collapsed)
            {
                _desiredSize = Size.Zero;
                IsMeasureValid = true;
                return Size.Zero;
            }

            var w = Width ?? available.Width;
            var h = Height ?? available.Height;

            if (MinWidth.HasValue) w = Math.Max(w, MinWidth.Value);
            if (MaxWidth.HasValue) w = Math.Min(w, MaxWidth.Value);
            if (MinHeight.HasValue) h = Math.Max(h, MinHeight.Value);
            if (MaxHeight.HasValue) h = Math.Min(h, MaxHeight.Value);

            _desiredSize = new Size(w, h);
            IsMeasureValid = true;
            return _desiredSize;
        }

        protected virtual Size Arrange(Size finalSize)
        {
            _desiredSize = finalSize;
            IsArrangeValid = true;
            return finalSize;
        }

        protected void InvalidateLayout()
        {
            IsMeasureValid = false;
            IsArrangeValid = false;
            RequestRender();
        }

        // =========================
        // 9. PARAMETER CHANGE DETECTION
        // =========================

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            var layoutChanged = false;
            var visualChanged = false;

            if (_lastVisibility != Visibility) { _lastVisibility = Visibility; visualChanged = true; }
            if (!_lastMargin.Equals(Margin)) { _lastMargin = Margin; layoutChanged = true; }
            if (!_lastPadding.Equals(Padding)) { _lastPadding = Padding; layoutChanged = true; }
            if (_lastWidth != Width) { _lastWidth = Width; layoutChanged = true; }
            if (_lastHeight != Height) { _lastHeight = Height; layoutChanged = true; }
            if (_lastIsEnabled != IsEnabled) { _lastIsEnabled = IsEnabled; visualChanged = true; }

            if (layoutChanged) InvalidateLayout();
            if (visualChanged) InvalidateVisual();
        }

        // =========================
        // 10. BUILDERS
        // =========================

        /// <summary>
        /// Builds Tailwind class string from base parameters.
        /// Derived components override this and call base + append their own classes.
        /// </summary>
        protected virtual string BuildBaseClass()
        {
            return TailwindHelper.Classes(
                // alignment
                TailwindHelper.HAlignSelfClass(HorizontalAlignment),
                // cursor
                IsEnabled
                    ? TailwindHelper.CursorClass(Cursor)
                    : "cursor-not-allowed",
                // disabled state
                !IsEnabled ? "opacity-60 pointer-events-none" : null,
                // color
                Color != ThemeColor.Default
                    ? TailwindHelper.TextColorClass(Color)
                    : null,
                // consumer classes
                Class
            );
        }

        /// <summary>
        /// Builds inline style string — only for values Tailwind cannot express
        /// (arbitrary pixel widths/heights, margins from Thickness, etc.).
        /// </summary>
        protected virtual string BuildBaseStyle()
        {
            var sb = new StringBuilder();

            if (Margin != Thickness.Zero) sb.Append($"margin:{Margin.ToCss()};");
            if (Padding != Thickness.Zero) sb.Append($"padding:{Padding.ToCss()};");

            if (Width.HasValue) sb.Append(CultureInfo.InvariantCulture, $"width:{Width}px;");
            if (Height.HasValue) sb.Append(CultureInfo.InvariantCulture, $"height:{Height}px;");
            if (MinWidth.HasValue) sb.Append(CultureInfo.InvariantCulture, $"min-width:{MinWidth}px;");
            if (MaxWidth.HasValue) sb.Append(CultureInfo.InvariantCulture, $"max-width:{MaxWidth}px;");
            if (MinHeight.HasValue) sb.Append(CultureInfo.InvariantCulture, $"min-height:{MinHeight}px;");
            if (MaxHeight.HasValue) sb.Append(CultureInfo.InvariantCulture, $"max-height:{MaxHeight}px;");

            switch (Visibility)
            {
                case VisibilityMode.Hidden: sb.Append("visibility:hidden;"); break;
                case VisibilityMode.Collapsed: sb.Append("display:none;"); break;
            }

            if (ZIndex.HasValue) sb.Append($"z-index:{ZIndex.Value};");

            // ColorHex override — arbitrary hex not expressible in Tailwind at build time
            if (!string.IsNullOrWhiteSpace(ColorHex))
                sb.Append($"color:{ColorHex};");

            if (!string.IsNullOrWhiteSpace(Style)) sb.Append(Style);

            return sb.ToString();
        }

        // =========================
        // 11. HTML ATTRIBUTE HELPERS
        // =========================

        protected Dictionary<string, object> GetHtmlAttributes()
        {
            var attrs = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(ElementId))
                attrs["id"] = ElementId;

            if (TabIndex != 0)
                attrs["tabindex"] = TabIndex;

            if (DataAttributes is not null)
                foreach (var kvp in DataAttributes)
                    attrs[$"data-{kvp.Key}"] = kvp.Value;

            return attrs;
        }

        // =========================
        // 12. DISPOSE
        // =========================

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderThrottleCts?.Cancel();
                _renderThrottleCts?.Dispose();
                DataAttributes?.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
