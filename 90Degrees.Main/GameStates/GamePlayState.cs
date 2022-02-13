using Artemis;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Twengine.SubSystems.Raycast;
using XNAGameGui.Gui.Widgets;
using IUpdateable = Engine.GameStates.IUpdateable;

namespace raycaster.GameStates
{
    internal class GamePlayState : GameState, IUpdateable
    {
        private readonly InputHandler mFpsControl;
        private KeyboardState mLastKeyboardState;
        private LabelWidget mLoadingScreen;
        private Thread mLoadThread;
        private bool mSpawnFinished;
        private readonly GameStateManager mStateManager;
        private readonly EntityWorld mWorld;

        public GamePlayState(EntityWorld world, GameStateManager stateManager, InputHandler fpsControl)
        {
            mWorld = world;
            mStateManager = stateManager;
            mFpsControl = fpsControl;
            //mFpsControl.CenterMouse();
            mLastKeyboardState = Keyboard.GetState();
            mSpawnFinished = false;
        }

        public bool Enabled => true;

        public override void Update(GameTime gameTime)
        {
            mWorld.Update(gameTime.ElapsedGameTime.Ticks);
            //mWorld.LoopStart();
            //mWorld.Delta = ;
            //DebugDrawer.DrawString("spawnFinished: " + mSpawnFinished);
            //DebugDrawer.DrawString("RaycastGame.FinishedLoading: " + RaycastGame.FinishedLoading);
            if (RaycastGame.FinishedLoading && mSpawnFinished)
            {
                //mSystemManager.UpdateSynchronous(ExecutionType.Update);

                var keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.P) && mLastKeyboardState.IsKeyUp(Keys.P))
                    mStateManager.Push(new PauseState(mStateManager));

                if (keyboardState.IsKeyDown(Keys.Escape) && mLastKeyboardState.IsKeyUp(Keys.Escape))
                    mStateManager.Push(new EscapeMenuState(mStateManager));

                mLastKeyboardState = keyboardState;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            mFpsControl.CenterMouse();
        }

        protected override void OnEntered()
        {
            base.OnEntered();
            Debug.Print("Entering GamePlayState");
            RaycastGame.ShowLoadingScreen();

            RaycastGame.AudioManager.StartPlaylist(new List<int>
                {(int) SoundCue.GamePlayMusic01, (int) SoundCue.GamePlayMusic02, (int) SoundCue.GamePlayMusic03});

            Spawn();
        }

        private void Spawn()
        {
            Debug.Print("SpawnThread Started..");
            RaycastGame.SpawnThings();
            Debug.Print("Spawnining Player");
            RaycastGame.InitialSpawnPlayer();
            Debug.Print("Killing Loading Screen..");
            RaycastGame.DestroyLoadingScreen();
            Debug.Print("Trying to reset AudioManager..");

            //RaycastGame.MultiKillTimer
            Debug.Print("Trying to set finished Flag..");
            mSpawnFinished = true;
            Debug.Print("Spawning finished!");
            mFpsControl.CenterMouse();
        }


        protected override void OnLeaving()
        {
            base.OnLeaving();
            Debug.Print("Leaving GamePlayState");
            mSpawnFinished = false;
            RaycastGame.Reset();
            //RaycastGame.ResetTilemap(); FIX THIS, we mean to dispose of this, not load it again
            RaycastGame.AudioManager.StopPlaylist();
        }
    }
}