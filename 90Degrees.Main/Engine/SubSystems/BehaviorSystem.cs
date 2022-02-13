using Artemis;
using Artemis.System;
using Twengine.Components;

namespace Twengine.SubSystems
{
    public class BehaviorSystem : EntityComponentProcessingSystem<MetaBehavior>
    {
        public BehaviorSystem()
            : base()
        {
        }

        public override void Process(Entity e, MetaBehavior script)
        {
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            script.Update(deltaTimeInSeconds);
        }
    }
}
