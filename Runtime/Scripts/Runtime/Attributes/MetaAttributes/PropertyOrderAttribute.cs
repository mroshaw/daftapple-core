using System;

namespace DaftAppleGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PropertyOrderAttribute : MetaAttribute
    {
        public int Order { get; private set; }

        public PropertyOrderAttribute(int order)
        {
            Order = order;
        }
    }
}