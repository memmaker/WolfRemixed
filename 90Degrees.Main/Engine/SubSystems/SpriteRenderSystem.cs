using Artemis;
using Artemis.System;
using IndependentResolutionRendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using raycaster;
using Twengine.Components;

namespace Twengine.SubSystems
{
    public class SpriteRenderSystem : EntityComponentProcessingSystem<Sprite, Transform>
    {
        private SpriteBatch mSpriteBatch;
        private Transform mTransform;
        private Sprite mSprite;
        private RenderTarget2D mSpritesRenderTarget;

        public SpriteRenderSystem(SpriteBatch spriteBatch)
            : base()
        {
            mSpriteBatch = spriteBatch;
            mSpritesRenderTarget = new RenderTarget2D(mSpriteBatch.GraphicsDevice, Const.InternalRenderResolutionWidth, Const.SpriteRenderResolutionHeight);
        }

        public Texture2D SpriteLayer
        {
            get { return mSpritesRenderTarget; }
        }

        protected override void Begin()
        {
            base.Begin();
            mSpriteBatch.GraphicsDevice.SetRenderTarget(mSpritesRenderTarget);
            mSpriteBatch.GraphicsDevice.Clear(Color.Transparent);
            mSpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Resolution.getTransformationMatrix());
        }

        protected override void End()
        {
            mSpriteBatch.End();
            base.End();
        }

        public override void Process(Entity e, Sprite sprite, Transform transform)
        {
            mTransform = transform;
            mSprite = sprite;
            Vector2 origin = mSprite.Origin;
            Vector2 offset = Vector2.Zero;
            mSpriteBatch.Draw(mSprite.SpriteSheet.Texture, mTransform.Position - offset, mSprite.SourceRect, Color.White, mTransform.Rotation, origin, mSprite.Scale, SpriteEffects.None, mSprite.Depth);
        }
    }
}
