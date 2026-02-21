using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YanickSenn.SelectionHistory.Editor
{
    public class SelectionProfileWindow : EditorWindow
    {
        private List<SelectionProfile> _profiles = new List<SelectionProfile>();
        private string[] _profileNames = Array.Empty<string>();
        private int _selectedProfileIndex = -1;
        
        private SelectionProfile _selectedProfile;
        private SerializedObject _serializedProfile;
        private SerializedProperty _objectsProperty;
        private SerializedProperty _isLockedProperty;

        private Vector2 _scrollPosition;
        private Object _selectedObjectInList;

        [MenuItem("Window/Selection Profile")]
        public static void ShowWindow()
        {
            GetWindow<SelectionProfileWindow>("Selection Profile");
        }

        private void OnEnable()
        {
            RefreshProfiles();
        }

        private void OnFocus()
        {
            RefreshProfiles();
        }

        private void RefreshProfiles()
        {
            var guids = AssetDatabase.FindAssets("t:SelectionProfile");
            _profiles = guids.Select(g => AssetDatabase.LoadAssetAtPath<SelectionProfile>(AssetDatabase.GUIDToAssetPath(g)))
                             .Where(p => p != null)
                             .ToList();
            
            _profileNames = _profiles.Select(p => p.name).ToArray();

            if (_selectedProfile != null && _profiles.Contains(_selectedProfile))
            {
                _selectedProfileIndex = _profiles.IndexOf(_selectedProfile);
            }
            else
            {
                _selectedProfileIndex = _profiles.Count > 0 ? 0 : -1;
            }

            UpdateSelectedProfile();
        }

        private void UpdateSelectedProfile()
        {
            if (_selectedProfileIndex >= 0 && _selectedProfileIndex < _profiles.Count)
            {
                _selectedProfile = _profiles[_selectedProfileIndex];
                if (_selectedProfile != null)
                {
                    _serializedProfile = new SerializedObject(_selectedProfile);
                    _objectsProperty = _serializedProfile.FindProperty("_objects");
                    _isLockedProperty = _serializedProfile.FindProperty("_isLocked");
                }
                else
                {
                    _serializedProfile = null;
                    _objectsProperty = null;
                    _isLockedProperty = null;
                }
            }
            else
            {
                _selectedProfile = null;
                _serializedProfile = null;
                _objectsProperty = null;
                _isLockedProperty = null;
            }
        }

        private void OnGUI()
        {
            if (_serializedProfile != null)
            {
                _serializedProfile.Update();
            }

            DrawToolbar();

            if (_selectedProfile == null || _serializedProfile == null)
            {
                EditorGUILayout.HelpBox("Create or select a Selection Profile to begin.", MessageType.Info);
                return;
            }

            HandleDragAndDropAdd();

            DrawHeader();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawList();
            EditorGUILayout.EndScrollView();

            if (_serializedProfile != null && _serializedProfile.targetObject != null)
            {
                _serializedProfile.ApplyModifiedProperties();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                _selectedProfileIndex = EditorGUILayout.Popup(_selectedProfileIndex, _profileNames, EditorStyles.toolbarPopup, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateSelectedProfile();
                }

                if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    CreateNewProfile();
                }

                EditorGUI.BeginDisabledGroup(_selectedProfile == null);
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    DeleteSelectedProfile();
                }

                if (_isLockedProperty != null)
                {
                    var lockIcon = _isLockedProperty.boolValue ? EditorGUIUtility.IconContent("LockIcon-On") : EditorGUIUtility.IconContent("LockIcon");
                    lockIcon.tooltip = "Lock Profile";
                    _isLockedProperty.boolValue = GUILayout.Toggle(_isLockedProperty.boolValue, lockIcon, EditorStyles.toolbarButton, GUILayout.Width(25));
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private void CreateNewProfile()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Selection Profile", "New Selection Profile", "asset", "Save new selection profile");
            if (!string.IsNullOrEmpty(path))
            {
                var newProfile = ScriptableObject.CreateInstance<SelectionProfile>();
                AssetDatabase.CreateAsset(newProfile, path);
                AssetDatabase.SaveAssets();
                
                RefreshProfiles();
                _selectedProfileIndex = _profiles.IndexOf(newProfile);
                UpdateSelectedProfile();
            }
        }

        private void DeleteSelectedProfile()
        {
            if (_selectedProfile != null)
            {
                if (EditorUtility.DisplayDialog("Delete Profile", $"Are you sure you want to delete the profile '{_selectedProfile.name}'?", "Yes", "No"))
                {
                    string path = AssetDatabase.GetAssetPath(_selectedProfile);
                    AssetDatabase.DeleteAsset(path);
                    
                    _selectedProfile = null;
                    RefreshProfiles();
                }
            }
        }

        private void HandleDragAndDropAdd()
        {
            var evt = Event.current;
            var dropArea = new Rect(0, EditorGUIUtility.singleLineHeight, position.width, position.height - EditorGUIUtility.singleLineHeight);
            
            if (dropArea.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                {
                    if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.GetGenericData("SelectionProfileReorder") == null)
                    {
                        if (_selectedProfile.IsLocked)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                            return;
                        }

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            
                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                if (obj != null)
                                {
                                    AddObject(obj);
                                }
                            }
                            evt.Use();
                        }
                    }
                }
            }
        }

        private void AddObject(Object obj)
        {
            for (int i = 0; i < _objectsProperty.arraySize; i++)
            {
                if (_objectsProperty.GetArrayElementAtIndex(i).objectReferenceValue == obj)
                {
                    return;
                }
            }

            _objectsProperty.arraySize++;
            var prop = _objectsProperty.GetArrayElementAtIndex(_objectsProperty.arraySize - 1);
            prop.objectReferenceValue = obj;
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Icon", GUILayout.Width(40));
                GUILayout.Label("Name");
                GUILayout.Label("Type", GUILayout.Width(100));
            }
        }

        private void DrawList()
        {
            var evt = Event.current;

            for (int i = 0; i < _objectsProperty.arraySize; i++)
            {
                var prop = _objectsProperty.GetArrayElementAtIndex(i);
                var obj = prop.objectReferenceValue;

                if (obj == null)
                {
                    _objectsProperty.DeleteArrayElementAtIndex(i);
                    i--;
                    continue;
                }

                Rect rowRect = EditorGUILayout.GetControlRect(false, 20, GUILayout.ExpandWidth(true));
                
                if (_selectedObjectInList == obj)
                {
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.37f, 0.59f));
                }

                // Drop line indicator
                int? draggedIndex = DragAndDrop.GetGenericData("SelectionProfileReorder") as int?;
                if (!_selectedProfile.IsLocked && draggedIndex.HasValue && rowRect.Contains(evt.mousePosition))
                {
                    Rect dropLine = new Rect(rowRect.x, evt.mousePosition.y < rowRect.center.y ? rowRect.yMin : rowRect.yMax - 2, rowRect.width, 2);
                    EditorGUI.DrawRect(dropLine, Color.white);
                }

                var icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(obj));
                GUI.Label(new Rect(rowRect.x, rowRect.y, 40, rowRect.height), new GUIContent(icon));
                GUI.Label(new Rect(rowRect.x + 40, rowRect.y, rowRect.width - 140, rowRect.height), obj.name);
                GUI.Label(new Rect(rowRect.x + rowRect.width - 100, rowRect.y, 100, rowRect.height), obj.GetType().Name);

                HandleItemEvents(rowRect, i, obj, evt);
            }
            
            Rect remainingRect = GUILayoutUtility.GetRect(0.0f, 0.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (evt.type == EventType.DragUpdated && DragAndDrop.GetGenericData("SelectionProfileReorder") != null)
            {
                if (remainingRect.Contains(evt.mousePosition))
                {
                    if (!_selectedProfile.IsLocked)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
            }
            else if (evt.type == EventType.DragPerform && DragAndDrop.GetGenericData("SelectionProfileReorder") != null)
            {
                if (remainingRect.Contains(evt.mousePosition))
                {
                    if (!_selectedProfile.IsLocked)
                    {
                        DragAndDrop.AcceptDrag();
                        int fromIndex = (int)DragAndDrop.GetGenericData("SelectionProfileReorder");
                        int toIndex = _objectsProperty.arraySize - 1;
                        
                        if (fromIndex != toIndex)
                        {
                            _objectsProperty.MoveArrayElement(fromIndex, toIndex);
                        }
                    }
                    
                    DragAndDrop.SetGenericData("SelectionProfileReorder", null);
                    evt.Use();
                }
            }

            if (evt.type == EventType.MouseDrag && DragAndDrop.GetGenericData("SelectionProfileReorder") != null)
            {
                DragAndDrop.StartDrag("Dragging from Profile");
                evt.Use();
            }

            if (evt.type == EventType.MouseUp || evt.type == EventType.DragExited)
            {
                DragAndDrop.SetGenericData("SelectionProfileReorder", null);
            }
        }

        private void HandleItemEvents(Rect rowRect, int index, Object obj, Event evt)
        {
            if (rowRect.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.MouseDown)
                {
                    if (evt.button == 0)
                    {
                        _selectedObjectInList = obj;
                        
                        if (evt.clickCount == 1)
                        {
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new[] { obj };
                            DragAndDrop.SetGenericData("SelectionProfileReorder", index);
                            Repaint();
                        }
                        else if (evt.clickCount == 2)
                        {
                            Selection.activeObject = obj;
                            EditorGUIUtility.PingObject(obj);
                            evt.Use();
                        }
                    }
                    else if (evt.button == 1)
                    {
                        int capturedIndex = index;
                        GenericMenu menu = new GenericMenu();
                        if (!_selectedProfile.IsLocked)
                        {
                            menu.AddItem(new GUIContent("Remove"), false, () => 
                            {
                                _serializedProfile.Update();
                                
                                var p = _objectsProperty.GetArrayElementAtIndex(capturedIndex);
                                if (p.objectReferenceValue != null)
                                {
                                    p.objectReferenceValue = null;
                                }
                                _objectsProperty.DeleteArrayElementAtIndex(capturedIndex);
                                
                                _serializedProfile.ApplyModifiedProperties();
                            });
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Remove"));
                        }
                        menu.ShowAsContext();
                        evt.Use();
                    }
                }
                else if (evt.type == EventType.DragUpdated && DragAndDrop.GetGenericData("SelectionProfileReorder") != null)
                {
                    if (!_selectedProfile.IsLocked)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    evt.Use();
                }
                else if (evt.type == EventType.DragPerform && DragAndDrop.GetGenericData("SelectionProfileReorder") != null)
                {
                    if (!_selectedProfile.IsLocked)
                    {
                        DragAndDrop.AcceptDrag();
                        int fromIndex = (int)DragAndDrop.GetGenericData("SelectionProfileReorder");
                        int toIndex = evt.mousePosition.y < rowRect.center.y ? index : index + 1;
                        
                        if (fromIndex < toIndex)
                        {
                            toIndex--;
                        }
                        
                        if (fromIndex != toIndex)
                        {
                            _objectsProperty.MoveArrayElement(fromIndex, toIndex);
                        }
                    }
                    
                    DragAndDrop.SetGenericData("SelectionProfileReorder", null);
                    evt.Use();
                }
            }
        }
    }
}
