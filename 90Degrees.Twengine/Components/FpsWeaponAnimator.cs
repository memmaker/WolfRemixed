using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Microsoft.Xna.Framework;

namespace Twengine.Components
{
    public class FpsWeaponAnimator : Component
    {
        private Vector2 mDefaultPos;
        private Vector2[] mFramePositions;
        public int HitOnFrame { get; set; }
        public FpsWeaponAnimator(int frameCount, Vector2 defaultPos, int hitOnFrame)
        {
            mFramePositions = new Vector2[frameCount];
            mDefaultPos = defaultPos;
            HitOnFrame = hitOnFrame;
        }
        public Vector2 GetPosition(int frameIndex)
        {
            if (mFramePositions[frameIndex] != Vector2.Zero) return mFramePositions[frameIndex];
            return mDefaultPos;
        }
        public void AddFramePosition(int frameIndex, Vector2 pos)
        {
            mFramePositions[frameIndex] = pos;
        }
    }
}
