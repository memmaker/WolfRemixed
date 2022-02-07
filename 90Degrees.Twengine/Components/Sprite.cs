using System;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Helper;
using XNAHelper;

namespace Twengine.Components
{
    public class Sprite : Component
    {
        public SpriteSheet SpriteSheet { get; private set; }
        protected int mFrameIndex;
        protected Rectangle mSourceRect;

        public Sprite(SpriteSheet sheet, int index)
        {
            Orientation = Orientation.None;
            Scale = 1f;
            Opacity = 1f;
            SpriteSheet = sheet;
            mFrameIndex = index;
            UpdateSourceRect();
        }

        private void UpdateSourceRect()
        {
            mSourceRect = SpriteSheet.GetSourceRectByIndex(mFrameIndex);
            
        }
        
        public Orientation Orientation { get; set; }
        /// <summary>
        /// Setting this has the side-effect of updating the internal source rect..
        /// </summary>
        public int FrameIndex { get { return mFrameIndex; } set { mFrameIndex = value; UpdateSourceRect(); } }

        public Rectangle SourceRect { get { return mSourceRect; } }


        public float Scale { get; set; }

        public Vector2 Origin { get; set; }

        public float Depth { get; set; }

        public float Opacity { get; set; }
    }
}
