using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Artemis;
using Artemis.Manager;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Twengine;
using Twengine.Helper;
using Twengine.SubSystems.Raycast;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    class GamePlayState : GameState, Engine.GameStates.IUpdateable
    {
        private SystemManager mSystemManager;
        private EntityWorld mWorld;
        private GameStateManager mStateManager;
        private KeyboardState mLastKeyboardState;
        private FPSControlSystem mFpsControl;
        private SpriteBatch mSpriteBatch;
        private LabelWidget mLoadingScreen;
        private Thread mLoadThread;
        private bool mSpawnFinished;

        public bool Enabled => true;

        public GamePlayState(EntityWorld world, SystemManager systemManager, GameStateManager stateManager, FPSControlSystem fpsControl, SpriteBatch spriteBatch)
        {

            mSpriteBatch = spriteBatch;
            mWorld = world;
            mSystemManager = systemManager;
            mStateManager = stateManager;
            mFpsControl = fpsControl;
            //mFpsControl.CenterMouse();
            mLastKeyboardState = Keyboard.GetState();
            mSpawnFinished = false;
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
            
            ComponentTwengine.AudioManager.StartPlaylist(new List<int>() { (int)SoundCue.GamePlayMusic01, (int)SoundCue.GamePlayMusic02, (int)SoundCue.GamePlayMusic03 });

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

                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.P) && mLastKeyboardState.IsKeyUp(Keys.P))
                    mStateManager.Push(new PauseState(mStateManager));

                if (keyboardState.IsKeyDown(Keys.Escape) && mLastKeyboardState.IsKeyUp(Keys.Escape))
                    mStateManager.Push(new EscapeMenuState(mStateManager));

                mLastKeyboardState = keyboardState;
            }
        }


        protected override void OnLeaving()
        {
            base.OnLeaving();
            Debug.Print("Leaving GamePlayState");
            mSpawnFinished = false;
            RaycastGame.KillAllEntities();
            //RaycastGame.ResetTilemap(); FIX THIS, we mean to dispose of this, not load it again
            ComponentTwengine.AudioManager.StopPlaylist();
        }

    }
}
