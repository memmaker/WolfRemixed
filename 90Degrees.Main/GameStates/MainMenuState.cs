using Artemis;
using Artemis.Manager;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MP3Player;
using TurnBasedCombat.GameStates;
using Twengine.Managers;
using Twengine.SubSystems.Raycast;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    public class MainMenuState : MenuGameState
    {
        private readonly EntityWorld mWorld;
        private readonly GameStateManager mStateManager;
        private readonly InputHandler mFPSControl;

        public MainMenuState(EntityWorld world, GameStateManager stateManager, InputHandler fpsControl)
            : base(stateManager)
        {
            mWorld = world;
            mStateManager = stateManager;
            mFPSControl = fpsControl;

            ButtonPressed += MainMenuState_ButtonPressed;

        }

        void MainMenuState_ButtonPressed(ButtonWidget instance)
        {
            switch (instance.Text)
            {
                case "Quit":
                    mGameStateManager.Pop();
                    break;
                case "Play":
                    mGameStateManager.Push(new GamePlayState(mWorld, mStateManager, mFPSControl));
                    break;
                case "Options":
                    mGameStateManager.Push(new OptionsMenuState(mStateManager));
                    break;
            }
        }


        protected override void OnEntered()
        {
            base.OnEntered();
            float menuWidth = GameGui.Viewport.Width * 0.4f;
            mMenu = new MenuWindowWidget( (int)menuWidth, (int)(menuWidth - (2 * 20)));
            mMenu.AddSimpleButtons(new List<string>() { "Play", "Options", "Quit" });
            mMenu.Background = AssetManager.Default.LoadTexture("Menu/mainmenu.png");
            mMenu.Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0));
            mMenu.DrawLabelBackground = true;
            foreach (ButtonWidget button in mMenu.Buttons)
            {
                button.LabelColor = Color.Transparent;
                button.SelectionColor = new Color(103, 84, 15);
            }
            GameGui.RootWidget.AddChild(mMenu);
            AudioPlayer.PlaySong((int)SoundCue.MenuMusic, true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            AudioPlayer.PlaySong((int)SoundCue.MenuMusic, true);
        }

        protected override void OnPause()
        {
            base.OnPause();
            AudioPlayer.StopSong();
        }
        protected override void OnLeaving()
        {
            base.OnLeaving();

            //mMainScreen.Desktop.Children.Remove(mMenu);
            //mGui.RootWidget.RemoveChild(mMenu);
            AudioPlayer.StopSong();
        }

        protected override void KeyPressed(Keys key)
        {
            base.KeyPressed(key);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
