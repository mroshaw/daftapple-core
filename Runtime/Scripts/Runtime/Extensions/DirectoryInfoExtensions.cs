using System.IO;
using UnityEngine;

namespace DaftAppleGames.Extensions
{
    /// <summary>
    /// Useful OS file/folder extensions
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        // Copies a folder and all it's contents to the target
        public static void DeepCopy(this DirectoryInfo directory, string destinationDir)
        {
            // Create the parent directory
            string newDestinationDir = Path.Combine(destinationDir, directory.Name);
            Directory.CreateDirectory(newDestinationDir);

            foreach (string dir in Directory.GetDirectories(directory.FullName, "*", SearchOption.AllDirectories))
            {
                string dirToCreate = dir.Replace(directory.FullName, newDestinationDir);
                Directory.CreateDirectory(dirToCreate);
            }

            foreach (string newPath in Directory.GetFiles(directory.FullName, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(directory.FullName, newDestinationDir), true);
            }

            // If there's a .meta file for the parent folder, copy that too
            string folderMetaFullPath = $"{directory}.meta";
            string metaFileName = Path.GetFileName(folderMetaFullPath);
            string metaFileDestinationFullPath = Path.Combine(destinationDir, metaFileName);
            if (File.Exists(folderMetaFullPath))
            {
                File.Copy(folderMetaFullPath, metaFileDestinationFullPath);
            }
        }
    }
}