using Artemis;
using Microsoft.Xna.Framework;
using Twengine.Components;

namespace Twengine.SubSystems.Raycast
{
    public class FpsWeaponAnimationSystem : EntityProcessingSystem
    {
        private ComponentMapper<Sprite> mSpriteMapper;
        private ComponentMapper<SpriteAnimator> mAnimatorMapper;
        private ComponentMapper<Transform> mTransformMapper;
        private ComponentMapper<FpsWeaponAnimator> mFpsWeaponMapper;

        public FpsWeaponAnimationSystem()
            : base(typeof(Transform), typeof(Sprite), typeof(SpriteAnimator))
        {

        }

        public override void Initialize()
        {
            mSpriteMapper = new ComponentMapper<Sprite>(world);
            mAnimatorMapper = new ComponentMapper<SpriteAnimator>(world);
            mTransformMapper = new ComponentMapper<Transform>(world);
            mFpsWeaponMapper = new ComponentMapper<FpsWeaponAnimator>(world);
        }

        public override void Process(Entity e)
        {
            Sprite sprite = mSpriteMapper.Get(e);
            SpriteAnimator animator = mAnimatorMapper.Get(e);
            Transform transform = mTransformMapper.Get(e);
            
            animator.UpdateFrame(world.Delta);
            sprite.FrameIndex = animator.CurrentFrameIndex;
            if (!e.HasComponent<FpsWeaponAnimator>()) return;
            FpsWeaponAnimator fpsWeaponAnimator = mFpsWeaponMapper.Get(e);
            transform.Position = fpsWeaponAnimator.GetPosition(sprite.FrameIndex);
        }
    }
}
