using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    // Neutral title screen: game name in the game's own font on a procedural brick-wall background.
    internal class TitleState : GameState
    {
        private readonly GameStateManager mStateManager;
        private readonly System.Func<IGameState> mNext;
        private LabelWidget mBackground, mTitle, mPressKey;
        private bool mWaitForRelease = true;
        private double mBlink;

        public TitleState(GameStateManager stateManager, System.Func<IGameState> next)
        {
            mStateManager = stateManager;
            mNext = next;
        }

        private static Texture2D CreateBackground(GraphicsDevice device)
        {
            const int w = 320, h = 180;
            var data = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int row = y / 12;
                    bool mortar = y % 12 == 0 || (x + (row % 2) * 12) % 24 == 0;
                    float shade = 0.25f + 0.75f * (float)y / h;   // fade to black towards the top
                    data[y * w + x] = mortar ? new Color(8, 8, 12) : new Color((int)(40 * shade), (int)(44 * shade), (int)(70 * shade));
                }
            }
            var tex = new Texture2D(device, w, h);
            tex.SetData(data);
            return tex;
        }

        protected override void OnEntered()
        {
            mBackground = new LabelWidget
            {
                Background = CreateBackground(GameGui.WhiteRectangle.GraphicsDevice),
                DrawLabelBackground = true,
                Bounds = new UniRectangle(0, 0, new UniScalar(1, 0), new UniScalar(1, 0))
            };
            mTitle = new LabelWidget("WOLF REMIXED")
            {
                TextColor = Color.Gold,
                Bounds = new UniRectangle(new UniScalar(0, 0), new UniScalar(0.3f, 0), new UniScalar(1, 0), new UniScalar(0.2f, 0))
            };
            mPressKey = new LabelWidget("press any key")
            {
                Font = GameFont.LongTexts,
                TextColor = Color.WhiteSmoke,
                Bounds = new UniRectangle(new UniScalar(0, 0), new UniScalar(0.7f, 0), new UniScalar(1, 0), new UniScalar(0.1f, 0))
            };
            GameGui.RootWidget.AddChild(mBackground);
            GameGui.RootWidget.AddChild(mTitle);
            GameGui.RootWidget.AddChild(mPressKey);
        }

        protected override void OnLeaving()
        {
            mBackground.Destroy();
            mTitle.Destroy();
            mPressKey.Destroy();
        }

        public override void Update(GameTime gameTime)
        {
            mBlink += gameTime.ElapsedGameTime.TotalSeconds;
            mPressKey.IsVisible = mBlink % 1.0 < 0.6;

            bool pressed = Keyboard.GetState().GetPressedKeys().Length > 0 || Mouse.GetState().LeftButton == ButtonState.Pressed;
            if (mWaitForRelease) { mWaitForRelease = pressed; return; }
            if (pressed) mStateManager.Switch(mNext());
        }
    }
}
