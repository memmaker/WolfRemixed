using System;
using System.Collections.Generic;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Game.States;
using XNAGameGui.Gui.Widgets;

namespace TurnBasedCombat.GameStates
{
    public delegate void ButtonEventHandler(ButtonWidget instance);

    public abstract class MenuGameState : GameState
    {
        protected MenuWindowWidget mMenu;

        protected GameStateManager mGameStateManager;
        protected KeyboardState mLastKeyboardState;
        private MouseState mLastMouseState;
        
        private string mEditOnButton;
        protected int MarkerLimit { get; set; }
        private int mMarkedButtons;
        public event ButtonEventHandler ButtonPressed;
        public event ButtonEventHandler ButtonMarked;
        public event ButtonEventHandler ButtonUnmarked;
        public event ButtonEventHandler ButtonAppendedStringChanged;
        public event ButtonEventHandler CounterIncreased;
        public event ButtonEventHandler CounterDecreased;

        public MenuGameState(GameStateManager gameStateManager)
        {
            mGameStateManager = gameStateManager;
            mLastKeyboardState = Keyboard.GetState();
            mLastMouseState = Mouse.GetState();
            mEditOnButton = "";
            MarkerLimit = 1;
            mMarkedButtons = 0;
        }

        protected virtual void KeyPressed(Keys key)
        {
            if (mMenu == null || !mMenu.IsVisible) return;


            if (mEditOnButton != "" && (key != Keys.LeftShift))
            {
                if (key == Keys.Enter)
                {
                    mMenu.Buttons[mEditOnButton].AppendedString = mMenu.Buttons[mEditOnButton].AppendedString.Substring(0, mMenu.Buttons[mEditOnButton].AppendedString.Length - 1);
                    OnAppendedStringChanged(mMenu.Buttons[mEditOnButton]);
                    mEditOnButton = "";
                }
                else if (key == Keys.Back)
                {
                    if (mMenu.Buttons[mEditOnButton].AppendedString != "_")
                    {
                        mMenu.Buttons[mEditOnButton].AppendedString = mMenu.Buttons[mEditOnButton].AppendedString.Substring(0, mMenu.Buttons[mEditOnButton].AppendedString.Length - 2);
                        mMenu.Buttons[mEditOnButton].AppendedString += "_";
                    }
                }
                else if (key == Keys.Space)
                {
                    mMenu.Buttons[mEditOnButton].AppendedString = mMenu.Buttons[mEditOnButton].AppendedString.Substring(0, mMenu.Buttons[mEditOnButton].AppendedString.Length - 1);
                    mMenu.Buttons[mEditOnButton].AppendedString += " _";
                }
                else
                {
                    mMenu.Buttons[mEditOnButton].AppendedString = mMenu.Buttons[mEditOnButton].AppendedString.Substring(0, mMenu.Buttons[mEditOnButton].AppendedString.Length - 1);
                    if (mMenu.Buttons[mEditOnButton].AppendedString == "_")
                        mMenu.Buttons[mEditOnButton].AppendedString += Enum.GetName(typeof(Keys), key).ToUpper() + "_";
                    else
                        mMenu.Buttons[mEditOnButton].AppendedString += Enum.GetName(typeof(Keys), key).ToLower() + "_";
                }
            }

        }

        private void OnPressed(ButtonWidget button)
        {

            if (ButtonPressed == null) return;
            ButtonPressed(button);
        }

        private void OnMarked(ButtonWidget button)
        {

            if (ButtonMarked == null) return;
            ButtonMarked(button);
        }

        private void OnUnmarked(ButtonWidget button)
        {

            if (ButtonUnmarked == null) return;
            ButtonUnmarked(button);
        }

        private void OnAppendedStringChanged(ButtonWidget button)
        {
            if (ButtonAppendedStringChanged == null) return;
            ButtonAppendedStringChanged(button);
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
                ButtonWidget selectedButton = GetSelectedButton();

                if (selectedButton != null)
                {
                    if (selectedButton.IsMarkable)
                    {
                        if (selectedButton.IsMarked)    // switching off
                        {
                            mMarkedButtons--;
                            selectedButton.IsMarked = !selectedButton.IsMarked;
                            OnUnmarked(selectedButton);
                        }
                        else if (mMarkedButtons < MarkerLimit)  // switching on
                        {
                            mMarkedButtons++;
                            selectedButton.IsMarked = !selectedButton.IsMarked;
                            OnMarked(selectedButton);
                        }
                    }
                    else if (selectedButton.AppendCounter)
                    {
                        int oldValue = selectedButton.Counter;
                        selectedButton.Increase();
                        if (selectedButton.Counter > oldValue)
                            OnCounterChange(selectedButton, true);

                    }
                    else if (selectedButton.AppendString)
                    {
                        mEditOnButton = selectedButton.Text;
                        mMenu.Buttons[mEditOnButton].AppendedString = "_";
                    }
                    else
                        OnPressed(selectedButton);
                }
            }
            if (mLastMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed)
            {
                ButtonWidget selectedButton = GetSelectedButton();
                if (selectedButton != null)
                {
                    if (selectedButton.AppendCounter)
                    {
                        int oldValue = selectedButton.Counter;
                        selectedButton.Decrease();
                        if (selectedButton.Counter < oldValue)
                            OnCounterChange(selectedButton, false);

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
        public List<string> GetMarkedList()
        {
            List<string> markedButtons = new List<string>();
            foreach (ButtonWidget buttonWidget in mMenu.Buttons.Values)
            {
                if (buttonWidget.IsMarked)
                {
                    markedButtons.Add(buttonWidget.Text);
                }
            }
            return markedButtons;
        }
        public ButtonWidget GetSelectedButton()
        {
            foreach (ButtonWidget buttonWidget in mMenu.Buttons.Values)
            {
                if (buttonWidget.IsSelected)
                {
                    return buttonWidget;
                }
            }
            return null;
        }
    }
}