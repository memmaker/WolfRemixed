using System;
using Artemis;
using Artemis.System;
using Microsoft.Xna.Framework;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Helper;
using Twengine.Managers;
using XNAHelper;

namespace Twengine.SubSystems.Raycast
{
    public class RayAnimationSystem : EntityComponentProcessingSystem<SpriteAnimator, Transform>
    {
        private Raycaster mRaycaster;
        public int SideRangeInDegrees { get; set; }
        public RayAnimationSystem(Raycaster raycaster)
            : base()
        {
            mRaycaster = raycaster;
            SideRangeInDegrees = 27;
        }

        public override void Process(Entity e, SpriteAnimator animator, Transform transform)
        {
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            animator.UpdateFrame(deltaTimeInSeconds);

            if (e.HasComponent<Sprite>() || e.HasComponent<RaycastSprite>()) {
                Sprite sprite = e.HasComponent<Sprite>() ? e.GetComponent<Sprite>() : e.GetComponent<RaycastSprite>();
                int frameIndex = GetNextFrameIndex(transform, animator, sprite);

                sprite.FrameIndex = frameIndex;
            }
        }


        private int GetNextFrameIndex(Transform transform, SpriteAnimator animator, Sprite sprite)
        {
            int viewAngleIndex = GetViewAngle(transform);
            int nextFrameIndex = sprite.FrameIndex;
            if (animator != null)
            {
                nextFrameIndex = animator.CurrentFrameIndex;
                if (animator.IsCurrentAnimationDirectional)
                {
                    nextFrameIndex = animator.GetDirectionalFrameIndex(animator.CurrentAnimation, viewAngleIndex,
                                                                       animator.CurrentFrameIndex);
                    //angleIndex = GetAngleIndexByAngle(viewAngleIndex, sprite);
                }
            }
            return nextFrameIndex;
        }

        private int GetViewAngle(Transform transform)
        {
            Camera camera = mRaycaster.Camera;
            float sideRange = MathHelper.ToRadians(SideRangeInDegrees);

            //compute the angle between de camera and the objetc
            Vector2 v = transform.Position - camera.Position;
            double angleR = Math.Atan2(v.Y, v.X);

            Vector2 direction = TwenMath.RotationToDirectionVector(transform.Rotation);
            //compute the angle of the object's direction
            double angleL = Math.Atan2(direction.Y, direction.X);

            //obtain the difference between de angle of objects and the angle of object's direction
            double angleO = angleL - angleR;

            // invert the result 270 degrees - angle
            double angleF = MathHelper.ToRadians(270) - angleO;

            int index = 0;

            //calculates de range of the interleaved textures (90 - range of the nort/east/west/south textures )

            double interleavedTextureRange = MathHelper.ToRadians(90) - sideRange;   // 24 based on 90° fov

            // if angle is greater than 360 degrees
            float threeSixty = MathHelper.ToRadians(360);
            angleF = TwenMath.NormalizeAngle(angleF);

            //
            //calculates the ranges for every texture based on the ranges constants
            double range1 = sideRange / 2;
            double range2 = range1 + interleavedTextureRange;
            double range3 = range2 + sideRange;
            double range4 = range3 + interleavedTextureRange;
            double range5 = range4 + sideRange;
            double range6 = range5 + interleavedTextureRange;
            double range7 = range6 + sideRange;
            double range8 = range7 + interleavedTextureRange;


            //compute the texture index depending on the ranges);
            if (angleF >= 0 && angleF < range1)
                index = 2;
            else if (angleF >= range1 && angleF < range2)
                index = 1;
            else if (angleF >= range2 && angleF < range3)
                index = 0;
            else if (angleF >= range3 && angleF < range4)
                index = 7;
            else if (angleF >= range4 && angleF < range5)
                index = 6;
            else if (angleF >= range5 && angleF < range6)
                index = 5;
            else if (angleF >= range6 && angleF < range7)
                index = 4;
            else if (angleF >= range7 && angleF < range8)
                index = 3;
            else if (angleF >= range8 && angleF < threeSixty)
                index = 2;
            return index;
        }
    }
}
