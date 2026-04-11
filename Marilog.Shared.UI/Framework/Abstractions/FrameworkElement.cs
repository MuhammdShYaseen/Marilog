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

    public interface ILayoutAware
    {
        Size Measure(Size availableSize);
        Size Arrange(Size finalSize);
        Size DesiredSize { get; }
    }

    // =========================
    // MAIN FRAMEWORKELEMENT
    // =========================

    /// <summary>
    /// Base class for all UI.Framework components.
    /// Mirrors WPF FrameworkElement with layout, styling, and performance features.
    /// </summary>
    public abstract class FrameworkElement : ComponentBase, IDisposable
    {
        // =========================
        // 1. VISIBILITY MODEL
        // =========================

        public enum VisibilityMode
        {
            Visible,
            Hidden,
            Collapsed
        }
        private VisibilityMode _lastVisibility;
        [Parameter] public VisibilityMode Visibility { get; set; } = VisibilityMode.Visible;

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
        [Parameter]public HAlign HorizontalAlignment { get; set; } = HAlign.Stretch;
        
        [Parameter]
        public VAlign VerticalAlignment { get; set; } = VAlign.Top;
        [Parameter] public int? ZIndex { get; set; }

        // =========================
        // 3. INTERACTION STATE
        // =========================

        private bool _lastIsEnabled;
        [Parameter]
        public bool IsEnabled { get; set; } = true;

        [Parameter] public CursorType Cursor { get; set; } = CursorType.Default;
        [Parameter] public int TabIndex { get; set; }

        // =========================
        // 4. STYLING
        // =========================

        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string? ElementId { get; set; }
        [Parameter] public ThemeColor Color { get; set; } = ThemeColor.Default;

        [Parameter]public string? ColorHex { get; set; }
        [Parameter] public Dictionary<string, object>? DataAttributes { get; set; }

        // =========================
        // 5. EVENTS
        // =========================

        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

        // =========================
        // 6. PERFORMANCE: RENDER BATCHING
        // =========================

        private bool _isRenderingSuspended = false;
        private int _pendingRenderRequests = 0;
        private CancellationTokenSource? _renderThrottleCts;

        protected IDisposable SuspendRendering() => new RenderSuspensionToken(this);

        private class RenderSuspensionToken : IDisposable
        {
            private readonly FrameworkElement _owner;
            public RenderSuspensionToken(FrameworkElement owner) => _owner = owner;
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
                    .ContinueWith(_ => InvokeAsync(StateHasChanged),
                        CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            }
            else
            {
                StateHasChanged();
            }
        }

        protected override bool ShouldRender() => !_isRenderingSuspended && base.ShouldRender();

        // =========================
        // 7. PERFORMANCE: CLASS/STYLE CACHING
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

        protected string GetCachedClass()
        {
            if (_classVersion != _lastClassVersion || _cachedClass == null)
            {
                _cachedClass = BuildBaseClass();
                _lastClassVersion = _classVersion;
            }
            return _cachedClass;
        }

        protected string GetCachedStyle()
        {
            if (_styleVersion != _lastStyleVersion || _cachedStyle == null)
            {
                _cachedStyle = BuildBaseStyle();
                _lastStyleVersion = _styleVersion;
            }
            return _cachedStyle;
        }

        protected MudBlazor.Color ResolveMudColor()
        {
            return ToMudColor(Color);
        }

        protected string? ResolveCssColor()
        {
            if (!string.IsNullOrWhiteSpace(ColorHex))
                return $"color:{ColorHex};";

            return null;
        }

        // =========================
        // 8. LAYOUT SYSTEM (Measure/Arrange)
        // =========================

        private Size _desiredSize = Size.Zero;
        protected bool IsMeasureValid { get; private set; }
        protected bool IsArrangeValid { get; private set; }
        public Size DesiredSize => _desiredSize;

        protected virtual Size Measure(Size availableSize)
        {
            var width = Width ?? availableSize.Width;
            var height = Height ?? availableSize.Height;

            if (MinWidth.HasValue)
                width = Math.Max(width, MinWidth.Value);

            if (MinHeight.HasValue)
                height = Math.Max(height, MinHeight.Value);

            if (MaxWidth.HasValue)
                width = Math.Min(width, MaxWidth.Value);

            if (MaxHeight.HasValue)
                height = Math.Min(height, MaxHeight.Value);

            if (Visibility == VisibilityMode.Collapsed)
            {
                _desiredSize = Size.Zero;
                IsMeasureValid = true;
                return Size.Zero;
            }

            _desiredSize = new Size(width, height);

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

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            bool layoutChanged = false;
            bool visualChanged = false;

            if (_lastVisibility != Visibility)
            {
                _lastVisibility = Visibility;
                visualChanged = true;
            }

            if (!_lastMargin.Equals(Margin))
            {
                _lastMargin = Margin;
                layoutChanged = true;
            }

            if (!_lastPadding.Equals(Padding))
            {
                _lastPadding = Padding;
                layoutChanged = true;
            }

            if (_lastWidth != Width)
            {
                _lastWidth = Width;
                layoutChanged = true;
            }

            if (_lastHeight != Height)
            {
                _lastHeight = Height;
                layoutChanged = true;
            }

            if (_lastIsEnabled != IsEnabled)
            {
                _lastIsEnabled = IsEnabled;
                visualChanged = true;
            }

            if (layoutChanged)
                InvalidateLayout();

            if (visualChanged)
                InvalidateVisual();
        }

        // =========================
        // 9. STYLE/CLASS BUILDERS
        // =========================

        protected virtual string BuildBaseStyle()
        {
            var sb = new StringBuilder();

            if (Margin != Thickness.Zero) sb.Append($"margin:{Margin.ToCss()};");
            if (Padding != Thickness.Zero) sb.Append($"padding:{Padding.ToCss()};");

            if (Width.HasValue) sb.Append(CultureInfo.InvariantCulture, $"width:{Width.Value}px;");
            if (Height.HasValue) sb.Append(CultureInfo.InvariantCulture, $"height:{Height.Value}px;");

            if (MinWidth.HasValue) sb.Append(CultureInfo.InvariantCulture, $"min-width:{MinWidth.Value}px;");
            if (MaxWidth.HasValue) sb.Append(CultureInfo.InvariantCulture, $"max-width:{MaxWidth.Value}px;");
            if (MinHeight.HasValue) sb.Append(CultureInfo.InvariantCulture, $"min-height:{MinHeight.Value}px;");
            if (MaxHeight.HasValue) sb.Append(CultureInfo.InvariantCulture, $"max-height:{MaxHeight.Value}px;");

            switch (Visibility)
            {
                case VisibilityMode.Hidden: sb.Append("visibility:hidden;"); break;
                case VisibilityMode.Collapsed: sb.Append("display:none;"); break;
            }

            sb.Append($"cursor:{(IsEnabled ? CursorToCss(Cursor) : "not-allowed")};");
            if (ZIndex.HasValue) sb.Append($"z-index:{ZIndex.Value};");
            if (!IsEnabled) sb.Append("opacity:0.6;");
            if (!string.IsNullOrWhiteSpace(Style)) sb.Append(Style);

            return sb.ToString();
        }

        protected virtual string BuildBaseClass()
        {
            var classes = new List<string> { "ui-framework-element" };

            if (!string.IsNullOrWhiteSpace(HAlignClass)) classes.Add(HAlignClass);
            if (!string.IsNullOrWhiteSpace(VAlignClass)) classes.Add(VAlignClass);

            classes.Add(IsEnabled ? "enabled" : "disabled");

            if (Color != ThemeColor.Default) classes.Add($"theme-{Color.ToString().ToLower()}");
            if (!string.IsNullOrWhiteSpace(Class)) classes.Add(Class);

            return string.Join(" ", classes);
        }

        protected string HAlignClass => HorizontalAlignment switch
        {
            HAlign.Left => "justify-start",
            HAlign.Center => "justify-center",
            HAlign.Right => "justify-end",
            HAlign.Stretch => "justify-stretch",
            _ => ""
        };

        protected string VAlignClass => VerticalAlignment switch
        {
            VAlign.Top => "items-start",
            VAlign.Center => "items-center",
            VAlign.Bottom => "items-end",
            VAlign.Stretch => "items-stretch",
            _ => ""
        };

        protected static string CursorToCss(CursorType cursor) => cursor switch
        {
            CursorType.Pointer => "pointer",
            CursorType.Text => "text",
            CursorType.Move => "move",
            CursorType.NotAllowed => "not-allowed",
            CursorType.Wait => "wait",
            CursorType.Resize => "row-resize",
            _ => "default"
        };

        protected static Color ToMudColor(ThemeColor color) => color switch
        {
            ThemeColor.Primary => MudBlazor.Color.Primary,
            ThemeColor.Secondary => MudBlazor.Color.Secondary,
            ThemeColor.Success => MudBlazor.Color.Success,
            ThemeColor.Warning => MudBlazor.Color.Warning,
            ThemeColor.Error => MudBlazor.Color.Error,
            ThemeColor.Info => MudBlazor.Color.Info,
            ThemeColor.Dark => MudBlazor.Color.Dark,
            ThemeColor.Inherit => MudBlazor.Color.Inherit,
            ThemeColor.Transparent => MudBlazor.Color.Transparent,
            _ => MudBlazor.Color.Default
        };

        protected Dictionary<string, object> GetDataAttributes()
        {
            var attrs = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(ElementId)) attrs["id"] = ElementId;
            if (DataAttributes != null)
                foreach (var kvp in DataAttributes) attrs[$"data-{kvp.Key}"] = kvp.Value;
            return attrs;
        }

       

        // =========================
        // 11. IDISPOSABLE
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