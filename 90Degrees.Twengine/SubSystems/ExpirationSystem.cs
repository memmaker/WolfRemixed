using Artemis;
using Twengine.Components;

namespace Twengine.SubSystems
{
    public class ExpirationSystem : EntityProcessingSystem
    {

        private ComponentMapper<Expires> mExpiresMapper;

        public ExpirationSystem()
            : base(typeof(Expires))
        {
        }

        public override void Initialize()
        {
            mExpiresMapper = new ComponentMapper<Expires>(world);
        }

        public override void Process(Entity e)
        {
            Expires expires = mExpiresMapper.Get(e);
            expires.ReduceLifeTime(world.Delta);

            if (expires.IsExpired)
            {
                if (expires.ExpiredCallback != null) expires.ExpiredCallback();
                e.Delete();
            }

        }
    }
}