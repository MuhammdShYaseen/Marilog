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
        //Image-----------------

        public static string ImageFit(ImageFitMode mode) => mode switch
        {
            ImageFitMode.Cover => "object-cover",
            ImageFitMode.Contain => "object-contain",
            ImageFitMode.Fill => "object-fill",
            ImageFitMode.ScaleDown => "object-scale-down",
            _ => "object-none"
        };

        public static string ImageShape(ImageShape shape) => shape switch
        {
            Enums.ImageShape.Circle => "rounded-full",
            Enums.ImageShape.Rounded => "rounded-lg",
            Enums.ImageShape.Square => "rounded-none",
            Enums.ImageShape.Rectangle => "rounded-md",
            _ => "rounded-none"
        };

        public static string ImageOverflow(ImageOverflowMode mode) => mode switch
        {
            ImageOverflowMode.Clip => "overflow-hidden",
            ImageOverflowMode.Visible => "overflow-visible",
            _ => "overflow-hidden"
        };

        //navbar----------------
        public static string NavMenuModeClass(NavMenuMode mode) => mode switch
        {
            NavMenuMode.Vertical => "flex flex-col",
            NavMenuMode.Horizontal => "flex flex-row",
            NavMenuMode.Compact => "flex flex-col text-sm gap-1",
            NavMenuMode.Mini => "flex flex-col w-12",
            _ => "flex flex-col"
        };
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
            ThemeColor.Primary => "text-primary-600 dark:text-primary-400",
            ThemeColor.Secondary => "text-secondary-600 dark:text-secondary-400",
            ThemeColor.Success => "text-success-600 dark:text-success-400",
            ThemeColor.Warning => "text-warning-600 dark:text-warning-400",
            ThemeColor.Error => "text-error-600 dark:text-error-400",
            ThemeColor.Info => "text-info-600 dark:text-info-400",
            ThemeColor.Dark => "text-gray-900 dark:text-gray-100",
            ThemeColor.Light => "text-gray-600 dark:text-gray-300",  // ✅ قابل للقراءة
            ThemeColor.Inherit => "text-inherit",
            ThemeColor.Transparent => "text-transparent",
            ThemeColor.Default => "text-gray-700 dark:text-gray-300"  // ✅ متسق
,
            _ => throw new NotImplementedException()
        };

        // =========================
        // THEME COLOR → BACKGROUND
        // =========================

        public static string BgColorClass(ThemeColor color) => color switch
        {
            ThemeColor.Primary => "bg-primary-600 dark:bg-primary-500",
            ThemeColor.Secondary => "bg-secondary-600 dark:bg-secondary-500",
            ThemeColor.Success => "bg-success-600 dark:bg-success-500",
            ThemeColor.Warning => "bg-warning-500 dark:bg-warning-400",
            ThemeColor.Error => "bg-error-600 dark:bg-error-500",
            ThemeColor.Info => "bg-info-600 dark:bg-info-500",
            ThemeColor.Dark => "bg-gray-900 dark:bg-gray-800",
            ThemeColor.Light => "bg-gray-100 dark:bg-gray-700",
            ThemeColor.Inherit => "bg-inherit",
            ThemeColor.Transparent => "bg-transparent",
            ThemeColor.Default => "bg-white dark:bg-gray-800",
            _ =>  "bg-white dark:bg-gray-800"
        };

        // =========================
        // THEME COLOR → BORDER
        // =========================
        public static string DisabledClass =>
        "opacity-50 cursor-not-allowed";  // ✅ بدون pointer-events-none

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

        public static string VAlignSelfClass(VAlign alignment) => alignment switch
        {
            VAlign.Top => "self-start",
            VAlign.Center => "self-center",
            VAlign.Bottom => "self-end",
            VAlign.Stretch => "self-stretch",
            _ => ""
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
