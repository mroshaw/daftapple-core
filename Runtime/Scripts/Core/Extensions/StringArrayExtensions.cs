using System;

namespace DaftAppleGames.Extensions
{
    public static class StringArrayExtensions
    {
        public static bool ItemInString(this string[] stringArray, string stringToCheck)
        {
            foreach (string arrayItem in stringArray)
            {
                if (stringToCheck.IndexOf(arrayItem, StringComparison.OrdinalIgnoreCase) >= 0)
                    // if (arrayItem.IndexOf(stringToCheck, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}