using UnityEngine;

namespace EngineeringPlayground.UI
{
    /// <summary>Single source of truth for Engineering Playground visual tokens.</summary>
    public static class EngineeringPlaygroundTheme
    {
        public static readonly Color32 Canvas = new(5, 10, 18, 255);
        public static readonly Color32 Surface = new(13, 24, 38, 244);
        public static readonly Color32 SurfaceRaised = new(20, 36, 53, 248);
        public static readonly Color32 SurfacePressed = new(29, 49, 68, 255);
        public static readonly Color32 Accent = new(55, 220, 184, 255);
        public static readonly Color32 AccentPressed = new(42, 188, 157, 255);
        public static readonly Color32 Text = new(246, 250, 252, 255);
        public static readonly Color32 TextMuted = new(156, 178, 194, 255);
        public static readonly Color32 Stroke = new(47, 69, 88, 190);
        public static readonly Color32 Success = new(92, 226, 155, 255);
        public static readonly Color32 Warning = new(255, 194, 92, 255);

        public const float RadiusSmall = 14f;
        public const float RadiusMedium = 22f;
        public const float RadiusLarge = 32f;
        public const float Space1 = 8f;
        public const float Space2 = 12f;
        public const float Space3 = 16f;
        public const float Space4 = 24f;
        public const float TouchMin = 56f;
        public const float MotionFast = .12f;
        public const float MotionNormal = .22f;
    }
}
