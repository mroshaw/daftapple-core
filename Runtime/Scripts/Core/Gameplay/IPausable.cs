namespace DaftAppleGames.Gameplay
{
    public interface IPausable
    {
        public void Pause();
        public void Resume();
        protected bool IsPaused();
    }
}