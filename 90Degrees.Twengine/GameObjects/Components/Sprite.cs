using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Helper;
using Twengine.Managers;
using Rectangle = xTile.Dimensions.Rectangle;

namespace Twengine.GameObjects.Components
{
    public class Sprite
    {
        protected SpriteBatch mSpriteBatch;

        public Texture2D Texture { get; private set; }
        public SpriteSheet SheetController { get; private set; }
        public string AssetName { get; private set; }
        
        private Vector2 mOrigin;
        private GameObject mParentGameObject;
        
        public float LayerDepth { get; set; }
        public Color Color { get; set; }
        public Color HoverColor { get; set; }

        /// <summary>
        /// Toggles wether this Sprite is drawn based on it's own Rotation and Position (false) or
        /// if it queries the GameMechanic representation it's bound to for these attributes (true).
        /// </summary>
        public bool IsGameObject { get { return mParentGameObject != null; } }
        public bool IsAnimated { get { return SheetController != null; } }
        private Texture2D mDummyTexture;

        private float mRotation;
        public float Rotation
        {
            get { return mRotation; }
            set
            {
                mRotation = value;
                UpdateTransformationMatrix();
            }
        }

        private Vector2 mPosition;

        public Vector2 Position
        {
            get { return mPosition; }
            set
            {
                mPosition = value;
                UpdateTransformationMatrix();
            }
        }

        public float ScaledWidth { get; set; }
        public float ScaledHeight { get; set; }

        public int FrameHeight { get; set; }
        public int FrameWidth { get; set; }

        private Vector2 mScale;
        private bool mFlashing;
        private double mStartFlashTime;
        private Matrix mTransformationMatrix;

        public Vector2 Scale
        {
            get { return mScale; }
            set
            {
                mScale = value;
                ScaledWidth = FrameWidth * mScale.X;
                ScaledHeight = FrameHeight * mScale.Y;
                UpdateTransformationMatrix();
            }
        }

        public int X { get; set; }
        public int Y { get; set; }

        public int LifeTime { get; set; }
        private double mCreatedAt;

        /// <summary>
        /// Non-Animated Sprite Constructor
        /// </summary>
        /// <param name="assetname"></param>
        /// <param name="asset"></param>
        /// <param name="spriteBatch"></param>
        public Sprite(string assetname, Texture2D asset, SpriteBatch spriteBatch) : this(assetname,asset,spriteBatch,asset.Width,asset.Height){ }

        /// <summary>
        /// Animated Sprite Constructor
        /// </summary>
        /// <param name="assetname"></param>
        /// <param name="asset"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="frameWidth"></param>
        /// <param name="frameHeight"></param>
        public Sprite(string assetname, Texture2D asset, SpriteBatch spriteBatch, int frameWidth, int frameHeight)
        {
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            mOrigin = new Vector2(FrameWidth / 2.0f, FrameHeight / 2.0f);

            ScaledWidth = FrameWidth * Scale.X;
            ScaledHeight = FrameHeight * Scale.Y;

            LifeTime = -1;
            mScale = new Vector2(1f, 1f);
            mSpriteBatch = spriteBatch;
            LayerDepth = 0f;
            Color = Color.White;
            HoverColor = Color.White;
            Position = Vector2.Zero;
            Texture = asset;
            AssetName = assetname;
            mDummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            mDummyTexture.SetData(new Color[] {Color.White});
            Color transparentGreen = Color.Green;
            transparentGreen.A = 100;
            UpdateTransformationMatrix();

        }

        public void LoadAnimation(int framesPerSec, List<int> animationIndices)
        {
            SheetController = new SpriteSheet(Texture, framesPerSec, animationIndices, FrameWidth, FrameHeight);
        }

        public void LoadAnimation(int framesPerSec)
        {
            int frameCount = (Texture.Width/FrameWidth)*(Texture.Height/FrameHeight);
            List<int> indices = new List<int>();
            for (int i = 0; i < frameCount; i++)
            {
                indices.Add(i);
            }
            SheetController = new SpriteSheet(Texture, framesPerSec, indices, FrameWidth, FrameHeight);
        }

        public virtual void Draw(Rectangle mapViewport)
        {
            Vector2 offset = new Vector2(mapViewport.X, mapViewport.Y);
            if (IsAnimated)
            {
                SheetController.DrawFrame(mSpriteBatch, Position - offset, Rotation, mOrigin, Scale, LayerDepth);
            }
            else
            {
                mSpriteBatch.Draw(Texture, Position - offset, null, Color, Rotation, mOrigin, Scale, SpriteEffects.None, LayerDepth);
            }
        }

        private void UpdateTransformationMatrix()
        {
            mTransformationMatrix = Matrix.CreateTranslation(new Vector3(-mOrigin, 0)) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateScale(new Vector3(mScale, 1f)) * Matrix.CreateTranslation(new Vector3(Position, 0));
            if (IsGameObject && ParentGameObject.IsCollider)
            {
                ParentGameObject.Collider.UpdateBoundingBox();
            }
        }

        public Matrix GetTransformationMatrix()
        { 
            return mTransformationMatrix;
        }

        public Color[,] GetTextureData()
        {
            Color[] colors1D = new Color[FrameWidth*FrameHeight];
            if (IsAnimated)
            {
                colors1D = SheetController.GetFramaData();
            }
            else
            {
                Texture.GetData(colors1D);
            }
            Color[,] colors2D = new Color[FrameWidth, FrameHeight];
            for (int x = 0; x < FrameWidth; x++)
                for (int y = 0; y < FrameHeight; y++)
                    colors2D[x, y] = colors1D[x + y * FrameWidth];

            return colors2D;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (LifeTime > 0)
            {
                // we have a lifetime set..
                if ((gameTime.TotalGameTime.TotalSeconds - mCreatedAt) > LifeTime)
                {
                    // time is up
                    Console.WriteLine("Destroying Sprite: " + AssetName);
                    SpriteManager.Default.RemoveSprite(this);
                    return;
                }
            }
            if (IsGameObject)
            {
                Position = ParentGameObject.Position;
                Rotation = ParentGameObject.Rotation;
            }
            if (mFlashing) UpdateFlash(gameTime);
            if (IsAnimated) SheetController.UpdateFrame((float) gameTime.ElapsedGameTime.TotalSeconds * TwengineGame.TimeDilationFactor);
        }

        private void UpdateFlash(GameTime gameTime)
        {
            if ((gameTime.TotalGameTime.TotalMilliseconds - mStartFlashTime) > 200)
            {
                // it's over
                mFlashing = false;
                Color = Color.White;
                Color transparentGreen = Color.Green;
                transparentGreen.A = 100;
            }
            else
            {
                Color = Color.Red;
                Color transparentRed = Color.Red;
                transparentRed.A = 100;
            }
        }


        private void ParentGameObjectStateChanged(GameObject source)
        {
            switch (mParentGameObject.State)
            {
                case GameObjectState.Idle:
                    //Color = Color.White;
                    break;
                case GameObjectState.Colliding:
                    //Color = Color.Red;
                    break;
            }
            if (mParentGameObject.State == GameObjectState.Idle && mParentGameObject.IsMoving)
            {
                //Color = Color.Green;
            }
        }


        public override string ToString()
        {
            return AssetName;
        }

        public void StartCreationTimer()
        {
            mCreatedAt = TwengineGame.Time.TotalGameTime.TotalSeconds;
        }

        public GameObject ParentGameObject
        {
            get { return mParentGameObject; }

            internal set
            {
                if(mParentGameObject != null && value != mParentGameObject)
                {
                    mParentGameObject.StateChanged -= ParentGameObjectStateChanged;
                    mParentGameObject.Moved -= ParentGameObjectStateChanged;
                }
                mParentGameObject = value;
                mParentGameObject.StateChanged += ParentGameObjectStateChanged;
                mParentGameObject.Moved += ParentGameObjectStateChanged;
            }
        }

        

        public void FlashForOneSecond()
        {
            mFlashing = true;
            mStartFlashTime = TwengineGame.Time.TotalGameTime.TotalMilliseconds;            
        }
    }
}