using UnityEngine;

namespace DaftAppleGames.Extensions
{
    public static class QuaternionExtensions
    {
        public static Quaternion RandomRotationX()
        {
            float randomX = Random.Range(0f, 360f);
            return Quaternion.Euler(randomX, 0f, 0f);
        }
    }
}