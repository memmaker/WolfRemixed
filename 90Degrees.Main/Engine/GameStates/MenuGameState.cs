using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAGameGui.Gui.Widgets;

namespace TurnBasedCombat.GameStates
{
    public delegate void ButtonEventHandler(ButtonWidget instance);

    public abstract class MenuGameState : GameState, Engine.GameStates.IUpdateable
    {
        protected MenuWindowWidget mMenu;

        protected GameStateManager mGameStateManager;
        protected KeyboardState mLastKeyboardState;
        private MouseState mLastMouseState;

        protected int MarkerLimit { get; set; }

        public bool Enabled => true;

        private int mMarkedButtons;
        public event ButtonEventHandler ButtonPressed;
        public event ButtonEventHandler ButtonMarked;
        public event ButtonEventHandler ButtonUnmarked;
        public event ButtonEventHandler CounterIncreased;
        public event ButtonEventHandler CounterDecreased;

        public MenuGameState(GameStateManager gameStateManager)
        {
            mGameStateManager = gameStateManager;
            mLastKeyboardState = Keyboard.GetState();
            mLastMouseState = Mouse.GetState();
            MarkerLimit = 1;
            mMarkedButtons = 0;
        }

        protected virtual void KeyPressed(Keys key)
        {
            if (mMenu == null || !mMenu.IsVisible) return;


            if (key == Keys.Enter)
            {
                var button = mMenu.GetSelectedButton();
                ButtonPressed?.Invoke(button);
            }
            else if (key == Keys.Up)
            {
                mMenu.SelectPrevious();
            }
            else if (key == Keys.Down)
            {
                mMenu.SelectNext();
            }
            else if (key == Keys.Left)
            {
                var button = mMenu.GetSelectedButton();
                DecreaseCounter(button);
            }
            else if (key == Keys.Right)
            {
                var button = mMenu.GetSelectedButton();
                IncreaseCounter(button);
            }

        }

        private void OnPressed(ButtonWidget button)
        {
            ButtonPressed?.Invoke(button);
        }

        private void OnMarked(ButtonWidget button)
        {
            ButtonMarked?.Invoke(button);
        }

        private void OnUnmarked(ButtonWidget button)
        {
            ButtonUnmarked?.Invoke(button);
        }

        private void OnCounterChange(ButtonWidget button, bool increased)
        {
            if (increased)
            {
                if (CounterIncreased == null) return;
                CounterIncreased(button);
            }
            else
            {
                if (CounterDecreased == null) return;
                CounterDecreased(button);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            mMenu.IsVisible = false;
        }

        protected override void OnResume()
        {
            base.OnResume();
            mMenu.IsVisible = true;
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            if (mLastMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
            {
                ButtonWidget selectedButton = mMenu.GetSelectedButton();

                if (selectedButton != null)
                {
                    if (selectedButton.AppendCounter)
                    {
                        IncreaseCounter(selectedButton);
                    }

                    else
                        OnPressed(selectedButton);
                }
            }
            if (mLastMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed)
            {
                ButtonWidget selectedButton = mMenu.GetSelectedButton();
                if (selectedButton != null)
                {
                    if (selectedButton.AppendCounter)
                    {
                        DecreaseCounter(selectedButton);
                    }
                    else
                        OnPressed(selectedButton);
                }
            }

            foreach (Keys pressedKey in keyboardState.GetPressedKeys())
            {
                if (mLastKeyboardState.IsKeyUp(pressedKey))
                {
                    KeyPressed(pressedKey);
                }
            }

            mLastKeyboardState = keyboardState;
            mLastMouseState = mouseState;
        }

        private void IncreaseCounter(ButtonWidget selectedButton)
        {
            int oldValue = selectedButton.Counter;
            selectedButton.Increase();
            if (selectedButton.Counter > oldValue)
                OnCounterChange(selectedButton, true);
        }

        private void DecreaseCounter(ButtonWidget selectedButton)
        {
            int oldValue = selectedButton.Counter;
            selectedButton.Decrease();
            if (selectedButton.Counter < oldValue)
                OnCounterChange(selectedButton, false);
        }
    }
}