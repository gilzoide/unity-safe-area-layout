using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gilzoide.SafeAreaLayout
{
    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public class SafeAreaLayoutGroup : MonoBehaviour, ILayoutGroup
    {
        public bool IsDrivingLayout => (Application.isPlaying || PreviewInEditor)
            && _canvas != null && _canvas.renderMode != RenderMode.WorldSpace;

#if UNITY_EDITOR
        public static bool PreviewInEditor = false;
#else
        public const bool PreviewInEditor = false;
#endif

        [Tooltip("Whether safe area's top edge will be respected")]
        public bool TopEdge = true;
        [Tooltip("Whether safe area's left edge will be respected")]
        public bool LeftEdge = true;
        [Tooltip("Whether safe area's right edge will be respected")]
        public bool RightEdge = true;
        [Tooltip("Whether safe area's bottom edge will be respected")]
        public bool BottomEdge = true;

        public RectTransform SelfRectTransform => (RectTransform) transform;

        private readonly Dictionary<RectTransform, Anchors> _childrenAnchors = new Dictionary<RectTransform, Anchors>();
        private DrivenRectTransformTracker _drivenRectTransformTracker = new DrivenRectTransformTracker();
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private Canvas _canvas;
        private Rect _screenRect;

        private void OnEnable()
        {
            _canvas = GetComponentInParent<Canvas>();
            RefreshChildrenAnchors();
        }

        private void OnDisable()
        {
            _canvas = null;
            ClearChildrenAnchors();
        }

        private void OnTransformChildrenChanged()
        {
            RefreshChildrenAnchors();
        }

        public void SetLayoutHorizontal()
        {
            if (!IsDrivingLayout)
            {
                return;
            }

            RefreshScreenRect();
            float horizontalSize = _screenRect.size.x;
            if (horizontalSize <= 0)
            {
                return;
            }

            Rect safeArea = SafeAreaUtility.GetSafeArea();
            float leftMargin = LeftEdge ? Mathf.Max(0, safeArea.xMin - _screenRect.xMin) / horizontalSize : 0;
            float rightMargin = RightEdge ? Mathf.Max(0, _screenRect.xMax - safeArea.xMax) / horizontalSize : 0;

            foreach ((RectTransform child, Anchors anchors) in _childrenAnchors)
            {
                anchors.WithHorizontalMargins(leftMargin, rightMargin).ApplyTo(child);
            }
        }

        public void SetLayoutVertical()
        {
            if (!IsDrivingLayout)
            {
                return;
            }

            float verticalSize = _screenRect.size.y;
            if (verticalSize <= 0)
            {
                return;
            }

            Rect safeArea = SafeAreaUtility.GetSafeArea();
            float bottomMargin = BottomEdge ? Mathf.Max(0, safeArea.yMin - _screenRect.yMin) / verticalSize : 0;
            float topMargin = TopEdge ? Mathf.Max(0, _screenRect.yMax - safeArea.yMax) / verticalSize : 0;

            foreach ((RectTransform child, Anchors anchors) in _childrenAnchors)
            {
                new Anchors(child).WithVerticalMargins(bottomMargin, topMargin).ApplyTo(child);
            }
        }

        private void ClearChildrenAnchors()
        {
            _drivenRectTransformTracker.Clear();
            foreach ((RectTransform child, Anchors anchors) in _childrenAnchors)
            {
                anchors.ApplyTo(child);
            }
            _childrenAnchors.Clear();
        }

        private void RefreshChildrenAnchors()
        {
            if (!IsDrivingLayout)
            {
                ClearChildrenAnchors();
                return;
            }

            _drivenRectTransformTracker.Clear();
            var childrenToUntrack = new HashSet<RectTransform>(_childrenAnchors.Keys);
            foreach (Transform child in transform)
            {
                if (!(child is RectTransform rectTransform)
                    || child.TryGetComponent(out ILayoutIgnorer ignorer) && ignorer.ignoreLayout)
                {
                    continue;
                }

                _drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.Anchors);
                if (!_childrenAnchors.ContainsKey(rectTransform))
                {
                    _childrenAnchors[rectTransform] = new Anchors(rectTransform);
                    LayoutRebuilder.MarkLayoutForRebuild(SelfRectTransform);
                }
                childrenToUntrack.Remove(rectTransform);
            }

            foreach (RectTransform previousChild in childrenToUntrack)
            {
                _childrenAnchors.Remove(previousChild);
            }
        }

        private void RefreshScreenRect()
        {
            SelfRectTransform.GetWorldCorners(_worldCorners);

            Vector3 bottomLeft = _worldCorners[0];
            Vector3 topRight = _worldCorners[2];
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.worldCamera != null)
            {
                Camera camera = _canvas.worldCamera;
                bottomLeft = camera.WorldToScreenPoint(bottomLeft);
                topRight = camera.WorldToScreenPoint(topRight);
            }
            _screenRect = Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            LayoutRebuilder.MarkLayoutForRebuild(SelfRectTransform);
        }
#endif
    }
}
