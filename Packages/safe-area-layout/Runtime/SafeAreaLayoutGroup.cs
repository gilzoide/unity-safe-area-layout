using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Gilzoide.SafeAreaLayout
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaLayoutGroup : MonoBehaviour, ILayoutGroup
    {
#if UNITY_EDITOR
        public static bool IsDrivingLayout => Application.isPlaying || _previewInEditor;
        private static bool _previewInEditor = false;
#else
        public const bool IsDrivingLayout = true;
#endif

        [Tooltip("Whether safe area's top edge will be respected")]
        public bool TopEdge = true;
        [Tooltip("Whether safe area's left edge will be respected")]
        public bool LeftEdge = true;
        [Tooltip("Whether safe area's right edge will be respected")]
        public bool RightEdge = true;
        [Tooltip("Whether safe area's bottom edge will be respected")]
        public bool BottomEdge = true;

        private RectTransform SelfRectTransform => (RectTransform) transform;

        private readonly Dictionary<RectTransform, Anchors> _childrenAnchors = new Dictionary<RectTransform, Anchors>();
        private DrivenRectTransformTracker _drivenRectTransformTracker = new DrivenRectTransformTracker();
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private Rect _worldRect;

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
            float leftMargin = LeftEdge ? Mathf.Max(0, safeArea.xMin - _worldRect.xMin) / horizontalSize : 0;
            float rightMargin = RightEdge ? Mathf.Max(0, _worldRect.xMax - safeArea.xMax) / horizontalSize : 0;

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

            float verticalSize = _worldRect.size.y;
            if (verticalSize <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            float bottomMargin = BottomEdge ? Mathf.Max(0, safeArea.yMin - _worldRect.yMin) / verticalSize : 0;
            float topMargin = TopEdge ? Mathf.Max(0, _worldRect.yMax - safeArea.yMax) / verticalSize : 0;

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

        private void RefreshWorldRect()
        {
            SelfRectTransform.GetWorldCorners(_worldCorners);

            Vector3 bottomLeft = _worldCorners[0];
            Vector3 topRight = _worldCorners[2];
            _worldRect = Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            LayoutRebuilder.MarkLayoutForRebuild(SelfRectTransform);
        }

        [CustomEditor(typeof(SafeAreaLayoutGroup)), CanEditMultipleObjects]
        public class SafeAreaLayoutGroupEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();

                bool preview = HoverButton("Hover to Preview Layout");
                if (preview != _previewInEditor)
                {
                    SetPreviewEnabled(preview);
                }
            }

            private void SetPreviewEnabled(bool enabled)
            {
                _previewInEditor = enabled;
                foreach (SafeAreaLayoutGroup safeArea in FindObjectsOfType<SafeAreaLayoutGroup>())
                {
                    safeArea.RefreshChildrenAnchors();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(safeArea.SelfRectTransform);
                }
                SceneView.RepaintAll();
            }

            private static bool HoverButton(string content)
            {
                GUILayout.Box(content, EditorStyles.miniButton);
                return GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
            }
        }
#endif
    }
}
