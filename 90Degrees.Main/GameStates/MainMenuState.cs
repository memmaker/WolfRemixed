﻿using System.Collections.Generic;
using Artemis;
using Artemis.Manager;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TurnBasedCombat.GameStates;
using Twengine;
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
        private readonly FPSControlSystem mFPSControl;
        private readonly SpriteBatch mSpriteBatch;

        public MainMenuState(EntityWorld world, SystemManager systemManager, GameStateManager stateManager, FPSControlSystem fpsControl, SpriteBatch spriteBatch)
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
                    mGameStateManager.Push(new GamePlayState(mWorld, mSystemManager, mStateManager, mFPSControl, mSpriteBatch));
                    break;
                case "Options":
                    mGameStateManager.Push(new OptionsMenuState(mWorld, mSystemManager, mStateManager, mFPSControl, mSpriteBatch));
                    break;
            }
        }


        protected override void OnEntered()
        {
            base.OnEntered();
            float menuWidth = GameGui.Viewport.Width * 0.4f;
            mMenu = new MenuWindowWidget(new List<string>() { "Play", "Options", "Quit" }, (int) menuWidth, (int) (menuWidth - (2*20)));
            mMenu.Background = AssetManager.Default.LoadTexture("Menu/titlescreen_widescreen.png");
            mMenu.Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0));
            mMenu.DrawLabelBackground = true;
            mMenu.Buttons["Play"].IsSelected = true;
            foreach (ButtonWidget button in mMenu.Buttons.Values)
            {
                button.LabelColor = new Color(55, 55, 55);
                button.SelectionColor = new Color(103, 84, 15);
            }
            GameGui.RootWidget.AddChild(mMenu);
            ComponentTwengine.AudioManager.PlaySound("MenuMusic", true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            ComponentTwengine.AudioManager.PlaySound("MenuMusic", true);
        }

        protected override void OnPause()
        {
            base.OnPause();
            ComponentTwengine.AudioManager.StopSound("MenuMusic");
        }
        protected override void OnLeaving()
        {
            base.OnLeaving();
            
            //mMainScreen.Desktop.Children.Remove(mMenu);
            //mGui.RootWidget.RemoveChild(mMenu);
            mMenu.Destroy();
            ComponentTwengine.AudioManager.StopSound("MenuMusic");
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