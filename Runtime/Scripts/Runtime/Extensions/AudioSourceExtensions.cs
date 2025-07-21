using UnityEngine;
using Random = UnityEngine.Random;

namespace DaftAppleGames.Extensions
{
    public static class AudioSourceExtensions
    {
        /// <summary>
        /// Play a random clip from the given clip array
        /// </summary>
        public static void PlayRandomClip(this AudioSource source, AudioClip[] clips)
        {
            if (clips.Length == 0)
            {
                return;
            }

            source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}