using System;
using System.Text.RegularExpressions;

namespace DaftAppleGames.Extensions
{
    public static class StringArrayExtensions
    {
        private static readonly Regex Whitespace = new Regex(@"\s+");

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

        public static string RemoveWhiteSpace(this string stringToParse)
        {
            return Whitespace.Replace(stringToParse, "");
        }

        public static string ReplaceWhiteSpaceWithUnderscore(this string stringToParse)
        {
            return Whitespace.Replace(stringToParse, "_");
        }
    }
}