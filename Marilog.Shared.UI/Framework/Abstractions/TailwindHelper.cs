using Marilog.Shared.UI.Framework.Enums;

namespace Marilog.Shared.UI.Framework.Abstractions
{
    /// <summary>
    /// Central mapping from UI.Framework enums → Tailwind CSS utility classes.
    /// This is the ONLY place that knows about Tailwind class names.
    /// No MudBlazor dependency anywhere in this file.
    /// </summary>
    public static class TailwindHelper
    {
        // =========================
        // FONT SIZE
        // =========================

        public static string FontSizeClass(FontSize size) => size switch
        {
            FontSize.Tiny => "text-xs",      // 10-12px
            FontSize.Small => "text-sm",      // 14px
            FontSize.Regular => "text-base",    // 16px — Tailwind base
            FontSize.Medium => "text-lg",      // 18px
            FontSize.Large => "text-xl",      // 20px
            FontSize.XLarge => "text-2xl",     // 24px
            FontSize.XXLarge => "text-3xl",     // 30px
            FontSize.Display => "text-5xl",     // 48px
            _ => "text-base"
        };

        // =========================
        // FONT WEIGHT
        // =========================

        public static string FontWeightClass(FontWeight weight) => weight switch
        {
            FontWeight.Thin => "font-thin",
            FontWeight.Light => "font-light",
            FontWeight.Normal => "font-normal",
            FontWeight.Medium => "font-medium",
            FontWeight.SemiBold => "font-semibold",
            FontWeight.Bold => "font-bold",
            _ => "font-normal"
        };

        // =========================
        // TEXT ALIGNMENT
        // =========================

        public static string TextAlignClass(TextAlignment align) => align switch
        {
            TextAlignment.Left => "text-left",
            TextAlignment.Center => "text-center",
            TextAlignment.Right => "text-right",
            TextAlignment.Justify => "text-justify",
            _ => "text-left"
        };

        // =========================
        // TEXT WRAP
        // =========================

        public static string TextWrapClass(TextWrap wrap) => wrap switch
        {
            TextWrap.NoWrap => "whitespace-nowrap",
            TextWrap.WrapWithOverflow => "whitespace-pre-wrap break-all",
            _ => "whitespace-normal"
        };

        // =========================
        // THEME COLOR → TEXT
        // =========================

        public static string TextColorClass(ThemeColor color) => color switch
        {
            ThemeColor.Primary => "text-blue-600",
            ThemeColor.Secondary => "text-slate-500",
            ThemeColor.Success => "text-emerald-600",
            ThemeColor.Warning => "text-amber-500",
            ThemeColor.Error => "text-red-600",
            ThemeColor.Info => "text-sky-500",
            ThemeColor.Dark => "text-slate-900",
            ThemeColor.Light => "text-slate-100",
            ThemeColor.Inherit => "text-inherit",
            ThemeColor.Transparent => "text-transparent",
            _ => "text-slate-800"  // Default
        };

        // =========================
        // THEME COLOR → BACKGROUND
        // =========================

        public static string BgColorClass(ThemeColor color) => color switch
        {
            ThemeColor.Primary => "bg-blue-600",
            ThemeColor.Secondary => "bg-slate-500",
            ThemeColor.Success => "bg-emerald-600",
            ThemeColor.Warning => "bg-amber-500",
            ThemeColor.Error => "bg-red-600",
            ThemeColor.Info => "bg-sky-500",
            ThemeColor.Dark => "bg-slate-900",
            ThemeColor.Light => "bg-slate-100",
            ThemeColor.Transparent => "bg-transparent",
            _ => "bg-white"
        };

        // =========================
        // THEME COLOR → BORDER
        // =========================

        public static string BorderColorClass(ThemeColor color) => color switch
        {
            ThemeColor.Primary => "border-blue-600",
            ThemeColor.Secondary => "border-slate-400",
            ThemeColor.Success => "border-emerald-600",
            ThemeColor.Warning => "border-amber-500",
            ThemeColor.Error => "border-red-600",
            ThemeColor.Info => "border-sky-500",
            ThemeColor.Dark => "border-slate-900",
            _ => "border-slate-300"
        };

        // =========================
        // BUTTON VARIANT
        // =========================

        public static string ButtonClass(ButtonVariant variant, ThemeColor color) => variant switch
        {
            ButtonVariant.Filled => string.Join(" ", new[]
            {
                BgColorClass(color),
                "text-white",
                "hover:opacity-90",
                "border border-transparent"
            }),

            ButtonVariant.Outlined => string.Join(" ", new[]
            {
                "bg-transparent",
                TextColorClass(color),
                BorderColorClass(color),
                "border",
                "hover:bg-opacity-10",
                $"hover:{BgColorClass(color).Replace("bg-", "bg-")}"
            }),

            ButtonVariant.Text => string.Join(" ", new[]
            {
                "bg-transparent",
                "border-transparent",
                TextColorClass(color),
                "hover:underline"
            }),

            _ => BgColorClass(color)
        };

        public static string ButtonSizeClass(ButtonSize size) => size switch
        {
            ButtonSize.Small => "px-3 py-1 text-sm",
            ButtonSize.Large => "px-6 py-3 text-base",
            _ => "px-4 py-2 text-sm"  // Medium
        };

        // =========================
        // ELEVATION → BOX SHADOW
        // =========================

        public static string ElevationClass(ElevationLevel level) => level switch
        {
            ElevationLevel.None => "shadow-none",
            ElevationLevel.Low => "shadow-sm",
            ElevationLevel.Medium => "shadow-md",
            ElevationLevel.High => "shadow-lg",
            ElevationLevel.Floating => "shadow-xl",
            _ => "shadow-sm"
        };

        // =========================
        // HALIGN → FLEX/SELF CLASS
        // =========================

        public static string HAlignSelfClass(HAlign align) => align switch
        {
            HAlign.Left => "self-start",
            HAlign.Center => "self-center",
            HAlign.Right => "self-end",
            HAlign.Stretch => "self-stretch",
            _ => "self-auto"
        };

        public static string HAlignItemsClass(HAlign align) => align switch
        {
            HAlign.Left => "items-start",
            HAlign.Center => "items-center",
            HAlign.Right => "items-end",
            HAlign.Stretch => "items-stretch",
            _ => "items-start"
        };

        public static string JustifyClass(HAlign align) => align switch
        {
            HAlign.Left => "justify-start",
            HAlign.Center => "justify-center",
            HAlign.Right => "justify-end",
            HAlign.Stretch => "justify-between",
            _ => "justify-start"
        };

        // =========================
        // VALIGN → FLEX CLASS
        // =========================

        public static string VAlignItemsClass(VAlign align) => align switch
        {
            VAlign.Top => "items-start",
            VAlign.Center => "items-center",
            VAlign.Bottom => "items-end",
            VAlign.Stretch => "items-stretch",
            _ => "items-start"
        };

        // =========================
        // SPACING → GAP CLASS
        // =========================

        public static string GapClass(SpacingSize spacing) => spacing switch
        {
            SpacingSize.None => "gap-0",
            SpacingSize.Tiny => "gap-1",   // 4px
            SpacingSize.Small => "gap-2",   // 8px
            SpacingSize.Medium => "gap-4",   // 16px
            SpacingSize.Large => "gap-6",   // 24px
            SpacingSize.XLarge => "gap-8",   // 32px
            _ => "gap-2"
        };

        /// <summary>Raw pixel gap — converts to closest Tailwind gap class.</summary>
        public static string GapClass(int pixels) => pixels switch
        {
            0 => "gap-0",
            <= 4 => "gap-1",
            <= 8 => "gap-2",
            <= 12 => "gap-3",
            <= 16 => "gap-4",
            <= 20 => "gap-5",
            <= 24 => "gap-6",
            <= 32 => "gap-8",
            <= 40 => "gap-10",
            _ => "gap-12"
        };

        // =========================
        // CURSOR
        // =========================

        public static string CursorClass(CursorType cursor) => cursor switch
        {
            CursorType.Pointer => "cursor-pointer",
            CursorType.Text => "cursor-text",
            CursorType.Move => "cursor-move",
            CursorType.NotAllowed => "cursor-not-allowed",
            CursorType.Wait => "cursor-wait",
            CursorType.Resize => "cursor-row-resize",
            _ => "cursor-default"
        };

        // =========================
        // INPUT VARIANT
        // =========================

        public static string InputBaseClass(InputVariant variant) => variant switch
        {
            InputVariant.Filled => "bg-slate-100 border-b-2 border-slate-400 focus:border-blue-600 rounded-t px-3 py-2 outline-none w-full",
            InputVariant.Underline => "bg-transparent border-b-2 border-slate-400 focus:border-blue-600 px-0 py-2 outline-none w-full",
            _ => "bg-white border border-slate-300 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 rounded px-3 py-2 outline-none w-full"
        };

        // =========================
        // COMPOSE UTILITY
        // =========================

        /// <summary>
        /// Joins non-null, non-empty class strings.
        /// Usage: TailwindHelper.Classes("flex", condition ? "gap-4" : null, customClass)
        /// </summary>
        public static string Classes(params string?[] parts)
            => string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
