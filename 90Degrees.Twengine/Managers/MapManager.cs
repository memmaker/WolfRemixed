using System;
using System.Collections.Generic;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using XNAHelper;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = xTile.Dimensions.Rectangle;
using Size = xTile.Dimensions.Size;

namespace Twengine.Managers
{
    public class SpawnEntityEventArgs : EventArgs
    {
        public Point Position { get; set; }
        public int TileIndex { get; set; }
        public Texture2D Texture { get; set; }
        public string LayerId { get; set; }
        public IPropertyCollection Properties { get; set; }
    }
    public class MapManager
    {
        public event EventHandler<SpawnEntityEventArgs> CreateWalls;
        public event EventHandler<SpawnEntityEventArgs> CreateItems;
        public event EventHandler<SpawnEntityEventArgs> CreateEnemies;
        public event EventHandler<SpawnEntityEventArgs> CreateMetaInfo;
        
        protected Map mMap;
        private IDisplayDevice mMapDisplayDevice;
        private Rectangle mMapViewport;
        public Rectangle MapViewport
        {
            get { return mMapViewport; }
            set { mMapViewport = value; }
        }

        private Vector2 mViewportCenter;
        private ContentManager mContentManager;
        private GraphicsDevice mGraphicsDevice;
        private EntityWorld mWorld;
        private Dictionary<string,Texture2D> mTextures;

        public MapManager(EntityWorld world, ContentManager content, GraphicsDevice graphics)
        {
            mWorld = world;
            mContentManager = content;
            mMapDisplayDevice = new XnaDisplayDevice(content, graphics);
            mGraphicsDevice = graphics;
            MapViewport = new Rectangle(new Size(graphics.Viewport.Width, graphics.Viewport.Height));
            mTextures = new Dictionary<string, Texture2D>();
        }


        public Texture2D GetTileSheet(string tileSheetId)
        {
            return mTextures[tileSheetId];
        }

        public void LoadMap(string map)
        {
            
            mMap = mContentManager.Load<Map>(map);
            
            mMap.LoadTileSheets(mMapDisplayDevice);
            foreach (TileSheet tileSheet in mMap.TileSheets)
            {
                mTextures[tileSheet.Id] = mContentManager.Load<Texture2D>(tileSheet.ImageSource);
            }
            
            Layer wallLayer = mMap.GetLayer("Walls");
            //CollisionDetector.RegisterCollisionLayer(MapViewport, collisionLayer);
            if (CreateWalls != null)
            {
                ProcessAllTiles(wallLayer, CreateWalls);
            }
            Layer itemLayer = mMap.GetLayer("Items");

            if (CreateItems != null)
            {
                ProcessAllTiles(itemLayer, CreateItems);
            }

            Layer metaLayer = mMap.GetLayer("MetaInfo");
            if (CreateMetaInfo != null)
            {
                ProcessAllTiles(metaLayer, CreateMetaInfo);
            }

            Layer enemyLayer = mMap.GetLayer("Enemies");
            if (CreateEnemies != null)
            {
                ProcessAllTiles(enemyLayer, CreateEnemies);
            }
        }

        private void ProcessAllTiles(Layer layer, EventHandler<SpawnEntityEventArgs> function)
        {
            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerWidth; y++)
                {
                    Tile tile = layer.Tiles[x, y];
                    if (tile == null) continue;
                    
                    //Rectangle tileDisplayRectangle = layer.GetTileDisplayRectangle(MapViewport, new Location(x, y));
                    //Vector2 pos = new Vector2(tileDisplayRectangle.X + (tileDisplayRectangle.Width/2), tileDisplayRectangle.Y + (tileDisplayRectangle.Height/2));
                    //Vector2 pos = new Vector2(x+0.5f,y+0.5f);
                    Point pos = new Point(x,y);
                    Texture2D texture = mTextures[tile.TileSheet.Id];

                    function(this, new SpawnEntityEventArgs() { Position = pos, TileIndex = tile.TileIndex, Texture = texture, LayerId = tile.Layer.Id, Properties = tile.Properties });
                }
            }
        }

        public void Update(GameTime gameTime)
        {

            if (mMap != null)
            {
                ScrollMap(gameTime);
                mMap.Update(gameTime.ElapsedGameTime.Milliseconds);
            }

        }
        public void Draw()
        {
            if (mMap != null) mMap.Draw(mMapDisplayDevice, MapViewport);
        }

        protected void MoveViewport(GameTime gameTime)
        {
            int viewportSpeed = 3;
            Vector2 currentPos = new Vector2(MapViewport.X, MapViewport.Y);
            Vector2 targetPos = mViewportCenter - new Vector2(mGraphicsDevice.Viewport.Width / 2, mGraphicsDevice.Viewport.Height / 2);
            Vector2 dirVec = targetPos - currentPos;

            mMapViewport.X += (int)(dirVec.X * viewportSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            mMapViewport.Y += (int)(dirVec.Y * viewportSpeed * gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void ScrollMap(GameTime gameTime)
        {
            if (mWorld.GroupManager.GetEntities("Player").Size == 0) return;
            Microsoft.Xna.Framework.Rectangle playerRect = FindPlayerRect();

            mViewportCenter = playerRect.Center.ToVector2();

            MoveViewport(gameTime);
        }
        private Microsoft.Xna.Framework.Rectangle FindPlayerRect()
        {

            Vector2 topLeft = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 bottomRight = new Vector2(float.MinValue, float.MinValue);
            foreach (Entity playerEntity in mWorld.GroupManager.GetEntities("Player"))
            {
                Transform playerTransform = playerEntity.GetComponent<Transform>();
                if (playerTransform.Position.X < topLeft.X)
                    topLeft.X = playerTransform.Position.X;
                if (playerTransform.Position.Y < topLeft.Y)
                    topLeft.Y = playerTransform.Position.Y;
                if (playerTransform.Position.X > bottomRight.X)
                    bottomRight.X = playerTransform.Position.X;
                if (playerTransform.Position.Y > bottomRight.Y)
                    bottomRight.Y = playerTransform.Position.Y;
            }
            float width = bottomRight.X - topLeft.X;
            float height = bottomRight.Y - topLeft.Y;
            return new Microsoft.Xna.Framework.Rectangle((int)topLeft.X, (int)topLeft.Y, (int)width, (int)height);
        }

        public int WorldWidth { get { return mMap.DisplayWidth; } }
        public int WorldHeight { get { return mMap.DisplayHeight; } }

        public void ChangeResolution(int width, int height)
        {
            mMapViewport.Size = new Size(width,height);
            mMapViewport.Location = new Location(0,0);
        }
    }
}
