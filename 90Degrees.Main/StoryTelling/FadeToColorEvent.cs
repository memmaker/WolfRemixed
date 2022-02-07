using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Helper;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;

namespace raycaster.StoryTelling
{
    public class FadeToColorEvent : StoryTellingEvent
    {
        private Color mCurrentColor;
        private Color mTargetColor;
        private int mSpeed;
        private LabelWidget mFullScreenOverlay;
        private int mActualAlphaCounter;
        private Color mSourceColor;

        public FadeToColorEvent(Color targetColor, int speed)
            : this(new Color(targetColor.R, targetColor.G, targetColor.B, (byte)0), targetColor, speed)
        {
        }

        public FadeToColorEvent(Color sourceColor, Color targetColor, int speed)
        {
            mTargetColor = targetColor;
            mSpeed = sourceColor.A < targetColor.A ? speed : -speed;
            mSourceColor = sourceColor;
            Reset();
        }

        public override sealed void Reset()
        {
            mCurrentColor = mSourceColor;
            mActualAlphaCounter = mSourceColor.A;
        }


        public override bool IsFinished()
        {
            return mSpeed > 0 ? mActualAlphaCounter >= mTargetColor.A : mActualAlphaCounter <= mTargetColor.A;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsFinished()) return;
            byte alphaChange = (byte)Math.Abs(mSpeed);
            mActualAlphaCounter += mSpeed;
            if (mSpeed < 0)
            {
                mCurrentColor.A -= alphaChange;
                if (mActualAlphaCounter <= mTargetColor.A)
                    mCurrentColor.A = mTargetColor.A;
            }
            else
            {
                mCurrentColor.A += alphaChange;
                if (mActualAlphaCounter >= mTargetColor.A)
                    mCurrentColor.A = mTargetColor.A;
            }

            mFullScreenOverlay.LabelColor = mCurrentColor;
        }

        public override void Begin()
        {
            mFullScreenOverlay = RaycastGame.FullScreenOverlay();
            mFullScreenOverlay.LabelColor = mCurrentColor;
        }

        public override void End()
        {
            if (mFullScreenOverlay == null) return;
            mFullScreenOverlay.Destroy();
            mFullScreenOverlay = null;
        }

    }
}
