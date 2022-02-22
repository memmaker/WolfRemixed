using Engine.GameStates;
using System.Collections.Generic;
using System.Drawing;
using Degrees.Main.Engine.Managers;
using TurnBasedCombat.GameStates;
using Twengine.Managers;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace raycaster.GameStates
{
    public class OptionsMenuState : MenuGameState
    {
        private const string KeyboardOnlyLabel = "Keyboard Only";
        private const string KeyboardMouseLabel = "Keyboard + Mouse";
        private const string FullscreenLabel = "Fullscreen";
        private const string WindowedLabel = "Windowed";
        private const string MouseSensitivityLabel = "Mouse Sensitivity: ";
        private const string MusicVolLabel = "Music Volume: ";
        private const string SfxVolLabel = "SFX Volume: ";

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
            CounterIncreased += UpdateCounter;
            CounterDecreased += UpdateCounter;

        }
        
        private void UpdateCounter(ButtonWidget instance)
        {
            switch (instance.Text)
            {
                case MusicVolLabel:
                    Settings.MusicVolume = instance.Counter * 10;
                    break;
                case SfxVolLabel:
                    Settings.SfxVolume = instance.Counter * 10;
                    break;
                case MouseSensitivityLabel:
                    Settings.MouseSensitivity = instance.Counter / 10.0f;
                    break;
            }
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
                case FullscreenLabel: // switch to windows
                    Settings.Fullscreen = false;
                    instance.Text = WindowedLabel;
                    break;
                case WindowedLabel: // switch to fullscreen
                    instance.Text = FullscreenLabel;
                    Settings.Fullscreen = true;
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
            string fullscreen = Settings.Fullscreen ? FullscreenLabel : WindowedLabel;
            
            mMenu = new MenuWindowWidget((int)menuWidth, (int)(menuWidth - (2 * 20)))
            {
                Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0)),
                Background = AssetManager.Default.LoadTexture("Menu/options_widescreen.png"),
                DrawLabelBackground = true,
                MenuButtonBeginFraction = 0.3f
            };
            mMenu.AddButton(new ButtonWidget() { Text = controls });
            mMenu.AddButton(new ButtonWidget() { Text = fullscreen });
            mMenu.AddButton(new ButtonWidget()
            {
                Text = MusicVolLabel, 
                AppendCounter = true,
                CounterMin = 0, 
                CounterMax = 10,
                Counter = Settings.MusicVolume / 10
            });
            mMenu.AddButton(new ButtonWidget()
            {
                Text = SfxVolLabel,
                AppendCounter = true,
                CounterMin = 0,
                CounterMax = 10,
                Counter = Settings.SfxVolume / 10
            });
            mMenu.AddButton(new ButtonWidget()
            {
                Text = MouseSensitivityLabel,
                AppendCounter = true,
                CounterMin = 1,
                CounterMax = 20,
                Counter = (int) (Settings.MouseSensitivity * 10.0f)
            });
            mMenu.AddButton(new ButtonWidget() { Text = "Back" });

            //new List<string>() { controls, "Back" }
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
