using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    public abstract class EditorToolsList : ScriptableObject
    {
        // List of tools to be made available via the Wizard UI
        [SerializeField] private List<EditorTool> editorToolsList;
    }
}