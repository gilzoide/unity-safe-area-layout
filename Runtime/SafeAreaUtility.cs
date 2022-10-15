using UnityEditor;
using UnityEngine;

namespace Gilzoide.SafeAreaLayout
{
    public static class SafeAreaUtility
    {
#if UNITY_EDITOR
        public const float NotchScreenProportion = 44f / 812f;
        public const float HomeButtonNotchScreenProportion = 21f / 375f;

        public static readonly string EditorPreviewModePrefsKey = $"{typeof(SafeAreaLayoutGroup).FullName}.{nameof(EditorPreviewMode)}";

        public enum EditorPreviewMode
        {
            /// <summary>Uses <see ref="Screen.safeArea"/>. Preview can only be seen on scenes</summary>
            ScreenSafeArea,
            /// <summary>Simulates an iPhone X cutout (top + bottom in portrait, left + right + bottom in landscape)</summary>
            IPhoneX,
            /// <summary>Simulates a single cutout (top in portrait, left in landscape)</summary>
            SingleCutoutTop,
            /// <summary>Simulates a single cutout (bottom in portrait, right in landscape)</summary>
            SingleCutoutBottom,
            /// <summary>Simulates a double cutout (top + bottom in portrait, left + right in landscape)</summary>
            DoubleCutout,
        }

        public static readonly string[] EditorPreviewModeNames = {
            "Screen.safeArea",
            "iPhone X",
            "Single Cutout (top)",
            "Single Cutout (bottom)",
            "Double Cutout",
        };

        public static int CurrentEditorPreviewMode
        {
            get => EditorPrefs.GetInt(EditorPreviewModePrefsKey, (int) EditorPreviewMode.ScreenSafeArea);
            set => EditorPrefs.SetInt(EditorPreviewModePrefsKey, value);
        }

        public static Rect GetSafeArea()
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            switch ((EditorPreviewMode) CurrentEditorPreviewMode)
            {
                case EditorPreviewMode.ScreenSafeArea:
                default:
                    return Screen.safeArea;
                
                case EditorPreviewMode.IPhoneX:
                    return ApplyCutoutIPhoneX(screenRect);

                case EditorPreviewMode.SingleCutoutTop:
                    return ApplyCutoutTop(screenRect);

                case EditorPreviewMode.SingleCutoutBottom:
                    return ApplyCutoutBottom(screenRect);

                case EditorPreviewMode.DoubleCutout:
                    return ApplyCutoutDouble(screenRect);
            }
        }

        public static Rect ApplyCutoutIPhoneX(Rect rect)
        {
            bool isPortrait = rect.height >= rect.width;
            if (isPortrait)
            {
                float inset = rect.height * NotchScreenProportion;
                rect.yMin += inset;
                rect.yMax -= inset;
            }
            else
            {
                float horizontalInset = rect.width * NotchScreenProportion;
                rect.xMin += horizontalInset;
                rect.xMax -= horizontalInset;
                float homeButtonInset = rect.height * HomeButtonNotchScreenProportion;
                rect.yMin += homeButtonInset;
            }
            return rect;
        }

        public static Rect ApplyCutoutTop(Rect rect)
        {
            bool isPortrait = rect.height >= rect.width;
            if (isPortrait)
            {
                float inset = rect.height * NotchScreenProportion;
                rect.yMax -= inset;
            }
            else
            {
                float horizontalInset = rect.width * NotchScreenProportion;
                rect.xMin += horizontalInset;
            }
            return rect;
        }

        public static Rect ApplyCutoutBottom(Rect rect)
        {
            bool isPortrait = rect.height >= rect.width;
            if (isPortrait)
            {
                float inset = rect.height * NotchScreenProportion;
                rect.yMin += inset;
            }
            else
            {
                float horizontalInset = rect.width * NotchScreenProportion;
                rect.xMax -= horizontalInset;
            }
            return rect;
        }

        public static Rect ApplyCutoutDouble(Rect rect)
        {
            bool isPortrait = rect.height >= rect.width;
            if (isPortrait)
            {
                float inset = rect.height * NotchScreenProportion;
                rect.yMin += inset;
                rect.yMax -= inset;
            }
            else
            {
                float horizontalInset = rect.width * NotchScreenProportion;
                rect.xMin += horizontalInset;
                rect.xMax -= horizontalInset;
            }
            return rect;
        }
#else
        public static Rect GetSafeArea()
        {
            return Screen.safeArea;
        }
#endif
    }
}
