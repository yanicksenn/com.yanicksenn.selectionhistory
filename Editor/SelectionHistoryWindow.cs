using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.SelectionHistory.Editor
{
    public class SelectionHistoryWindow : EditorWindow
    {
        private const string HistorySizeKey = "YanickSenn.SelectionHistory.HistorySize";
        private static int _historySize = 10;

        private List<Object> _history = new List<Object>();
        private Vector2 _scrollPosition;
        private Object _selectedObjectInList;

        [MenuItem("Window/Selection History")]
        public static void ShowWindow()
        {
            GetWindow<SelectionHistoryWindow>("Selection History");
        }

        private void OnEnable()
        {
            _historySize = EditorPrefs.GetInt(HistorySizeKey, 10);
            Selection.selectionChanged += OnSelectionChanged;
            _selectedObjectInList = null;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                {
                    _history.Clear();
                }

                GUILayout.FlexibleSpace();

                EditorGUI.BeginChangeCheck();
                _historySize = EditorGUILayout.IntField(_historySize, EditorStyles.toolbarTextField, GUILayout.Width(40));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetInt(HistorySizeKey, _historySize);
                }
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Header
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Icon", GUILayout.Width(40));
                GUILayout.Label("Name");
                GUILayout.Label("Type", GUILayout.Width(100));
            }

            for (int i = 0; i < _history.Count; i++)
            {
                var obj = _history[i];
                if (obj == null)
                {
                    _history.RemoveAt(i);
                    i--;
                    continue;
                }

                var icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(obj));

                Rect rowRect = EditorGUILayout.GetControlRect(false, 20, GUILayout.ExpandWidth(true));
                if (_selectedObjectInList == obj)
                {
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.37f, 0.59f));
                }

                GUI.Label(new Rect(rowRect.x, rowRect.y, 40, rowRect.height), new GUIContent(icon));
                GUI.Label(new Rect(rowRect.x + 40, rowRect.y, rowRect.width - 140, rowRect.height), obj.name);
                GUI.Label(new Rect(rowRect.x + rowRect.width - 100, rowRect.y, 100, rowRect.height), obj.GetType().Name);

                var currentEvent = Event.current;
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rowRect.Contains(currentEvent.mousePosition))
                {
                    if (currentEvent.clickCount == 1)
                    {
                        _selectedObjectInList = obj;
                        Repaint();
                        
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new[] { obj };
                        DragAndDrop.SetGenericData("SelectionHistoryDrag", obj);
                    }
                    else if (currentEvent.clickCount == 2)
                    {
                        Selection.activeObject = obj;
                        _selectedObjectInList = obj;
                        EditorGUIUtility.PingObject(obj);
                        Event.current.Use();
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            var evt = Event.current;
            if (evt.type == EventType.MouseDrag)
            {
                if (DragAndDrop.GetGenericData("SelectionHistoryDrag") != null)
                {
                    DragAndDrop.StartDrag("Dragging from Selection History");
                    evt.Use();
                }
            }
            else if (evt.type == EventType.MouseUp)
            {
                DragAndDrop.SetGenericData("SelectionHistoryDrag", null);
            }
        }

        private void OnSelectionChanged()
        {
            var currentSelection = Selection.objects;
            if (currentSelection.Length > 0)
            {
                foreach (var obj in currentSelection)
                {
                    if (!AssetDatabase.Contains(obj))
                    {
                        continue;
                    }

                    if (!_history.Contains(obj))
                    {
                        _history.Insert(0, obj);
                    }
                    else
                    {
                        _history.Remove(obj);
                        _history.Insert(0, obj);
                    }
                }
            }
            
            if (_history.Count > _historySize)
            {
                _history = _history.Take(_historySize).ToList();
            }

            Repaint();
        }
    }
}
