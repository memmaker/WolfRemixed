using Artemis;
using Twengine.Components;

namespace Twengine.SubSystems.Topdown
{
    public class AnimationSystem : EntityProcessingSystem
    {
        private ComponentMapper<Sprite> mSpriteMapper;
        private ComponentMapper<SpriteAnimator> mAnimatorMapper;

        public AnimationSystem()
            : base(typeof(Sprite), typeof(SpriteAnimator))
        {

        }

        public override void Initialize()
        {
            mSpriteMapper = new ComponentMapper<Sprite>(world);
            mAnimatorMapper = new ComponentMapper<SpriteAnimator>(world);
        }

        public override void Process(Entity e)
        {
            Sprite sprite = mSpriteMapper.Get(e);
            SpriteAnimator animator = mAnimatorMapper.Get(e);
            animator.UpdateFrame(world.Delta);
            sprite.FrameIndex = animator.CurrentFrameIndex;
        }
    }
}
