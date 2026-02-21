using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.SelectionHistory.Editor
{
    [FilePath("UserSettings/SelectionHistoryState.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SelectionHistoryState : ScriptableSingleton<SelectionHistoryState>
    {
        [SerializeField]
        private int _historySize = 10;

        [SerializeField]
        private List<Object> _history = new List<Object>();

        public int HistorySize
        {
            get => _historySize;
            set => _historySize = value;
        }

        public List<Object> History => _history;

        public void SaveState()
        {
            Save(true);
        }
    }
}
