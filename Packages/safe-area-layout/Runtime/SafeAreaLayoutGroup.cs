using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gilzoide.SafeAreaLayout
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaLayoutGroup : MonoBehaviour, ILayoutGroup
    {
        public static bool IsDrivingLayout => Application.isPlaying;

        private RectTransform SelfRectTransform => (RectTransform) transform;

        private readonly Dictionary<RectTransform, Anchors> _childrenAnchors = new Dictionary<RectTransform, Anchors>();
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private Rect _worldRect;
        private DrivenRectTransformTracker _drivenRectTransformTracker;

        private void OnEnable()
        {
            RefreshChildrenAnchors();
        }

        private void OnDisable()
        {
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

            RefreshWorldRect();
            float horizontalSize = _worldRect.size.x;
            if (horizontalSize <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            float leftMargin = Mathf.Max(0, safeArea.xMin - _worldRect.xMin) / horizontalSize;
            float rightMargin = Mathf.Max(0, _worldRect.xMax - safeArea.xMax) / horizontalSize;

            foreach ((RectTransform child, Anchors anchors) in _childrenAnchors)
            {
                anchors.WithHorizontalMargins(leftMargin, rightMargin).ApplyTo(child);
            }
        }

        public void SetLayoutVertical()
        {
            float verticalSize = _worldRect.size.y;
            if (verticalSize <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            float bottomMargin = Mathf.Max(0, safeArea.yMin - _worldRect.yMin) / verticalSize;
            float topMargin = Mathf.Max(0, _worldRect.yMax - safeArea.yMax) / verticalSize;

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
            _drivenRectTransformTracker.Clear();
            if (!IsDrivingLayout)
            {
                _childrenAnchors.Clear();
                return;
            }

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
                }
                childrenToUntrack.Remove(rectTransform);
            }

            foreach (RectTransform previousChild in childrenToUntrack)
            {
                _childrenAnchors.Remove(previousChild);
            }
        }

        private void RefreshWorldRect()
        {
            SelfRectTransform.GetWorldCorners(_worldCorners);

            Vector3 bottomLeft = _worldCorners[0];
            Vector3 topRight = _worldCorners[2];
            _worldRect = Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }
    }
}
