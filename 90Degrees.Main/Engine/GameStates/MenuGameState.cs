using System;
using System.Collections.Generic;
using Artemis;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Twengine;
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
        public event ButtonEventHandler ButtonAppendedStringChanged;
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
                var button = GetSelectedButton();
                ButtonPressed(button);
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