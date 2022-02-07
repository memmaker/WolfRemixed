using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Twengine.Components;

namespace Twengine.SubSystems
{
    public class BehaviorSystem : EntityProcessingSystem
    {
        private ComponentMapper<MetaBehavior> mBehaviorMapper;

        public BehaviorSystem()
            : base(typeof(MetaBehavior))
        {
        }
        public override void Initialize()
        {
            mBehaviorMapper = new ComponentMapper<MetaBehavior>(world);
        }
        
        public override void Process(Entity e)
        {
            MetaBehavior script = mBehaviorMapper.Get(e);
            script.Update(world.Delta);
        }
    }
}
