using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Managers;
using Twengine.Helper;
using Artemis.System;
using Artemis.Attributes;
using Artemis.Manager;
using Artemis;

namespace Twengine.SubSystems
{
    [ArtemisEntitySystem(GameLoopType = GameLoopType.Draw, Layer = 0)]
    public class MessageRenderSystem : EntityComponentProcessingSystem<Message>
    {
        private SpriteBatch mSpriteBatch;
        private SpriteFont mFont;
        private int mIndex;
        public MessageRenderSystem(SpriteBatch spriteBatch, SpriteFont font)
        {
            this.mSpriteBatch = spriteBatch;
            this.mFont = font;
        }

        protected override void Begin()
        {
            mIndex = 0;
            base.Begin();
            mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
        }

        protected override void End()
        {
            base.End();
            mSpriteBatch.End();
        }

        public override void Process(Entity e, Message message)
        {
            DeferredDrawString(message.Text,mIndex,message.IsCentered);
        }

        private void DeferredDrawString(string text, int index, bool centered)
        {
            Vector2 shadowOffset = new Vector2(-1, -2);
            Vector2 origin = new Vector2(10, 10);
            Vector2 pos;
            if (centered)
            {
                int width = mSpriteBatch.GraphicsDevice.Viewport.Width;
                int height = mSpriteBatch.GraphicsDevice.Viewport.Height;
                Vector2 screenCenter = new Vector2(width/2,height/2);
                Vector2 stringSize = mFont.MeasureString(text);
                Vector2 fontOffset = new Vector2(stringSize.X/2,stringSize.Y/2 + mFont.LineSpacing);
                pos = screenCenter - fontOffset;
            }
            else
            {
                pos = origin + (Vector2.UnitY * index * mFont.LineSpacing);
                mIndex++;
            }
            mSpriteBatch.DrawString(mFont, text, pos + shadowOffset, Color.Black);
            mSpriteBatch.DrawString(mFont, text, pos, Color.Red); // 
        }
    }
}
