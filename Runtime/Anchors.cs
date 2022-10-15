using UnityEngine;

namespace Gilzoide.SafeAreaLayout
{
    public readonly struct Anchors
    {
        public readonly Vector2 AnchorMin;
        public readonly Vector2 AnchorMax;

        public Anchors(Vector2 anchorMin, Vector2 anchorMax)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
        }

        public Anchors(RectTransform rectTransform)
        {
            AnchorMin = rectTransform.anchorMin;
            AnchorMax = rectTransform.anchorMax;
        }

        public Anchors WithHorizontalMargins(float leftMargin, float rightMargin)
        {
            return new Anchors(
                anchorMin: new Vector2(Remap01(AnchorMin.x, leftMargin, 1f - rightMargin), AnchorMin.y),
                anchorMax: new Vector2(Remap01(AnchorMax.x, leftMargin, 1f - rightMargin), AnchorMax.y)
            );
        }

        public Anchors WithVerticalMargins(float bottomMargin, float topMargin)
        {
            return new Anchors(
                anchorMin: new Vector2(AnchorMin.x, Remap01(AnchorMin.y, bottomMargin, 1f - topMargin)),
                anchorMax: new Vector2(AnchorMax.x, Remap01(AnchorMax.y, bottomMargin, 1f - topMargin))
            );
        }

        public void ApplyTo(RectTransform rectTransform)
        {
            rectTransform.anchorMin = AnchorMin;
            rectTransform.anchorMax = AnchorMax;
        }

        private static float Remap01(float value, float from, float to)
        {
            return from + (to - from) * value;
        }
    }
}
