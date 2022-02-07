namespace Engine.GameStates
{
    public interface IGameState
    {
        void Pause();
        void Resume();
        void Enter();
        void Leave();
    }
}