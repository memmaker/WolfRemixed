using System;
using System.Collections.Generic;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using Twengine.Managers;
using XNAHelper;

namespace Twengine.SubSystems.Raycast
{
    public class RaycastMapRenderer : EntityProcessingSystem
    {
        private SpriteBatch mSpriteBatch;
        private GraphicsDevice mGraphicsDevice;
        private SpriteFont mHudFont;
        private AssetManager mContent;
        private int mScreenWidth;
        private int mScreenHeight;
        private Tilemap mTilemap;
        private Texture2D mPixel;
        private int mGridSizeInPixels;
        private int mThingSizeInPixels;
        private Raycaster mRaycaster;
        private Texture2D mArrow;
        private RenderTarget2D mMapDisplay;
        private Vector2 mOffset;
        public Entity Player { get; set; }
        public bool DrawMap { get; set; }

        public int GridSizeInPixels
        {
            get { return mGridSizeInPixels; }
            set 
            { 
                mGridSizeInPixels = (int) MathHelper.Clamp(value, 2, 50);
                mThingSizeInPixels = GridSizeInPixels / 2;
                int width = mMapSize * 2 * mGridSizeInPixels;
                int height = mMapSize * 2 * mGridSizeInPixels;
                if (width > 4096 || height > 4096) throw new ArgumentException("GridSizeInPixels too high.. Texture would be greater than 4096..");
                if (mMapDisplay != null)
                {
                    mMapDisplay.Dispose();
                    mMapDisplay = null;
                }
                mMapDisplay = new RenderTarget2D(mGraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
                mOffset = new Vector2(mScreenWidth-width,0);
            }
        }

        private bool mBackground;
        private bool mDrawLoS;
        private bool[,] mFogMap;
        private int mMapSize;

        public RaycastMapRenderer(SpriteBatch spriteBatch, AssetManager content, Tilemap map, int screenWidth, int screenHeight, SpriteFont hudFont, Raycaster raycaster)
            : base(typeof(RaycastSprite))
        {
            mBackground = false;
            mDrawLoS = false;
            mSpriteBatch = spriteBatch;
            mGraphicsDevice = spriteBatch.GraphicsDevice;
            mHudFont = hudFont;
            mContent = content;
            mScreenWidth = screenWidth;
            mScreenHeight = screenHeight;
            mTilemap = map;
            mRaycaster = raycaster;
            DrawMap = false;
            mMapSize = 5;
            GridSizeInPixels = 15;
        }

        public override void Initialize()
        {
            base.Initialize();
            mPixel = Primitives2D.CreateThePixel(mSpriteBatch);
            mArrow = mContent.LoadTexture("Icons/arrow.png");
        }

        protected override void ProcessEntities(Dictionary<int, Entity> entities)
        {
            if (!DrawMap) return;

            DrawMapDisplay(entities);
        }

        private void DrawMapDisplay(Dictionary<int, Entity> entities)
        {
            mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            mFogMap = mRaycaster.FogOfWarMap;
            Rectangle mapRect = GetMapRect();

            if (mBackground) DrawBackground();
            
            DrawThings(mapRect);
            //DrawPlayer();
            DrawWalls(mapRect);
            if (mDrawLoS) DrawLoS();
            mSpriteBatch.End();
        }


        private void DrawBackground()
        {
            Rectangle destRect = new Rectangle(0, 0, mTilemap.MapWidth * GridSizeInPixels,mTilemap.MapHeight*GridSizeInPixels);
            mSpriteBatch.Draw(mPixel,destRect,Color.CornflowerBlue);
        }

        private void DrawLoS()
        {
            List<List<Point>> andClearLastLineOfSightLines = mTilemap.GetLastLineOfSightLines();
            
            foreach (List<Point> lineOfSightLine in andClearLastLineOfSightLines)
            {
                foreach (Point point in lineOfSightLine)
                {
                    DrawCellOnMap(point.X,point.Y,Color.Turquoise);
                }
            }
        }


        private void DrawThings(Rectangle mapRect)
        {
            float xcounter = 0;
            float ycounter = 0;
            for (int y = mapRect.Top; y < mapRect.Bottom; y++)
            {
                for (int x = mapRect.Left; x < mapRect.Right; x++)
                {
                    List<Entity> entities = mTilemap.Entities[y,x];
                    foreach (Entity entity in entities)
                    {
                        if (entity.Group != "Hud")
                        {
                            Transform transform = entity.GetComponent<Transform>();
                            float xDiff = x - transform.Position.X;
                            float yDiff = y - transform.Position.Y;
                            DrawEntityAt(entity, new Vector2(xcounter - xDiff, ycounter - yDiff));
                        }
                    }
                    xcounter++;
                }
                ycounter++;
                xcounter = 0;
            }
        }


        private void DrawEntityAt(Entity entity, Vector2 pos)
        {
            Transform transform = entity.GetComponent<Transform>();
            Color drawColor = Color.Pink;
            switch (entity.Group)
            {
                case "Player":
                    DrawDirectedThingOnMap(pos, transform.Rotation, Color.Green);
                    return;
                case "Enemy":
                    DrawEnemy(entity, transform, pos);
                    return;
                case "Deco":
                    drawColor = Color.AliceBlue;
                    break;
                case "Pickup":
                    drawColor = Color.DarkBlue;
                    break;
                case "Door":
                    drawColor = Color.Tan;
                    Door door = entity.GetComponent<Door>();
                    DrawDoorOnMap(pos, door.Orientation, drawColor);
                    return;
            }
            DrawThingOnMap(pos, drawColor);
        }

        private void DrawEnemy(Entity entity, Transform transform, Vector2 pos)
        {
            Color drawColor = Color.DarkRed;
            MetaBehavior behavior = entity.GetComponent<MetaBehavior>();

            if (behavior != null)
            {
                string enemyState = behavior.ToString();
                if (enemyState.Contains("Shoot"))
                {
                    drawColor = Color.Tomato;
                }
                else if (enemyState.Contains("Chase"))
                {
                    drawColor = Color.CadetBlue;
                }
            }
            else
            {
                DrawThingOnMap(pos, drawColor);
                return;
            }
            DrawDirectedThingOnMap(pos, transform.Rotation, drawColor);
        }


        private void DrawWalls(Rectangle mapRect)
        {
            int[,] mapData = mTilemap.GetMapData();

            int xcounter = 0;
            int ycounter = 0;
            for (int y = mapRect.Top; y < mapRect.Bottom; y++)
            {
                for (int x = mapRect.Left; x < mapRect.Right; x++)
                {
                    if (!mFogMap[y, x])
                    {
                        DrawCellOnMap(xcounter, ycounter, Color.Black);
                    }
                    else if (mapData[y, x] > 0)
                    {
                        DrawCellOnMap(xcounter, ycounter, Color.Gray);
                    }
                    xcounter++;
                }
                ycounter++;
                xcounter = 0;
            }
            
        }

        private Rectangle GetMapRect()
        {
            
            Transform transform = Player.GetComponent<Transform>();
            Point currentPlayerCell = new Point((int) transform.Position.X, (int) transform.Position.Y);
            int startX = Math.Max(0, currentPlayerCell.X - mMapSize);
            int startY = Math.Max(0, currentPlayerCell.Y - mMapSize);

            int endX = Math.Min(mTilemap.MapWidth, currentPlayerCell.X + mMapSize);
            int endY = Math.Min(mTilemap.MapHeight, currentPlayerCell.Y + mMapSize);

            return new Rectangle(startX,startY,endX - startX, endY - startY);
        }

        private void DrawDirectedThingOnMap(Vector2 position, float rotationInRadians, Color drawColor)
        {
            int x = (int)(((position.X) * GridSizeInPixels) + mOffset.X);
            int y = (int)(((position.Y) * GridSizeInPixels) + mOffset.Y);
            Rectangle destRect = new Rectangle(x, y, (GridSizeInPixels), (GridSizeInPixels));
            mSpriteBatch.Draw(mArrow, destRect, null, drawColor, rotationInRadians, new Vector2(mArrow.Width / 2f, mArrow.Height / 2f), SpriteEffects.None, 0.4f);
        }

        private void DrawThingOnMap(Vector2 position, Color drawColor)
        {
            int x = (int)(((position.X) * GridSizeInPixels) - (mThingSizeInPixels / 2f) + mOffset.X);
            int y = (int)(((position.Y) * GridSizeInPixels) - (mThingSizeInPixels / 2f) + mOffset.Y);
            Rectangle destRect = new Rectangle(x, y, mThingSizeInPixels, mThingSizeInPixels);
            mSpriteBatch.Draw(mPixel, destRect, drawColor);
        }

        private void DrawDoorOnMap(Vector2 position, Orientation orientation, Color drawColor)
        {
            Rectangle destRect;
            int doorDepth = 4;
            if (orientation == Orientation.Horizontal)
            {
                int x = (int)((position.X * GridSizeInPixels) - (mGridSizeInPixels/2) + mOffset.X);
                int y = (int)((position.Y * GridSizeInPixels) - (doorDepth / 2) + mOffset.Y);
                destRect = new Rectangle(x, y, mGridSizeInPixels, doorDepth);
            }
            else // vertical
            {
                int x = (int)((position.X * GridSizeInPixels) - (doorDepth / 2) + mOffset.X);
                int y = (int)((position.Y * GridSizeInPixels) - (mGridSizeInPixels / 2) + mOffset.Y);
                destRect = new Rectangle(x, y, doorDepth, mGridSizeInPixels);
            }

            mSpriteBatch.Draw(mPixel, destRect, drawColor);
        }

        private void DrawCellOnMap(int x, int y, Color color)
        {
            int xDest = (int) ((x * GridSizeInPixels) + mOffset.X);
            int yDest = (int) ((y * GridSizeInPixels) + mOffset.Y);
            Rectangle destRect = new Rectangle(xDest, yDest, GridSizeInPixels, GridSizeInPixels);
            mSpriteBatch.Draw(mPixel, destRect, color);
        }


        public override void Process(Entity e)
        {
            throw new NotImplementedException("Because it is not used.");
        }

        public void ChangeMap(Tilemap tilemap)
        {
            mTilemap = tilemap;
            GridSizeInPixels = GridSizeInPixels;    // called for the side-effects..
        }
    }
}
