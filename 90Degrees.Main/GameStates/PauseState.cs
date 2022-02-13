using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    class PauseState : GameState, Engine.GameStates.IUpdateable
    {

        private GameStateManager mStateManager;
        private KeyboardState mLastKeyboardState;
        private BaseWidget mPauseWindow;

        public bool Enabled => true;

        public PauseState(GameStateManager stateManager)
        {
            mStateManager = stateManager;
            mLastKeyboardState = Keyboard.GetState();
        }
        protected override void OnEntered()
        {
            mPauseWindow = RaycastGame.CreatePauseLabel();
        }
        protected override void OnLeaving()
        {
            mPauseWindow.Destroy();
        }
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.P) && mLastKeyboardState.IsKeyUp(Keys.P))
                mStateManager.Pop();
            mLastKeyboardState = keyboardState;
        }
    }
}
