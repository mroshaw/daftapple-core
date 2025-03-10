using UnityEngine;

namespace DaftAppleGames.Gameplay
{
    public interface IPausable
    {
        public abstract void Pause();
        public abstract void Resume();
        protected abstract bool IsPaused();
    }
}