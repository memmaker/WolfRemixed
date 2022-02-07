using System;
using Microsoft.Xna.Framework;

namespace raycaster.StoryTelling
{
    public abstract class StoryTellingEvent
    {
        public abstract bool IsFinished();
        public abstract void Update(GameTime gameTime);
        public abstract void Begin();
        public abstract void End();

        /// <summary>
        ///  When true, End() gets called only when the StoryTellingState leaves..
        ///  </summary>
        public virtual bool DeferredEnd { get; set; }

        public virtual bool IsNonBlocking { get; set; }
        public bool IsMandatory { get; set; }

        public abstract void Reset();
    }
}
