using System;
using System.Collections.Generic;
using System.Text;
using Artemis;
using Artemis.Manager;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MP3Player;
using Twengine.Helper;
using Twengine.Managers;
using Twengine.SubSystems;
using XNAGameGui.Gui;
using XNAHelper;


namespace Twengine
{
    public delegate void DefaultCallback();
    public abstract class ComponentTwengine : Game
    {
        protected static EntityWorld sWorld;
        protected SpriteBatch mSpriteBatch;
        protected static GraphicsDeviceManager Graphics;
        protected Artemis.Manager.SystemManager mSystemManager;
        
        protected static MapManager mMapManager;
        protected bool mXTileEnabled;
        protected static int sScreenWidth;
        protected static int sScreenHeight;
        public static event EventHandler<EventArgs> TicEvent;
        private int mTics;
        protected static GameStateManager mGameStateManager;
        public static AudioPlayer AudioManager { get; set; }
        protected static GameGui sGui;
        private MouseState mLastmouseState;
        protected bool mRunUpdateInEngine;

        public ComponentTwengine(bool useXtile, bool runUpdateInEngine)
        {
            mRunUpdateInEngine = runUpdateInEngine;
            Graphics = new GraphicsDeviceManager(this);
            sWorld = new EntityWorld();
            Content.RootDirectory = "Content";
            mXTileEnabled = useXtile;
            mTics = 0;
            sGui = new GameGui();
            mLastmouseState = Mouse.GetState();
            mGameStateManager = new GameStateManager(Services);
            AudioManager = new AudioPlayer();
            Components.Add(mGameStateManager);
        }

        public static void ChangeResolution(int screenWidth, int screenHeight, bool fullscreen)
        {
            sScreenWidth = screenWidth;
            sScreenHeight = screenHeight;
            Graphics.PreferredBackBufferWidth = sScreenWidth;
            Graphics.PreferredBackBufferHeight = sScreenHeight;
            Graphics.IsFullScreen = fullscreen;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Graphics.ApplyChanges();
            if (Graphics.GraphicsDevice != null) GameGui.Viewport = Graphics.GraphicsDevice.Viewport;

        }

        protected override void LoadContent()
        {
            base.LoadContent();
            mSpriteBatch = new SpriteBatch(GraphicsDevice);
            
            AssetManager.Default.Init(Content, GraphicsDevice);
            
            if (mXTileEnabled) mMapManager = new MapManager(Content);

            SpriteFont spriteFont = Content.Load<SpriteFont>("Fonts/DefaultFont");
            DebugDrawer.Init(mSpriteBatch, spriteFont);

            sGui.LoadContent(Content, GraphicsDevice, spriteFont, spriteFont);
            GameGui.Viewport = GraphicsDevice.Viewport;
            

            mSystemManager = sWorld.SystemManager;

            LoadGame();


            mSystemManager.SetSystem(new BehaviorSystem(), GameLoopType.Update);
            mSystemManager.SetSystem(new PhysicsSystem(), GameLoopType.Update);
            mSystemManager.SetSystem(new ExpirationSystem(), GameLoopType.Update);
            
            AddSubSystems();

            sWorld.InitializeAll();

            PostInit();
        }

        public static string GetEntityString(IEnumerable<Entity> entitiesOnCell)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Entity entity in entitiesOnCell)
            {
                sb.Append(entity.Group + ", ");
            }
            return sb.ToString();
        }

        protected abstract void AddSubSystems();

        protected abstract void LoadGame();
        protected abstract void PostInit();

        protected override void UnloadContent()
        {
            base.UnloadContent();
            sGui.UnloadContent();
            Shutdown();
        }

        protected abstract void Shutdown();

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MouseState mouseState = Mouse.GetState();

            //sGui.MouseMoved(new Point(mouseState.X, mouseState.Y));

            mLastmouseState = mouseState;

            if (mRunUpdateInEngine)
            {
                // mSystemManager.UpdateSynchronous(ExecutionType.Update);
                
                sWorld.Update();
            }

            AudioManager.Update();

            if (mGameStateManager.ActiveState == null)
                this.Exit();

            mGameStateManager.Update(gameTime);

            OnTic();
        }

        private void OnTic()
        {
            if (TicEvent != null)
            {
                mTics++;
                if (mTics == 2)
                    TicEvent(this, new EventArgs());
            }
            else
            {
                mTics = 0;
            }
        }
    }
}
