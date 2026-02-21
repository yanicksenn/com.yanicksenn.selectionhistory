using NUnit.Framework;
using UnityEngine;
using YanickSenn.SelectionHistory;

namespace YanickSenn.SelectionHistory.Tests
{
    public class SelectionProfileTests
    {
        private SelectionProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _profile = ScriptableObject.CreateInstance<SelectionProfile>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_profile != null)
            {
                Object.DestroyImmediate(_profile);
            }
        }

        [Test]
        public void Defaults_AreCorrect()
        {
            Assert.IsFalse(_profile.IsLocked);
            Assert.IsNotNull(_profile.Objects);
            Assert.IsEmpty(_profile.Objects);
        }

        [Test]
        public void IsLocked_CanBeSet()
        {
            _profile.IsLocked = true;
            Assert.IsTrue(_profile.IsLocked);
            
            _profile.IsLocked = false;
            Assert.IsFalse(_profile.IsLocked);
        }

        [Test]
        public void Objects_CanBeModified()
        {
            var go = new GameObject("Test");
            _profile.Objects.Add(go);
            
            Assert.AreEqual(1, _profile.Objects.Count);
            Assert.AreEqual(go, _profile.Objects[0]);
            
            Object.DestroyImmediate(go);
        }
    }
}
