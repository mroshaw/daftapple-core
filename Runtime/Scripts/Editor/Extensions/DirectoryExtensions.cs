using System.IO;

namespace DaftAppleGames.Editor.Extensions
{
    /// <summary>
    /// Provides Extension methods for the Directory class
    /// </summary>
    public static class DirectoryExtensions
    {
        public static void CreateFolderIfNotExists(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                return;
            }

            Directory.CreateDirectory(folderPath);
        }
    }
}