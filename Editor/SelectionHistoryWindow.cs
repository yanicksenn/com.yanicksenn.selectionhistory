using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.SelectionHistory.Editor
{
    public class SelectionHistoryWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Object _selectedObjectInList;

        private List<Object> History => SelectionHistoryState.instance.History;

        private int HistorySize
        {
            get => SelectionHistoryState.instance.HistorySize;
            set => SelectionHistoryState.instance.HistorySize = value;
        }

        [MenuItem("Window/Selection History")]
        public static void ShowWindow()
        {
            GetWindow<SelectionHistoryWindow>("Selection History");
        }

        private void OnEnable()
        {
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
                    History.Clear();
                    SelectionHistoryState.instance.SaveState();
                }

                GUILayout.FlexibleSpace();

                EditorGUI.BeginChangeCheck();
                HistorySize = EditorGUILayout.IntField(HistorySize, EditorStyles.toolbarTextField, GUILayout.Width(40));
                if (EditorGUI.EndChangeCheck())
                {
                    if (History.Count > HistorySize)
                    {
                        History.RemoveRange(HistorySize, History.Count - HistorySize);
                    }
                    SelectionHistoryState.instance.SaveState();
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

            for (int i = 0; i < History.Count; i++)
            {
                var obj = History[i];
                if (obj == null)
                {
                    History.RemoveAt(i);
                    i--;
                    SelectionHistoryState.instance.SaveState();
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
            bool changed = false;
            if (currentSelection.Length > 0)
            {
                foreach (var obj in currentSelection)
                {
                    if (!AssetDatabase.Contains(obj))
                    {
                        continue;
                    }

                    if (!History.Contains(obj))
                    {
                        History.Insert(0, obj);
                        changed = true;
                    }
                    else if (History.IndexOf(obj) != 0)
                    {
                        History.Remove(obj);
                        History.Insert(0, obj);
                        changed = true;
                    }
                }
            }
            
            if (History.Count > HistorySize)
            {
                History.RemoveRange(HistorySize, History.Count - HistorySize);
                changed = true;
            }

            if (changed)
            {
                SelectionHistoryState.instance.SaveState();
                Repaint();
            }
        }
    }
}
