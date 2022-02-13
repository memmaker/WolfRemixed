using System;
using System.Collections.Generic;
using System.Text;
using Artemis;
using Microsoft.Xna.Framework;
using raycaster;
using Twengine.Components;
using XNAHelper;

namespace Degrees.Main.UI
{
    public class HudFace
    {
        private readonly HealthPoints mPlayerHealthPoints;
        private SpriteAnimator mHudFaceAnimator;

        public HudFace(HealthPoints playerHealthPoints)
        {
            mPlayerHealthPoints = playerHealthPoints;

            Entity hudFace = EntitySpawn.CreateHudFace(new Point(Const.InternalRenderResolutionWidth, Const.InternalRenderResolutionHeight),
                (o, args) => AnimateFaceIdleAnimation());
            mHudFaceAnimator = hudFace.GetComponent<SpriteAnimator>();
        }

        #region status bar face animation

        public void AnimateEvilGrinOnHudFace()
        {
            if (mPlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = "Grin20Percent";
            else if (mPlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = "Grin40Percent";
            else if (mPlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = "Grin60Percent";
            else if (mPlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = "Grin80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = "GrinFullHealth";

            mHudFaceAnimator.ResetAndPlay();
        }

        public void AnimatePainOnHudFace(int damage)
        {
            string prefix = "";
            if (damage >= 20)
                prefix = "Big";

            if (mPlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain20Percent";
            else if (mPlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain40Percent";
            else if (mPlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain60Percent";
            else if (mPlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = prefix + "PainFullHealth";

            mHudFaceAnimator.ResetAndPlay();


        }

        public void AnimateFaceIdleAnimation()
        {
            if (mPlayerHealthPoints.IsImmortal)
                mHudFaceAnimator.CurrentAnimation = "IdleImmortal";
            else if (mPlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = "Idle20Percent";
            else if (mPlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = "Idle40Percent";
            else if (mPlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = "Idle60Percent";
            else if (mPlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = "Idle80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = "IdleFullHealth";

            mHudFaceAnimator.ResetAndPlay();
        }

        public void AnimateFaceLookingToEnemy(RelativeDirection dir)
        {
            if (mPlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "20Percent";
            else if (mPlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "40Percent";
            else if (mPlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "60Percent";
            else if (mPlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "FullHealth";

            mHudFaceAnimator.ResetAndPlay();
        }

        #endregion
    }
}
