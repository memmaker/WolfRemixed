using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;

namespace Twengine.Components
{

    public class Enemy : Component
    {
        public float VisibleRange { get; set; }
        public float HearingRange { get; set; }
        public float FiringRange { get; set; }

        public int FieldOfView { get; set; }


        public Enemy()
        {
            
        }

    }
}
