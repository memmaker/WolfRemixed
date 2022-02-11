using Microsoft.Xna.Framework;
using System;


namespace Engine.GameStates
{
    public abstract class GameState : IGameState, IUpdateable
    {
        private bool paused;

        protected bool Paused => paused;

        bool IUpdateable.Enabled => true;


        public void Pause()
        {
            if (!paused)
            {
                OnPause();
                paused = true;
            }
        }

        public void Resume()
        {
            if (paused)
            {
                OnResume();
                paused = false;
            }
        }

        public abstract void Update(GameTime gameTime);

        protected virtual void OnEntered()
        {
        }

        protected virtual void OnLeaving()
        {
        }

        protected virtual void OnPause()
        {
        }

        protected virtual void OnResume()
        {
        }

        void IGameState.Enter()
        {
            OnEntered();
        }

        void IGameState.Leave()
        {
            OnLeaving();
        }
    }
}