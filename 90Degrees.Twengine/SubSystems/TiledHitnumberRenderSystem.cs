using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Managers;
using Twengine.Helper;

namespace Twengine.SubSystems
{
    public class TiledHitnumberRenderSystem : EntityProcessingSystem
    {
        private ComponentMapper<Message> mMessageMapper;
        private SpriteBatch mSpriteBatch;
        private SpriteFont mFont;
        private int mGridSize;

        public TiledHitnumberRenderSystem(SpriteBatch spriteBatch, SpriteFont font, int gridsize)
            : base(typeof(Message), typeof(Transform))
        {
            mSpriteBatch = spriteBatch;
            mFont = font;
            mGridSize = gridsize;
        }

        public override void Initialize()
        {
            mMessageMapper = new ComponentMapper<Message>(world);
        }

        protected override void Begin()
        {
            base.Begin();
            mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
        }

        protected override void End()
        {
            base.End();
            mSpriteBatch.End();
        }

        public override void Process(Entity e)
        {
            Message message = mMessageMapper.Get(e);
            DeferredDrawString(e.GetComponent<Transform>(),message);
        }

        private void DeferredDrawString(Transform transform, Message msg)
        {
            Vector2 stringSize = mFont.MeasureString(msg.Text);

            Vector2 shadowOffset = new Vector2(-1, -2);
            Vector2 pixelPosition = new Vector2(transform.Position.X * mGridSize + (mGridSize/2),
                                                transform.Position.Y * mGridSize + (mGridSize / 2));
            Color shadowColor = Color.Black;
            shadowColor.A = msg.Color.A;

            mSpriteBatch.DrawString(mFont, msg.Text, pixelPosition + shadowOffset, shadowColor, 0f, stringSize / 2, 1f, SpriteEffects.None, 0f);
            mSpriteBatch.DrawString(mFont, msg.Text, pixelPosition, msg.Color, 0f, stringSize / 2, 1f, SpriteEffects.None, 0f);

            if (msg.IsMovingUp)
                transform.Position = new Vector2(transform.Position.X,transform.Position.Y - 0.01f);


            if (msg.IsFadingOut)
            {
                if (msg.Color.A == 0) return;

                Color nextColor = msg.Color;
                int nextColorAlpha = msg.Color.A;
                nextColorAlpha -= 2;
                if (nextColorAlpha <= 0)
                    nextColor.A = 0;
                else
                    nextColor.A = (byte) nextColorAlpha;
                msg.Color = nextColor;
            }
        }
    }
}
