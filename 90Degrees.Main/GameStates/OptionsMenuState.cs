using System;
using System.Collections.Generic;
using Artemis;
using Artemis.Manager;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TurnBasedCombat.GameStates;
using Twengine;
using Twengine.Managers;
using Twengine.SubSystems.Raycast;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    public class OptionsMenuState : MenuGameState
    {
        private readonly EntityWorld mWorld;
        private readonly SystemManager mSystemManager;
        private readonly GameStateManager mStateManager;
        private readonly FPSControlSystem mFPSControl;
        private readonly SpriteBatch mSpriteBatch;
        private Point[] mResolutions;
        private ButtonWidget mResolutionButton;
        private ButtonWidget mFullscreenButton;

        public OptionsMenuState(EntityWorld world, SystemManager systemManager, GameStateManager stateManager, FPSControlSystem fpsControl, SpriteBatch spriteBatch)
            : base(stateManager)
        {
            mWorld = world;
            mSystemManager = systemManager;
            mStateManager = stateManager;
            mFPSControl = fpsControl;
            mSpriteBatch = spriteBatch;

            mResolutions = new Point[5];
            mResolutions[0] = new Point(320, 180);
            mResolutions[1] = new Point(640, 360);
            mResolutions[2] = new Point(960, 540);
            mResolutions[3] = new Point(1280, 720);
            mResolutions[4] = new Point(1920, 1080);

            ButtonPressed += new ButtonEventHandler(MainMenuState_ButtonPressed);

        }

        private bool IntToBool(int number)
        {
            if (number == 0) return false;
            return true;
        }
        void MainMenuState_ButtonPressed(ButtonWidget instance)
        {
            switch (instance.Text)
            {
                case "Resolution":
                    break;
                case "Fullscreen":
                    break;
                case "Apply":
                    RaycastGame.ChangeResolution(mResolutions[mResolutionButton.Counter].X, mResolutions[mResolutionButton.Counter].Y, IntToBool(mFullscreenButton.Counter));
                    break;
                case "Back":
                    mGameStateManager.Pop();
                    break;
            }
        }


        protected override void OnEntered()
        {
            base.OnEntered();
            float menuWidth = GameGui.Viewport.Width * 0.4f;
            mMenu = new MenuWindowWidget(new List<string>() { "Resolution", "Fullscreen", "Apply", "Back" }, (int)menuWidth, (int)(menuWidth - (2 * 20)))
            {
                Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0)),
                Background = AssetManager.Default.LoadTexture("Menu/options_widescreen.png"),
                DrawLabelBackground = true
            };
            foreach (ButtonWidget button in mMenu.Buttons.Values)
            {
                button.LabelColor = new Color(55, 55, 55);
                button.SelectionColor = new Color(103, 84, 15);
            }

            mResolutionButton = mMenu.Buttons["Resolution"];
            mResolutionButton.AppendCounter = true;
            mResolutionButton.SetCounterBounds(0,4);
            mResolutionButton.CounterToStringFunction = ResolutionChoices;

            mFullscreenButton = mMenu.Buttons["Fullscreen"];
            mFullscreenButton.AppendCounter = true;
            mFullscreenButton.SetCounterBounds(0, 1);
            mFullscreenButton.CounterToStringFunction = FullscreenChoices;

            ReadConfigSettings();
            GameGui.RootWidget.AddChild(mMenu);
        }

        private void ReadConfigSettings()
        {
            float mouseSensitivity = float.Parse(RaycastGame.Config.AppSettings.Settings["MouseSensitivity"].Value);
            bool fullscreen = bool.Parse(RaycastGame.Config.AppSettings.Settings["Fullscreen"].Value);
            bool lowResRaytracing = bool.Parse(RaycastGame.Config.AppSettings.Settings["LowResRaycasting"].Value);
            int width = int.Parse(RaycastGame.Config.AppSettings.Settings["ScreenWidth"].Value);
            int height = int.Parse(RaycastGame.Config.AppSettings.Settings["ScreenHeight"].Value);
            bool secretWallsVisible = bool.Parse(RaycastGame.Config.AppSettings.Settings["SecretWallsVisible"].Value);

            mFullscreenButton.Counter = fullscreen ? 1 : 0;
            mResolutionButton.Counter = FindResolutionIndex(width, height);
        }

        private int FindResolutionIndex(int width, int height)
        {
            for (int i = 0; i < 5; i++)
            {
                if (mResolutions[i].X == width && mResolutions[i].Y == height)
                    return i;
            }
            return 0;
        }

        private string FullscreenChoices(int counter)
        {
            switch (counter)
            {
                case 0:
                    return ": Off";
                case 1:
                    return ": On";
            }
            return "ERROR!";
        }

        private string ResolutionChoices(int counter)
        {
            return ": " + mResolutions[counter].X + "x" + mResolutions[counter].Y;
        }



        protected override void OnLeaving()
        {
            base.OnLeaving();
            
            //mMainScreen.Desktop.Children.Remove(mMenu);
            //mGui.RootWidget.RemoveChild(mMenu);
            mMenu.Destroy();

            RaycastGame.SaveConfiguration();
        }

    }
}
