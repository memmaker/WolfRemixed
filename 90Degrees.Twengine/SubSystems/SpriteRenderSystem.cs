using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using Twengine.Managers;

namespace Twengine.SubSystems
{
    public class SpriteRenderSystem : EntityProcessingSystem
    {
        private ComponentMapper<Sprite> mSpriteMapper;
        private ComponentMapper<Transform> mTransformMapper;
        private SpriteBatch mSpriteBatch;
        private Transform mTransform;
        private Sprite mSprite;
        private MapManager mMapManager;

        public SpriteRenderSystem(SpriteBatch spriteBatch, MapManager mapManager)
            : base(typeof(Transform), typeof(Sprite))
        {
            mSpriteBatch = spriteBatch;
            mMapManager = mapManager;
        }

        public override void Initialize()
        {
            mSpriteMapper = new ComponentMapper<Sprite>(world);
            mTransformMapper = new ComponentMapper<Transform>(world);
        }

        protected override void Begin()
        {
            base.Begin();
            mSpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
        }

        protected override void End()
        {
            base.End();
            mSpriteBatch.End();
        }

        public override void Process(Entity e)
        {
            mTransform = mTransformMapper.Get(e);
            mSprite = mSpriteMapper.Get(e);
            Vector2 origin = mSprite.Origin;
            Vector2 offset = mMapManager != null ? new Vector2(mMapManager.MapViewport.X, mMapManager.MapViewport.Y) : Vector2.Zero;
            mSpriteBatch.Draw(mSprite.SpriteSheet.Texture, mTransform.Position - offset, mSprite.SourceRect, Color.White, mTransform.Rotation, origin, mSprite.Scale, SpriteEffects.None, mSprite.Depth);
        }
    }
}
