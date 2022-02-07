using Artemis.Utils;
using System;

namespace Twengine.Helper
{
    public class CallbackTimer
    {
        private ExecutionFunctionType mFunction;
        private double delay;
        private bool repeat;

        public delegate void ExecutionFunctionType();

        public CallbackTimer(ExecutionFunctionType function, double delay, bool repeat)
        {
            mFunction = function;
            this.delay = delay;
            this.repeat = repeat;
            
        }

        void Execute()
        {
            if (mFunction != null) mFunction();
        }
        internal void Stop()
        {
            // TODO: Implement this timer
        }

        internal bool IsDone()
        {
            // TODO: Implement this timer
            return false;
        }

        internal void Update(double delta)
        {
            // TODO: Implement this timer
        }

        internal void Reset()
        {
            // TODO: Implement this timer
        }
    }
}
