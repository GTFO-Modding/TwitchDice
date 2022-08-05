using UnityEngine;

namespace TwitchDice.Extensions
{
    public static class RectTransformExtensions
    {
        /// <summary>Sets anchor min and max.</summary>
        public static RectTransform SetAnchor(this RectTransform rect, float x, float y)
        {
            return rect.SetAnchorMin(x, y)
                .SetAnchorMax(x, y);
        }
        /// <summary>Sets anchor min and max.</summary>
        public static RectTransform SetAnchor(this RectTransform rect, Vector2 anchorMinAndMax)
        {
            return rect.SetAnchorMin(anchorMinAndMax)
                .SetAnchorMax(anchorMinAndMax);
        }

        public static RectTransform SetAnchorMin(this RectTransform rect, float x, float y) => rect.SetAnchorMin(new(x, y));
        public static RectTransform SetAnchorMin(this RectTransform rect, Vector2 anchorMin)
        {
            rect.anchorMin = anchorMin;
            return rect;
        }

        public static RectTransform SetAnchorMax(this RectTransform rect, float x, float y) => rect.SetAnchorMax(new(x, y));
        public static RectTransform SetAnchorMax(this RectTransform rect, Vector2 anchorMax)
        {
            rect.anchorMax = anchorMax;
            return rect;
        }

        public static RectTransform SetAnchoredPosition(this RectTransform rect, float x, float y) => rect.SetAnchoredPosition(new(x, y));
        public static RectTransform SetAnchoredPosition(this RectTransform rect, Vector2 anchoredPosition)
        {
            rect.anchoredPosition = anchoredPosition;
            return rect;
        }

        // remember, set pivot before setting size delta
        public static RectTransform SetSizeDelta(this RectTransform rect, float x, float y) => rect.SetSizeDelta(new(x, y));
        public static RectTransform SetSizeDelta(this RectTransform rect, Vector2 sizeDelta)
        {
            rect.sizeDelta = sizeDelta;
            return rect;
        }

        public static RectTransform SetPivot(this RectTransform rect, float x, float y) => rect.SetPivot(new(x, y));
        public static RectTransform SetPivot(this RectTransform rect, Vector2 pivot)
        {
            rect.pivot = pivot;
            return rect;
        }
    }
}
