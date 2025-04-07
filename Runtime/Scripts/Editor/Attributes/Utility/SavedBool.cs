using UnityEditor;

namespace DaftAppleGames.Editor.Attributes
{
    internal class SavedBool
    {
        private bool _value;
        private string _name;

        public bool Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                _value = value;
                EditorPrefs.SetBool(_name, value);
            }
        }

        public SavedBool(string name, bool value)
        {
            _name = name;
            _value = EditorPrefs.GetBool(name, value);
        }
    }
}