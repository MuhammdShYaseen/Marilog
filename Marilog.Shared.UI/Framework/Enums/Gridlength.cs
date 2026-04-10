using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Marilog.Shared.UI.Framework.Enums
{
    /// <summary>
    /// Represents the size of a Grid column or row — mirrors WPF GridLength.
    /// Supports pixel, star (*), and auto sizing.
    /// </summary>
    public readonly struct GridLength : IEquatable<GridLength>
    {
        public double Value { get; }
        public GridUnitType UnitType { get; }

        private GridLength(double value, GridUnitType unitType)
        {
            // التحقق من صحة القيم
            if (unitType == GridUnitType.Star && value < 0)
                throw new ArgumentException("Star value cannot be negative", nameof(value));

            if (unitType == GridUnitType.Pixel && value < 0)
                throw new ArgumentException("Pixel value cannot be negative", nameof(value));

            Value = value;
            UnitType = unitType;
        }

        /// <summary>Auto-sized: shrinks/grows to content.</summary>
        public static GridLength Auto => new(0, GridUnitType.Auto);

        /// <summary>One star unit of remaining space.</summary>
        public static GridLength Star => new(1, GridUnitType.Star);

        /// <summary>N star units of remaining space (e.g. 2* = twice as wide).</summary>
        public static GridLength Stars(double n)
        {
            if (n <= 0)
                throw new ArgumentException("Star value must be positive", nameof(n));
            return new GridLength(n, GridUnitType.Star);
        }

        /// <summary>Fixed pixel width/height.</summary>
        public static GridLength Pixels(double px)
        {
            if (px < 0)
                throw new ArgumentException("Pixel value cannot be negative", nameof(px));
            return new GridLength(px, GridUnitType.Pixel);
        }

        /// <summary>
        /// Parses a XAML-style string: "Auto", "2*", "*", "200", "1.5*", "200px".
        /// Supports CSS-style "fr" units as well.
        /// </summary>
        public static GridLength Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Auto;

            value = value.Trim();

            // Handle "Auto" (case-insensitive)
            if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                return Auto;

            // Handle CSS "fr" unit (e.g., "1fr", "2.5fr")
            if (value.EndsWith("fr", StringComparison.OrdinalIgnoreCase))
            {
                var num = value[..^2].Trim();
                var n = string.IsNullOrWhiteSpace(num) ? 1.0 : ParseDouble(num);
                return Stars(n);
            }

            // Handle XAML star "*" syntax
            if (value.EndsWith('*'))
            {
                var num = value[..^1].Trim();
                var n = string.IsNullOrWhiteSpace(num) ? 1.0 : ParseDouble(num);
                return Stars(n);
            }

            // Handle CSS "px" unit (optional)
            if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                var num = value[..^2].Trim();
                return Pixels(ParseDouble(num));
            }

            // Handle raw numbers (assume pixels)
            return Pixels(ParseDouble(value));
        }

        /// <summary>
        /// Safe parsing with fallback
        /// </summary>
        public static bool TryParse(string value, out GridLength result)
        {
            try
            {
                result = Parse(value);
                return true;
            }
            catch
            {
                result = Auto;
                return false;
            }
        }

        private static double ParseDouble(string value)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                throw new FormatException($"Invalid numeric value: '{value}'");
            return result;
        }

        // Properties for easier checking
        public bool IsAbsolute => UnitType == GridUnitType.Pixel;
        public bool IsAuto => UnitType == GridUnitType.Auto;
        public bool IsStar => UnitType == GridUnitType.Star;

        // For star sizing: gets the proportional value (default 1 if not set)
        public double StarValue => IsStar ? Value : 0;

        /// <summary>
        /// Converts to CSS Grid track value string.
        /// </summary>
        public string ToCss()
        {
            return UnitType switch
            {
                GridUnitType.Auto => "auto",
                GridUnitType.Star => Value == 1 ? "1fr" : $"{Value.ToString(CultureInfo.InvariantCulture)}fr",
                GridUnitType.Pixel => $"{Value.ToString(CultureInfo.InvariantCulture)}px",
                _ => "auto"
            };
        }

        /// <summary>
        /// Converts to a human-readable string (XAML style)
        /// </summary>
        public override string ToString()
        {
            return UnitType switch
            {
                GridUnitType.Auto => "Auto",
                GridUnitType.Star => Value == 1 ? "*" : $"{Value.ToString(CultureInfo.InvariantCulture)}*",
                GridUnitType.Pixel => Value.ToString(CultureInfo.InvariantCulture),
                _ => "Auto"
            };
        }

        public bool Equals(GridLength other)
        {
            const double eps = 0.0001;
            return Math.Abs(Value - other.Value) < eps && UnitType == other.UnitType;
        }

        public override bool Equals(object? obj) => obj is GridLength g && Equals(g);

        public override int GetHashCode() => HashCode.Combine(Value, UnitType);

        public static bool operator ==(GridLength a, GridLength b) => a.Equals(b);
        public static bool operator !=(GridLength a, GridLength b) => !a.Equals(b);

        // Implicit conversions
        public static implicit operator GridLength(string s) => Parse(s);
        public static implicit operator GridLength(double d) => Pixels(d);

        // Explicit conversion to double (only for absolute values)
        public static explicit operator double(GridLength length)
        {
            if (!length.IsAbsolute)
                throw new InvalidOperationException("Cannot convert non-absolute GridLength to double");
            return length.Value;
        }
    }

    /// <summary>
    /// Collection of GridLengths for grid definition
    /// </summary>
    public class GridLengthCollection : List<GridLength>
    {
        public GridLengthCollection() { }

        public GridLengthCollection(IEnumerable<GridLength> collection) : base(collection) { }

        /// <summary>
        /// Parse a XAML-style string: "Auto, 2*, *, 200"
        /// </summary>
        public static GridLengthCollection Parse(string value)
        {
            var collection = new GridLengthCollection();

            if (string.IsNullOrWhiteSpace(value))
                return collection;

            var parts = value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                collection.Add(GridLength.Parse(part.Trim()));
            }

            return collection;
        }

        /// <summary>
        /// Convert to CSS grid-template-columns/rows string
        /// </summary>
        public string ToCss()
        {
            return string.Join(" ", this.Select(g => g.ToCss()));
        }

        public override string ToString() => ToCss();
    }
}
