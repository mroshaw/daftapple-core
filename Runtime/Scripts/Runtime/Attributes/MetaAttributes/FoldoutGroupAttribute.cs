using System;

namespace DaftAppleGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class FoldoutGroupAttribute : MetaAttribute, IGroupAttribute
    {
        public string Name { get; private set; }

        public FoldoutGroupAttribute(string name)
        {
            Name = name;
        }
    }
}