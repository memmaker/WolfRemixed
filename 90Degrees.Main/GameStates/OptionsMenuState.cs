using Engine.GameStates;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Degrees.Main.Engine.Managers;
using TurnBasedCombat.GameStates;
using Twengine.Managers;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    public class OptionsMenuState : MenuGameState
    {
        private const string KeyboardOnlyLabel = "Keyboard Only";
        private const string KeyboardMouseLabel = "Keyboard + Mouse";
        private const string FullscreenLabel = "Fullscreen";
        private const string WindowedLabel = "Windowed";
        private const string MouseSensitivityLabel = "Mouse Sensitivity";
        private const string MusicVolLabel = "Music Volume";
        private const string SfxVolLabel = "SFX Volume";

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

            ButtonPressed += OnMenuButtonPressed;

        }

        private bool IntToBool(int number)
        {
            if (number == 0) return false;
            return true;
        }
        void OnMenuButtonPressed(ButtonWidget instance)
        {
            switch (instance.Text)
            {
                case KeyboardOnlyLabel: // switch to keyboard & mouse
                    Settings.KeyboardOnly = false;
                    instance.Text = KeyboardMouseLabel;
                    break;
                case KeyboardMouseLabel:
                    Settings.KeyboardOnly = true;
                    instance.Text = KeyboardOnlyLabel;
                    break;
                case "Back":
                    Settings.SaveConfiguration();
                    mGameStateManager.Pop();
                    break;
            }
        }


        protected override void OnEntered()
        {
            base.OnEntered();
            float menuWidth = GameGui.Viewport.Width * 0.6f;
            string controls = Settings.KeyboardOnly ? KeyboardOnlyLabel : KeyboardMouseLabel;
            
            mMenu = new MenuWindowWidget(new List<string>() { controls, "Back" }, (int)menuWidth, (int)(menuWidth - (2 * 20)))
            {
                Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0)),
                Background = AssetManager.Default.LoadTexture("Menu/options_widescreen.png"),
                DrawLabelBackground = true
            };
            foreach (ButtonWidget button in mMenu.Buttons)
            {
                button.LabelColor = Color.Transparent;
                button.SelectionColor = new Color(103, 84, 15);
            }


            GameGui.RootWidget.AddChild(mMenu);
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

            Settings.SaveConfiguration();
        }

    }
}
