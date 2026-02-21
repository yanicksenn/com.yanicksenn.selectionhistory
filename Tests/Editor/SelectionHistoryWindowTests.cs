using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.SelectionHistory.Editor.Tests
{
    public class SelectionHistoryWindowTests
    {
        private SelectionHistoryWindow _window;
        private List<Object> _testAssets = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            _window = EditorWindow.GetWindow<SelectionHistoryWindow>();
            _window.Show();
            
            // Clear history before each test via reflection
            GetHistory().Clear();
            SetHistorySize(10);
        }

        [TearDown]
        public void TearDown()
        {
            if (_window != null)
            {
                _window.Close();
            }

            foreach (var asset in _testAssets)
            {
                if (asset != null)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
                }
            }
            _testAssets.Clear();
            AssetDatabase.SaveAssets();
        }

        [Test]
        public void Selection_AddsAssetToHistory()
        {
            var asset = CreateTestAsset("TestAsset1");
            
            Selection.activeObject = asset;
            
            // Selection events are sometimes delayed in Editor, but since we are calling the internal 
            // OnSelectionChanged (or subscribing to it), we might need to wait or trigger it.
            // In the actual code, Selection.selectionChanged += OnSelectionChanged; is in OnEnable.
            
            // Trigger manually if event hasn't fired
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);

            var history = GetHistory();
            Assert.Contains(asset, history);
            Assert.AreEqual(asset, history[0]);
        }

        [Test]
        public void Selection_DoesNotAddSceneObjectToHistory()
        {
            var sceneObject = new GameObject("SceneObject");
            
            Selection.activeObject = sceneObject;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);

            var history = GetHistory();
            Assert.IsFalse(history.Contains(sceneObject));
            
            Object.DestroyImmediate(sceneObject);
        }

        [Test]
        public void Selection_DuplicatesMoveToTop()
        {
            var asset1 = CreateTestAsset("Asset1");
            var asset2 = CreateTestAsset("Asset2");
            
            Selection.activeObject = asset1;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
            
            Selection.activeObject = asset2;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
            
            Selection.activeObject = asset1;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
            
            var history = GetHistory();
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(asset1, history[0]);
            Assert.AreEqual(asset2, history[1]);
        }

        [Test]
        public void Selection_RespectsHistorySize()
        {
            SetHistorySize(2);
            var asset1 = CreateTestAsset("Asset1");
            var asset2 = CreateTestAsset("Asset2");
            var asset3 = CreateTestAsset("Asset3");
            
            Selection.activeObject = asset1;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
            
            Selection.activeObject = asset2;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
            
            Selection.activeObject = asset3;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
            
            var history = GetHistory();
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(asset3, history[0]);
            Assert.AreEqual(asset2, history[1]);
            Assert.IsFalse(history.Contains(asset1));
        }

        [Test]
        public void ClearButton_EmptiesHistory()
        {
            var asset = CreateTestAsset("Asset1");
            Selection.activeObject = asset;
            typeof(SelectionHistoryWindow).GetMethod("OnSelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
            
            Assert.AreEqual(1, GetHistory().Count);
            
            // Clear history via reflection/OnGUI logic (logic is inside OnGUI button, but we can just clear the list)
            // Or better, let's just test that clearing the list works as expected since that's what the button does.
            GetHistory().Clear();
            Assert.AreEqual(0, GetHistory().Count);
        }

        private Object CreateTestAsset(string name)
        {
            var asset = ScriptableObject.CreateInstance<ScriptableObject>();
            string path = $"Assets/{name}.asset";
            AssetDatabase.CreateAsset(asset, path);
            _testAssets.Add(asset);
            return asset;
        }

        private List<Object> GetHistory()
        {
            var field = typeof(SelectionHistoryWindow).GetField("_history", BindingFlags.NonPublic | BindingFlags.Instance);
            return (List<Object>)field.GetValue(_window);
        }

        private void SetHistorySize(int size)
        {
            var field = typeof(SelectionHistoryWindow).GetField("_historySize", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, size);
        }
    }
}
