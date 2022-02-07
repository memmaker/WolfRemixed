using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNAGameGui.Gui.Widgets;

namespace raycaster.StoryTelling
{
    public class CallBackEvent : StoryTellingEvent
    {
        private EventHandler<EventArgs> mCallback;

        public CallBackEvent(EventHandler<EventArgs> callback)
        {
            mCallback = callback;
        }

        public override bool IsFinished()
        {
            return true;
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Begin()
        {
            mCallback(this, new EventArgs());
        }

        public override void End()
        {

        }

        public override void Reset()
        {
            
        }
    }
}
