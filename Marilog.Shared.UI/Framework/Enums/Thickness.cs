using System.Globalization;
namespace Marilog.Shared.UI.Framework.Enums
{
    // <summary>
    /// Represents a uniform or per-side spacing value — mirrors WPF Thickness.
    /// Used for Margin and Padding parameters.
    /// </summary>
    public readonly struct Thickness : IEquatable<Thickness>
    {
        public double Left { get; }
        public double Top { get; }
        public double Right { get; }
        public double Bottom { get; }

        public Thickness(double uniform)
            : this(uniform, uniform, uniform, uniform) { }

        public Thickness(double horizontal, double vertical)
            : this(horizontal, vertical, horizontal, vertical) { }

        public Thickness(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        // خصائص مفيدة إضافية
        public static Thickness Zero => new(0);
        public static Thickness One => new(1);

        public double Horizontal => Left + Right;
        public double Vertical => Top + Bottom;

        public bool IsUniform => Math.Abs(Left - Top) < 0.0001 &&
                                 Math.Abs(Left - Right) < 0.0001 &&
                                 Math.Abs(Left - Bottom) < 0.0001;

        public bool IsZero => Math.Abs(Left) < 0.0001 &&
                              Math.Abs(Top) < 0.0001 &&
                              Math.Abs(Right) < 0.0001 &&
                              Math.Abs(Bottom) < 0.0001;

        /// <summary>
        /// Parses XAML-style "8", "8 4", or "8 4 8 4".
        /// Supports both spaces and commas: "8,4", "8,4,8,4"
        /// </summary>
        public static Thickness Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Zero;

            // Replace commas with spaces for unified parsing
            value = value.Trim().Replace(',', ' ');

            // Handle CSS-style "8px 4px 8px 4px" (remove 'px' suffix)
            value = value.Replace("px", "", StringComparison.OrdinalIgnoreCase);

            var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            double p0, p1, p2, p3;

            switch (parts.Length)
            {
                case 1:
                    p0 = ParseDouble(parts[0]);
                    return new Thickness(p0);

                case 2:
                    p0 = ParseDouble(parts[0]);
                    p1 = ParseDouble(parts[1]);
                    return new Thickness(p0, p1);

                case 3:
                    // CSS shorthand: Top Right Bottom (Left = Right)
                    p0 = ParseDouble(parts[0]); // Top
                    p1 = ParseDouble(parts[1]); // Right
                    p2 = ParseDouble(parts[2]); // Bottom
                    return new Thickness(p1, p0, p1, p2);

                case 4:
                    p0 = ParseDouble(parts[0]);
                    p1 = ParseDouble(parts[1]);
                    p2 = ParseDouble(parts[2]);
                    p3 = ParseDouble(parts[3]);
                    return new Thickness(p0, p1, p2, p3);

                default:
                    return Zero;
            }
        }

        /// <summary>
        /// Safe parsing with fallback
        /// </summary>
        public static bool TryParse(string value, out Thickness result)
        {
            try
            {
                result = Parse(value);
                return true;
            }
            catch
            {
                result = Zero;
                return false;
            }
        }

        private static double ParseDouble(string value)
        {
            return double.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts to CSS shorthand (Top Right Bottom Left).
        /// </summary>
        public string ToCss()
        {
            if (IsZero)
                return "0";

            if (IsUniform)
                return $"{Left}px";

            // CSS shorthand: Top Right Bottom Left
            return $"{Top}px {Right}px {Bottom}px {Left}px";
        }

        /// <summary>
        /// Converts to CSS with units (rem, em, %)
        /// </summary>
        public string ToCss(string unit = "px")
        {
            if (IsZero)
                return "0";

            if (IsUniform)
                return $"{Left}{unit}";

            return $"{Top}{unit} {Right}{unit} {Bottom}{unit} {Left}{unit}";
        }

        public override string ToString() => ToCss();

        public bool Equals(Thickness other)
        {
            const double eps = 0.0001;

            return Math.Abs(Left - other.Left) < eps &&
                   Math.Abs(Top - other.Top) < eps &&
                   Math.Abs(Right - other.Right) < eps &&
                   Math.Abs(Bottom - other.Bottom) < eps;
        }

        public override bool Equals(object? obj)
            => obj is Thickness t && Equals(t);

        public override int GetHashCode()
            => HashCode.Combine(Left, Top, Right, Bottom);

        public static bool operator ==(Thickness a, Thickness b) => a.Equals(b);
        public static bool operator !=(Thickness a, Thickness b) => !a.Equals(b);

        public static Thickness operator +(Thickness a, Thickness b) => new(
            a.Left + b.Left,
            a.Top + b.Top,
            a.Right + b.Right,
            a.Bottom + b.Bottom
        );

        public static Thickness operator -(Thickness a, Thickness b) => new(
            a.Left - b.Left,
            a.Top - b.Top,
            a.Right - b.Right,
            a.Bottom - b.Bottom
        );

        // Ergonomic usage (kept for XAML-like DSL feel)
        public static implicit operator Thickness(double d) => new(d);
        public static implicit operator Thickness(string s) => Parse(s);

        // دعم CSS string مباشرة
        public static implicit operator Thickness((double horizontal, double vertical) tuple)
            => new(tuple.horizontal, tuple.vertical);
    }
}
