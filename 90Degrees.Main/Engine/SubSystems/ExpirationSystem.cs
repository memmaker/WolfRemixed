using Artemis;
using Artemis.System;
using Twengine.Components;

namespace Twengine.SubSystems
{
    public class ExpirationSystem : EntityComponentProcessingSystem<Expires>
    {
        public ExpirationSystem()
            : base()
        {
        }

        public override void Process(Entity e, Expires expires)
        {
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            expires.ReduceLifeTime(deltaTimeInSeconds);

            if (expires.IsExpired)
            {
                if (expires.ExpiredCallback != null) expires.ExpiredCallback();
                e.Delete();
            }

        }
    }
}