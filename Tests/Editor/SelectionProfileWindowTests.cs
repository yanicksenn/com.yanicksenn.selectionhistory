using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.SelectionHistory.Editor.Tests
{
    public class SelectionProfileWindowTests
    {
        private SelectionProfileWindow _window;
        private SelectionProfile _testProfile;
        private List<Object> _testAssets = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            _window = EditorWindow.GetWindow<SelectionProfileWindow>();
            _window.Show();

            _testProfile = ScriptableObject.CreateInstance<SelectionProfile>();
            string path = $"Assets/TestSelectionProfile.asset";
            AssetDatabase.CreateAsset(_testProfile, path);
            _testAssets.Add(_testProfile);
            AssetDatabase.SaveAssets();

            // Refresh profiles so the newly created one is found
            typeof(SelectionProfileWindow).GetMethod("RefreshProfiles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);

            // Select our test profile manually via reflection
            var profilesField = typeof(SelectionProfileWindow).GetField("_profiles", BindingFlags.NonPublic | BindingFlags.Instance);
            var profiles = (List<SelectionProfile>)profilesField.GetValue(_window);
            
            int index = profiles.IndexOf(_testProfile);
            typeof(SelectionProfileWindow).GetField("_selectedProfileIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_window, index);

            typeof(SelectionProfileWindow).GetMethod("UpdateSelectedProfile", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_window, null);
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
        public void AddObject_AppendsObjectToProfile()
        {
            var testAsset = CreateTestAsset("TestObject");
            
            var addObjectMethod = typeof(SelectionProfileWindow).GetMethod("AddObject", BindingFlags.NonPublic | BindingFlags.Instance);
            addObjectMethod.Invoke(_window, new object[] { testAsset });

            ApplySerializedProperties();

            Assert.AreEqual(1, _testProfile.Objects.Count);
            Assert.AreEqual(testAsset, _testProfile.Objects[0]);
        }

        [Test]
        public void AddObject_DoesNotAddDuplicates()
        {
            var testAsset = CreateTestAsset("TestObject");
            
            var addObjectMethod = typeof(SelectionProfileWindow).GetMethod("AddObject", BindingFlags.NonPublic | BindingFlags.Instance);
            
            addObjectMethod.Invoke(_window, new object[] { testAsset });
            ApplySerializedProperties();
            Assert.AreEqual(1, _testProfile.Objects.Count);

            // Add second time
            addObjectMethod.Invoke(_window, new object[] { testAsset });
            ApplySerializedProperties();

            Assert.AreEqual(1, _testProfile.Objects.Count);
        }

        private Object CreateTestAsset(string name)
        {
            var asset = ScriptableObject.CreateInstance<ScriptableObject>();
            string path = $"Assets/{name}.asset";
            AssetDatabase.CreateAsset(asset, path);
            _testAssets.Add(asset);
            return asset;
        }

        private void ApplySerializedProperties()
        {
            var serializedProfileField = typeof(SelectionProfileWindow).GetField("_serializedProfile", BindingFlags.NonPublic | BindingFlags.Instance);
            var serializedProfile = (SerializedObject)serializedProfileField.GetValue(_window);
            serializedProfile.ApplyModifiedProperties();
        }
    }
}
