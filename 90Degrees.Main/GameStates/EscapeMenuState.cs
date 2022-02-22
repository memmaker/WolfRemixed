using Artemis.Manager;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TurnBasedCombat.GameStates;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    public class EscapeMenuState : MenuGameState, Engine.GameStates.IUpdateable
    {
        private readonly GameStateManager mStateManager;


        public EscapeMenuState(GameStateManager stateManager)
            : base(stateManager)
        {


            mStateManager = stateManager;
            ButtonPressed += new ButtonEventHandler(MainMenuState_ButtonPressed);
        }
        void MainMenuState_ButtonPressed(ButtonWidget instance)
        {
            switch (instance.Text)
            {
                case "Resume":
                    mGameStateManager.Pop();
                    break;
                case "Options":
                    mGameStateManager.Push(new OptionsMenuState(mStateManager));
                    break;
                case "Quit to mainmenu":
                    mGameStateManager.Pop();
                    mGameStateManager.Pop();
                    break;
            }
        }


        protected override void OnEntered()
        {
            base.OnEntered();
            float menuWidth = GameGui.Viewport.Width * 0.4f;
            mMenu = new MenuWindowWidget((int)menuWidth, (int)(menuWidth - (2 * 20)))
            {
                //mMenu.Background = AssetManager.Default.LoadTexture("Menu/titlescreen_widescreen.png");
                Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0)),
                DrawLabelBackground = false
            };
            mMenu.AddSimpleButtons(new List<string>() { "Resume", "Options", "Quit to mainmenu" });
            foreach (ButtonWidget button in mMenu.Buttons)
            {
                button.LabelColor = new Color(55, 55, 55);
                button.SelectionColor = new Color(103, 84, 15);
            }
            GameGui.RootWidget.AddChild(mMenu);
        }

        protected override void OnLeaving()
        {
            base.OnLeaving();

            //mMainScreen.Desktop.Children.Remove(mMenu);
            //mGui.RootWidget.RemoveChild(mMenu);
            mMenu.Destroy();


        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape) && mLastKeyboardState.IsKeyUp(Keys.Escape))
            {
                mStateManager.Pop();
                return;
            }

            base.Update(gameTime);
        }
    }
}
