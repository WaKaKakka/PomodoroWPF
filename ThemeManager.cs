using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PomodoroWPF
{
    /// <summary>
    /// 多主题管理器
    /// </summary>
    public static class ThemeManager
    {
        public class ThemeColors
        {
            public string Name { get; set; } = "";
            public string Bg { get; set; } = "#000000";
            public string Card { get; set; } = "#0a0a0a";
            public string CardBorder { get; set; } = "#1a1a1a";
            public string Accent { get; set; } = "#f59e0b";
            public string AccentGlow { get; set; } = "#fbbf24";
            public string AccentDim { get; set; } = "#a16207";
            public string Success { get; set; } = "#10b981";
            public string SuccessDim { get; set; } = "#065f46";
            public string Text { get; set; } = "#fafaf9";
            public string TextDim { get; set; } = "#c8c3bc";
            public string TextMuted { get; set; } = "#57534e";
        }

        public static readonly Dictionary<string, ThemeColors> Themes = new()
        {
            ["dark_gold"] = new ThemeColors
            {
                Name = "黑金",
                Bg = "#000000", Card = "#0a0a0a", CardBorder = "#1a1a1a",
                Accent = "#f59e0b", AccentGlow = "#fbbf24", AccentDim = "#a16207",
                Success = "#10b981", SuccessDim = "#065f46",
                Text = "#fafaf9", TextDim = "#c8c3bc", TextMuted = "#57534e",
            },
            ["dark_blue"] = new ThemeColors
            {
                Name = "深蓝",
                Bg = "#020617", Card = "#0f172a", CardBorder = "#1e293b",
                Accent = "#3b82f6", AccentGlow = "#60a5fa", AccentDim = "#1d4ed8",
                Success = "#22d3ee", SuccessDim = "#0e7490",
                Text = "#f8fafc", TextDim = "#b0bec5", TextMuted = "#475569",
            },
            ["dark_green"] = new ThemeColors
            {
                Name = "森林",
                Bg = "#022c22", Card = "#064e3b", CardBorder = "#065f46",
                Accent = "#34d399", AccentGlow = "#6ee7b7", AccentDim = "#059669",
                Success = "#a3e635", SuccessDim = "#4d7c0f",
                Text = "#ecfdf5", TextDim = "#bbf7d0", TextMuted = "#047857",
            },
            ["light"] = new ThemeColors
            {
                Name = "极简白",
                Bg = "#fafafa", Card = "#ffffff", CardBorder = "#e5e7eb",
                Accent = "#f59e0b", AccentGlow = "#fbbf24", AccentDim = "#d97706",
                Success = "#10b981", SuccessDim = "#059669",
                Text = "#111827", TextDim = "#374151", TextMuted = "#f3f4f6",
            },
        };

        /// <summary>
        /// 应用主题到当前窗口
        /// </summary>
        public static void Apply(string themeId)
        {
            if (!Themes.TryGetValue(themeId, out var t))
                t = Themes["dark_gold"];

            var res = Application.Current.Resources;
            SetBrush(res, "BgBrush", t.Bg);
            SetBrush(res, "CardBrush", t.Card);
            SetBrush(res, "CardBorderBrush", t.CardBorder);
            SetBrush(res, "AccentBrush", t.Accent);
            SetBrush(res, "AccentGlowBrush", t.AccentGlow);
            SetBrush(res, "AccentDimBrush", t.AccentDim);
            SetBrush(res, "SuccessBrush", t.Success);
            SetBrush(res, "SuccessDimBrush", t.SuccessDim);
            SetBrush(res, "TextBrush", t.Text);
            SetBrush(res, "TextDimBrush", t.TextDim);
            SetBrush(res, "TextMutedBrush", t.TextMuted);
        }

        private static void SetBrush(ResourceDictionary res, string key, string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            res[key] = new SolidColorBrush(color);
        }

        /// <summary>
        /// 获取当前主题颜色（用于按钮颜色等不在资源字典中的地方）
        /// </summary>
        public static ThemeColors GetCurrent(string themeId)
        {
            return Themes.TryGetValue(themeId, out var t) ? t : Themes["dark_gold"];
        }
    }
}
