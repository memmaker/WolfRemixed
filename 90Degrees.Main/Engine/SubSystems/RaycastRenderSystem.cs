using Artemis;
using Artemis.System;
using IndependentResolutionRendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using raycaster;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Managers;
using XNAHelper;

namespace Twengine.SubSystems.Raycast
{
    public class RaycastRenderSystem : EntityProcessingSystem
    {
        private SpriteBatch mSpriteBatch;


        private Rectangle mCeiling;
        private Rectangle mFloor;

        private int mRaycasterResolutionX = Const.InternalRenderResolutionWidth;
        private int mRaycasterResolutionY = Const.InternalRenderResolutionHeight;
        private Color[] mBackgroundData;
        private BasicRaycastHitInfo[] mWallInfos;
        private List<Entity> mVisibleEntities;

        private Texture2D mPixel;
        private RenderTarget2D mFloorTexture;
        private RenderTarget2D mCeilingTexture;
        private RenderTarget2D mThreeDeeView;

        private SpriteSheet mWallTextures;

        public bool SecretWallsVisible { get; set; }

        private int mTextureHeight;

        private Rectangle mTextureSegmentRect;
        private Rectangle mDestinationOnScreenRect;

        private GraphicsDevice mGraphicsDevice;
        private Rectangle mThreeDeeViewRect;
        private int mScreenWidth;
        private int mScreenHeight;
        private float mScreenScale;
        private Color mFlashColor;
        private float mFlashIntensity;
        private int mLowResResolutionWidth;
        private int fov;
        private AssetManager mContent;
        private Rectangle mUpperScreen = new Rectangle(0,0,640,180);
        private Rectangle mLowerScreen = new Rectangle(0, 180, 640, 180);

        public RaycastRenderSystem(SpriteBatch spriteBatch, AssetManager content, Tilemap map)
            : base(Aspect.Empty())
        {

            mSpriteBatch = spriteBatch;
            mGraphicsDevice = spriteBatch.GraphicsDevice;
            mFlashColor = Color.Red;
            mFlashIntensity = 0;

            // raycaster
            mContent = content;
            

            //mStatusbarHeight = (int) (screenHeight * 0.07f);
            mTextureHeight = 64;
            mScreenScale = 1f;
            fov = 66;//75;

            Tilemap = map;
            mWallTextures = Tilemap.WallTextures;


            SecretWallsVisible = true;

            Raycaster = new Raycaster(Tilemap, mRaycasterResolutionX, mRaycasterResolutionY, fov, Tilemap.PlayerSpawn, Tilemap.PlayerViewDirection);

            SetResolutionDependentValues(Const.InternalRenderResolutionWidth, Const.InternalRenderResolutionHeight);

            Debug.Assert(mCeiling.Height + mFloor.Height == mRaycasterResolutionY, "Floor plus Ceiling are not using whole screenheight..");
        }
        
        private void SetResolutionDependentValues(int screenWidth, int screenHeight)
        {
            mScreenWidth = screenWidth;
            mScreenHeight = screenHeight;
            mThreeDeeViewRect = new Rectangle(0, 0, screenWidth, screenHeight);
            SetResolution(screenWidth);
        }
        private void SetResolution(int raycastWidth)
        {
            mLowResResolutionWidth = raycastWidth;
            mScreenScale = mScreenWidth / (float)mLowResResolutionWidth;
            float displayRatio = mScreenWidth / (float)mScreenHeight;
            mRaycasterResolutionX = mLowResResolutionWidth;
            mRaycasterResolutionY = (int)(Math.Ceiling(mLowResResolutionWidth / displayRatio));

            Raycaster.ViewportChanged(mRaycasterResolutionX, mRaycasterResolutionY);
            mCeiling = new Rectangle(0, 0, mRaycasterResolutionX, mRaycasterResolutionY / 2);
            mFloor = new Rectangle(0, mCeiling.Bottom, mRaycasterResolutionX, mRaycasterResolutionY - mCeiling.Height);
            LoadContent(mContent, mSpriteBatch.GraphicsDevice);
        }

        public Raycaster Raycaster { get; private set; }

        public Tilemap Tilemap { get; private set; }
        
        private void LoadContent(AssetManager content, GraphicsDevice graphicsDevice)
        {
            mPixel = Primitives2D.CreateThePixel(mSpriteBatch);

            mDestinationOnScreenRect = new Rectangle();
            mTextureSegmentRect = new Rectangle();

            mFloorTexture = new RenderTarget2D(graphicsDevice, mFloor.Width, mFloor.Height, false, SurfaceFormat.Color, DepthFormat.None);
            mCeilingTexture = new RenderTarget2D(graphicsDevice, mCeiling.Width, mCeiling.Height, false, SurfaceFormat.Color, DepthFormat.None);
            mThreeDeeView = new RenderTarget2D(graphicsDevice, mRaycasterResolutionX, mRaycasterResolutionY, false, SurfaceFormat.Color, DepthFormat.None);

            CreateCeiling();
            CreateFloor();
        }

        public void ChangeMap(Tilemap tilemap)
        {
            Tilemap = tilemap;
            mWallTextures = Tilemap.WallTextures;
        }

        public override void LoadContent()
        {
            this.entityWorld.EntityManager.RemovedEntityEvent += OnRemoved;
            base.LoadContent();
        }
        public override void OnAdded(Entity e)
        {
            Tilemap.AddEntity(e);
        }

        public override void OnRemoved(Entity e)
        {
            Tilemap.RemoveEntity(e);
        }

        public override void Process(Entity e)
        {
            // only needed to satisfy the interface
        }

        protected override void ProcessEntities(IDictionary<int, Entity> entities)
        {
            Draw();
        }

        protected void Draw()
        {
            //mBackgroundData = Raycaster.FloorCasting(); // UNCOMMENT FOR TEXTURED FLOOR / CEILING

            mWallInfos = Raycaster.Raycasting();

            mVisibleEntities = Raycaster.SpriteRaycasting();

            DrawScreenView(mBackgroundData, mWallInfos, mVisibleEntities);
        }
        
        private void DrawScreenView(Color[] backgroundData, BasicRaycastHitInfo[] raycastHitInfos, List<Entity> sprites)
        {

            mGraphicsDevice.SetRenderTarget(mThreeDeeView);

            //mThreeDeeView.SetData(backgroundData);  // UNCOMMENT FOR TEXTURED FLOOR / CEILING


            mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Resolution.getTransformationMatrix());

            // Comment for textured floor/ceiling
            mSpriteBatch.Draw(mCeilingTexture, mUpperScreen, Color.White);
            mSpriteBatch.Draw(mFloorTexture, mLowerScreen, Color.White);

            DrawWalls(raycastHitInfos);

            DrawSprites(sprites);
            
            DrawFlashScreen();

            mSpriteBatch.End();
        }

        private void DrawFlashScreen()
        {
            if (mFlashIntensity > 0)
            {
                mSpriteBatch.Draw(mPixel, mThreeDeeViewRect, mFlashColor);
                mFlashIntensity *= 0.6f;
                if (mFlashIntensity < 0.1f)
                {
                    mFlashIntensity = 0;
                }
                else
                {
                    mFlashColor.A = (byte)(mFlashIntensity * 255);
                }
            }
        }
        
        public Texture2D ThreeDeeView
        {
            get { return mThreeDeeView; }
        }

        private void DrawWalls(BasicRaycastHitInfo[] raycastHitInfos)
        {
            foreach (BasicRaycastHitInfo raycastHitInfo in raycastHitInfos) // draw wall segments
            {

                TexturedDraw(raycastHitInfo);
            }
        }

        private void DrawSprites(List<Entity> sprites)
        {
            foreach (Entity entity in sprites)
            {
                RaycastSprite sprite = entity.GetComponent<RaycastSprite>();
                //SpriteAnimator animator = entity.GetComponent<SpriteAnimator>();

                Color drawColor = sprite.IsFlashing && TwenMath.Random.NextDouble() > 0.5 ? sprite.DrawColor : Tilemap.GetShadingColor(sprite.Depth);

                //DebugDrawer.DrawRectangle(sprite.TextureOpaqueRect[sprite.FrameIndex]);
                //DebugDrawer.DrawString("Sprite Frame: " + sprite.FrameIndex);

                foreach (SpriteStripe stripe in sprite.Stripes)
                {
                    Rectangle stripeRect = stripe.TextureRect;

                    mSpriteBatch.Draw(sprite.SpriteSheet.Texture, stripe.ScreenRect, stripeRect, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                }
            }

        }


        private void CreateCeiling()
        {
            mGraphicsDevice.SetRenderTarget(mCeilingTexture);
            mSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Resolution.getTransformationMatrix());
            for (int y = 0; y <= mCeiling.Height; y++)
            {
                double dist = Raycaster.HorizontalToDist(mRaycasterResolutionY - y);
                Color drawColor = Tilemap.GetShadingColor(Color.Gray, dist);
                Rectangle destRect = new Rectangle(mCeiling.Left, y, mCeiling.Width, 1);
                mSpriteBatch.Draw(mPixel, destRect, null, drawColor);
            }
            mSpriteBatch.End();
            mGraphicsDevice.SetRenderTarget(null);
        }

        private void CreateFloor()
        {
            mGraphicsDevice.SetRenderTarget(mFloorTexture);
            mSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Resolution.getTransformationMatrix());
            for (int y = 0; y <= mFloor.Height; y++)
            {
                double dist = Raycaster.HorizontalToDist(y + mFloor.Top);
                Color drawColor = Tilemap.GetShadingColor(Color.DarkGray, dist);
                Rectangle destRect = new Rectangle(mFloor.Left, y, mFloor.Width, 1);
                mSpriteBatch.Draw(mPixel, destRect, null, drawColor);
            }
            mSpriteBatch.End();
            mGraphicsDevice.SetRenderTarget(null);
        }



        private void TexturedDraw(BasicRaycastHitInfo raycastHitInfo)
        {
            if (raycastHitInfo.WallType == -1) return;
            var texIndex = raycastHitInfo.WallType;

            Rectangle texRect = mWallTextures.GetSourceRectByIndex(texIndex);

            Texture2D texture = mWallTextures.Texture;


            int doorIndex = IsDoorSide(raycastHitInfo);
            if (doorIndex > 0) // door_side must follow door texture
            {
                texRect = mWallTextures.GetSourceRectByIndex(doorIndex + 1);
            }
            
            //textureSegmentRect = new Rectangle(raycastHitInfo.TexX, 0, 1, mTextureHeight);

            int texX;
            if (raycastHitInfo.VisibleWallSide == WallSide.North || raycastHitInfo.VisibleWallSide == WallSide.East)
                texX = 63 - raycastHitInfo.TexX;    // mirror so text is correct again..
            else texX = raycastHitInfo.TexX;

            mTextureSegmentRect.X = texRect.X + texX;
            mTextureSegmentRect.Y = texRect.Y;
            mTextureSegmentRect.Width = 1;
            mTextureSegmentRect.Height = mTextureHeight;

            mDestinationOnScreenRect.X = raycastHitInfo.X;
            mDestinationOnScreenRect.Y = raycastHitInfo.DrawStartY;
            mDestinationOnScreenRect.Width = 1;
            mDestinationOnScreenRect.Height = raycastHitInfo.LineHeight;
            // simple lighting effect..

            bool isSecretWall = (Tilemap.GetCellMetaDataByPosition(raycastHitInfo.MapX, raycastHitInfo.MapY) == 's');

            Color drawColor;
            if (Tilemap.PseudoShading)
                drawColor = Tilemap.GetShadingColor(raycastHitInfo.DistToWall);
            else
                drawColor = Color.White;

            if (raycastHitInfo.VisibleWallSide == WallSide.North || raycastHitInfo.VisibleWallSide == WallSide.South)
                drawColor = Color.Lerp(drawColor, Color.Black, 0.2f);

            if (isSecretWall && SecretWallsVisible)
            {
                //drawColor = Color.Lerp(drawColor, Color.LightGreen, 0.4f);
                drawColor.A = 220;
            }

            /* if (raycastHitInfo.TexX == 0 || raycastHitInfo.TexX == 63)   // outlines on the edges of tiles
                drawColor = Color.Black; */

            mSpriteBatch.Draw(texture, mDestinationOnScreenRect, mTextureSegmentRect, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
        }



        private int IsDoorSide(BasicRaycastHitInfo raycastHitInfo)
        {
            if (raycastHitInfo.VisibleWallSide == WallSide.East && Tilemap.GetCellMetaDataByPosition(raycastHitInfo.MapX + 1, raycastHitInfo.MapY) == 'd')
                return Tilemap.DoorSpawnPoints[new Point(raycastHitInfo.MapX + 1, raycastHitInfo.MapY)];

            if (raycastHitInfo.VisibleWallSide == WallSide.West && Tilemap.GetCellMetaDataByPosition(raycastHitInfo.MapX - 1, raycastHitInfo.MapY) == 'd')
                return Tilemap.DoorSpawnPoints[new Point(raycastHitInfo.MapX - 1, raycastHitInfo.MapY)];

            if (raycastHitInfo.VisibleWallSide == WallSide.North && Tilemap.GetCellMetaDataByPosition(raycastHitInfo.MapX, raycastHitInfo.MapY - 1) == 'd')
                return Tilemap.DoorSpawnPoints[new Point(raycastHitInfo.MapX, raycastHitInfo.MapY - 1)];

            if (raycastHitInfo.VisibleWallSide == WallSide.South && Tilemap.GetCellMetaDataByPosition(raycastHitInfo.MapX, raycastHitInfo.MapY + 1) == 'd')
                return Tilemap.DoorSpawnPoints[new Point(raycastHitInfo.MapX, raycastHitInfo.MapY + 1)];

            return 0;
        }


        public void FlashScreen(Color color, float intensity)
        {
            mFlashColor = color;
            mFlashIntensity = intensity;
            mFlashColor.A = (byte)(intensity * 255);
        }
    }
}
