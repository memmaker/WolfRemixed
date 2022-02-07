using Artemis;
using Artemis.System;
using Microsoft.Xna.Framework;
using Twengine.Components;

namespace Twengine.SubSystems.Raycast
{
    public class FpsWeaponAnimationSystem : EntityComponentProcessingSystem<Sprite, SpriteAnimator, Transform
        ,FpsWeaponAnimator>
    {
        public FpsWeaponAnimationSystem()
            : base()
        {

        }

        public override void Process(Entity e, Sprite sprite, SpriteAnimator animator, Transform transform, FpsWeaponAnimator fpsWeaponAnimator)
        {
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            animator.UpdateFrame(deltaTimeInSeconds);
            sprite.FrameIndex = animator.CurrentFrameIndex;
            if (!e.HasComponent<FpsWeaponAnimator>()) return;
            transform.Position = fpsWeaponAnimator.GetPosition(sprite.FrameIndex);
        }
    }
}
