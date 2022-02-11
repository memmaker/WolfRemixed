using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Artemis.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using Twengine.Managers;

namespace Twengine.SubSystems
{
    public class SpriteRenderSystem : EntityComponentProcessingSystem<Sprite, Transform>
    {
        private SpriteBatch mSpriteBatch;
        private Transform mTransform;
        private Sprite mSprite;

        public SpriteRenderSystem(SpriteBatch spriteBatch)
            : base()
        {
            mSpriteBatch = spriteBatch;
            
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
