using Artemis;
using Artemis.Manager;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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
        private readonly SystemManager mSystemManager;
        private readonly GameStateManager mStateManager;
        private readonly InputHandler mFPSControl;
        private readonly SpriteBatch mSpriteBatch;

        public MainMenuState(EntityWorld world, SystemManager systemManager, GameStateManager stateManager, InputHandler fpsControl, SpriteBatch spriteBatch)
            : base(stateManager)
        {
            mWorld = world;
            mSystemManager = systemManager;
            mStateManager = stateManager;
            mFPSControl = fpsControl;
            mSpriteBatch = spriteBatch;
            ButtonPressed += new ButtonEventHandler(MainMenuState_ButtonPressed);
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
            mMenu = new MenuWindowWidget(new List<string>() { "Play", "Options", "Quit" }, (int)menuWidth, (int)(menuWidth - (2 * 20)));
            mMenu.Background = AssetManager.Default.LoadTexture("Menu/titlescreen_widescreen.png");
            mMenu.Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0));
            mMenu.DrawLabelBackground = true;
            foreach (ButtonWidget button in mMenu.Buttons)
            {
                button.LabelColor = Color.Transparent;
                button.SelectionColor = new Color(103, 84, 15);
            }
            GameGui.RootWidget.AddChild(mMenu);
            RaycastGame.AudioManager.PlaySong((int)SoundCue.MenuMusic, true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RaycastGame.AudioManager.PlaySong((int)SoundCue.MenuMusic, true);
        }

        protected override void OnPause()
        {
            base.OnPause();
            RaycastGame.AudioManager.StopSong();
        }
        protected override void OnLeaving()
        {
            base.OnLeaving();

            //mMainScreen.Desktop.Children.Remove(mMenu);
            //mGui.RootWidget.RemoveChild(mMenu);
            mMenu.Destroy();
            RaycastGame.AudioManager.StopSong();
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
