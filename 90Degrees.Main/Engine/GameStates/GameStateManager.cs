using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Engine.GameStates
{
    public class GameStateManager : IDisposable, IGameComponent, IGameStateService
    {
        private GameServiceContainer gameServices;

        private bool disposeDroppedStates;

        private List<KeyValuePair<IGameState, GameStateModality>> gameStates;

        private List<IUpdateable> updateableStates;

        private List<IDrawable> drawableStates;

        public bool DisposeDroppedStates
        {
            get
            {
                return disposeDroppedStates;
            }
            set
            {
                disposeDroppedStates = value;
            }
        }

        public IGameState ActiveState
        {
            get
            {
                int count = gameStates.Count;
                if (count == 0)
                {
                    return null;
                }

                return gameStates[count - 1].Key;
            }
        }

        public GameStateManager()
        {
            gameStates = new List<KeyValuePair<IGameState, GameStateModality>>();
            updateableStates = new List<IUpdateable>();
            drawableStates = new List<IDrawable>();
        }

        public GameStateManager(GameServiceContainer gameServices)
            : this()
        {
            this.gameServices = gameServices;
            gameServices.AddService(typeof(IGameStateService), (object)this);
        }

        public void Dispose()
        {
            leaveAllActiveStates();
            if (gameServices != null)
            {
                object service = gameServices.GetService(typeof(IGameStateService));
                if (object.ReferenceEquals(service, this))
                {
                    gameServices.RemoveService(typeof(IGameStateService));
                }
            }
        }

        public void Pause()
        {
            if (gameStates.Count > 0)
            {
                gameStates[gameStates.Count - 1].Key.Pause();
            }
        }

        public void Resume()
        {
            if (gameStates.Count > 0)
            {
                gameStates[gameStates.Count - 1].Key.Resume();
            }
        }

        public void Push(IGameState state)
        {
            Push(state, GameStateModality.Exclusive);
        }

        public void Push(IGameState state, GameStateModality modality)
        {
            Pause();
            if (modality == GameStateModality.Exclusive)
            {
                drawableStates.Clear();
                updateableStates.Clear();
            }

            gameStates.Add(new KeyValuePair<IGameState, GameStateModality>(state, modality));
            appendToUpdateableAndDrawableList(state);
            try
            {
                state.Enter();
            }
            catch (Exception)
            {
                Pop();
                throw;
            }
        }

        public IGameState Pop()
        {
            int num = gameStates.Count - 1;
            if (num < 0)
            {
                throw new InvalidOperationException("No game states are on the stack");
            }

            KeyValuePair<IGameState, GameStateModality> keyValuePair = gameStates[num];
            IGameState key = keyValuePair.Key;
            key.Leave();
            gameStates.RemoveAt(num);
            if (keyValuePair.Value == GameStateModality.Exclusive)
            {
                updateableStates.Clear();
                drawableStates.Clear();
                rebuildUpdateableAndDrawableListRecursively(num - 1);
            }
            else
            {
                removeFromUpdateableAndDrawableList(keyValuePair.Key);
            }

            disposeIfSupportedAndDesired(keyValuePair.Key);
            Resume();
            return key;
        }

        public IGameState Switch(IGameState state)
        {
            return Switch(state, GameStateModality.Exclusive);
        }

        public IGameState Switch(IGameState state, GameStateModality modality)
        {
            int count = gameStates.Count;
            if (count == 0)
            {
                Push(state, modality);
                return null;
            }

            int index = count - 1;
            KeyValuePair<IGameState, GameStateModality> keyValuePair = gameStates[index];
            IGameState key = keyValuePair.Key;
            key.Leave();
            disposeIfSupportedAndDesired(key);
            if (keyValuePair.Value == GameStateModality.Popup)
            {
                removeFromUpdateableAndDrawableList(key);
            }
            else
            {
                updateableStates.Clear();
                drawableStates.Clear();
            }

            KeyValuePair<IGameState, GameStateModality> value = new KeyValuePair<IGameState, GameStateModality>(state, modality);
            gameStates[index] = value;
            if (keyValuePair.Value == GameStateModality.Exclusive && modality == GameStateModality.Popup)
            {
                rebuildUpdateableAndDrawableListRecursively(index);
            }
            else
            {
                appendToUpdateableAndDrawableList(state);
            }

            state.Enter();
            return key;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < updateableStates.Count; i++)
            {
                IUpdateable val = updateableStates[i];
                if (val.Enabled)
                {
                    val.Update(gameTime);
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            for (int i = 0; i < drawableStates.Count; i++)
            {
                IDrawable val = drawableStates[i];
                if (val.get_Visible())
                {
                    drawableStates[i].Draw(gameTime);
                }
            }
        }

        private void disposeIfSupportedAndDesired(IGameState state)
        {
            if (disposeDroppedStates)
            {
                (state as IDisposable)?.Dispose();
            }
        }

        private void rebuildUpdateableAndDrawableListRecursively(int index)
        {
            if (index >= 0)
            {
                if (gameStates[index].Value != 0)
                {
                    rebuildUpdateableAndDrawableListRecursively(index - 1);
                }

                appendToUpdateableAndDrawableList(gameStates[index].Key);
            }
        }

        private void removeFromUpdateableAndDrawableList(IGameState state)
        {
            int num = drawableStates.Count - 1;
            if (num > -1 && object.ReferenceEquals(drawableStates[num], state))
            {
                drawableStates.RemoveAt(num);
            }

            int num2 = updateableStates.Count - 1;
            if (num2 > -1 && object.ReferenceEquals(updateableStates[num2], state))
            {
                updateableStates.RemoveAt(num2);
            }
        }

        private void leaveAllActiveStates()
        {
            for (int num = gameStates.Count - 1; num >= 0; num--)
            {
                IGameState key = gameStates[num].Key;
                key.Leave();
                disposeIfSupportedAndDesired(key);
                gameStates.RemoveAt(num);
            }

            drawableStates.Clear();
            updateableStates.Clear();
        }

        private void appendToUpdateableAndDrawableList(IGameState state)
        {
            IUpdateable val = state as IUpdateable;
            if (val != null)
            {
                updateableStates.Add(val);
            }

            IDrawable val2 = state as IDrawable;
            if (val2 != null)
            {
                drawableStates.Add(val2);
            }
        }

        public void Initialize()
        {
        }
    }
}
