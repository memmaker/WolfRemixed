using System;
using System.Collections.Generic;
using System.Text;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MP3Player;
using Nuclex.Game.States;
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
        protected SystemManager mSystemManager;
        
        protected static MapManager mMapManager;
        protected bool mXTileEnabled;
        protected static int sScreenWidth;
        protected static int sScreenHeight;
        public static event EventHandler<EventArgs> TicEvent;
        private int mTics;
        protected static GameStateManager mGameStateManager;
        public static AudioPlayer AudioManager { get; set; }
        protected static GameGui sGui;
        private bool mDrawTileMap;
        private MouseState mLastmouseState;
        protected bool mRunUpdateInEngine;

        public ComponentTwengine(bool useXtile, bool runUpdateInEngine)
        {
            mRunUpdateInEngine = runUpdateInEngine;
            Graphics = new GraphicsDeviceManager(this);
            sWorld = new EntityWorld();
            Content.RootDirectory = "Content";
            mXTileEnabled = useXtile;
            mDrawTileMap = false;
            mTics = 0;
            sGui = new GameGui();
            mLastmouseState = Mouse.GetState();
            mGameStateManager = new GameStateManager(Services);
            AudioManager = new AudioPlayer();
            Components.Add(mGameStateManager);
        }

        public virtual void ChangeResolution(int screenWidth, int screenHeight, bool fullscreen)
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
            
            if (mXTileEnabled) mMapManager = new MapManager(sWorld, Content, GraphicsDevice);

            SpriteFont spriteFont = Content.Load<SpriteFont>("Fonts/DefaultFont");
            DebugDrawer.Init(mSpriteBatch, spriteFont);

            sGui.LoadContent(Content, GraphicsDevice, spriteFont, spriteFont);
            GameGui.Viewport = GraphicsDevice.Viewport;
            

            mSystemManager = sWorld.SystemManager;

            LoadGame();


            mSystemManager.SetSystem(new BehaviorSystem(), ExecutionType.Update);
            mSystemManager.SetSystem(new PhysicsSystem(), ExecutionType.Update);
            mSystemManager.SetSystem(new ExpirationSystem(), ExecutionType.Update);
            
            

            
            

            AddSubSystems();

            
            
            mSystemManager.InitializeAll();

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


            sGui.MouseMoved(new Point(mouseState.X, mouseState.Y));

            mLastmouseState = mouseState;
            
            if (mRunUpdateInEngine) mSystemManager.UpdateSynchronous(ExecutionType.Update);

            AudioManager.Update();

            if (mGameStateManager.ActiveState == null)
                this.Exit();

            if (mDrawTileMap) mMapManager.Update(gameTime);

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
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (mDrawTileMap) mMapManager.Draw();
            
            mSystemManager.UpdateSynchronous(ExecutionType.Draw);

            sGui.Draw(mSpriteBatch);

            DebugDrawer.Draw();

            base.Draw(gameTime);
        }
    }
}
