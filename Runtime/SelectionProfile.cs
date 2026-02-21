using System.Collections.Generic;
using UnityEngine;

namespace YanickSenn.SelectionHistory
{
    [CreateAssetMenu(fileName = "New Selection Profile", menuName = "YanickSenn/Selection History/Selection Profile")]
    public class SelectionProfile : ScriptableObject
    {
        [SerializeField]
        private bool _isLocked;

        [SerializeField]
        private List<Object> _objects = new List<Object>();

        public bool IsLocked
        {
            get => _isLocked;
            set => _isLocked = value;
        }

        public List<Object> Objects => _objects;
    }
}
