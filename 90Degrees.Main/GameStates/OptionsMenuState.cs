using Engine.GameStates;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TurnBasedCombat.GameStates;
using Twengine.Managers;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    public class OptionsMenuState : MenuGameState
    {
        private Point[] mResolutions;

        public OptionsMenuState(GameStateManager stateManager)
            : base(stateManager)
        {

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
                    //RaycastGame.ChangeResolution(mResolutions[mResolutionButton.Counter].X, mResolutions[mResolutionButton.Counter].Y, IntToBool(mFullscreenButton.Counter));
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
            foreach (ButtonWidget button in mMenu.Buttons)
            {
                button.LabelColor = new Color(55, 55, 55);
                button.SelectionColor = new Color(103, 84, 15);
            }


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
