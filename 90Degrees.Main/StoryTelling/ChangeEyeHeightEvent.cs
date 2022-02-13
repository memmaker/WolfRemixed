using Microsoft.Xna.Framework;
using Twengine.Managers;

namespace raycaster.StoryTelling
{
    public class ChangeEyeHeightEvent : StoryTellingEvent
    {
        private int mSpeed;
        private Raycaster mRaycaster;
        private int mTargetValue;

        public ChangeEyeHeightEvent(Raycaster raycaster, int speed, int targetValue)
        {
            mRaycaster = raycaster;
            mTargetValue = targetValue;
            mSpeed = mRaycaster.Camera.EyeHeight < targetValue ? speed : -speed;
        }

        public override sealed void Reset()
        {

        }


        public override bool IsFinished()
        {
            return mRaycaster.Camera.EyeHeight == mTargetValue;
        }

        public override void Update(GameTime gameTime)
        {
            mRaycaster.Camera.EyeHeight += mSpeed;
            if (mSpeed < 0)
            {
                if (mRaycaster.Camera.EyeHeight < mTargetValue)
                    mRaycaster.Camera.EyeHeight = mTargetValue;
            }
            else
            {
                if (mRaycaster.Camera.EyeHeight > mTargetValue)
                    mRaycaster.Camera.EyeHeight = mTargetValue;
            }
        }

        public override void Begin()
        {

        }

        public override void End()
        {

        }

    }
}
