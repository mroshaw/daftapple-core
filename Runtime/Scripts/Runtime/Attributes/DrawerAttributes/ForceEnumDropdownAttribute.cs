using UnityEngine;

namespace DaftAppleGames.Attributes
{
    /// <summary>
    /// Lets us override some weird Unity behaviour where inspectors render ENUMs as masks - i.e. LightmapBakeType
    /// </summary>
    public class ForceEnumDropdownAttribute : PropertyAttribute
    {
    }
}