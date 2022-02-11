using Artemis.Utils;
using System;

namespace Twengine.Helper
{
    public class CallbackTimer
    {
        private ExecutionFunctionType mFunction;
        private double delay;
        private double currentTimer;
        private bool repeat;
        private bool done = false;

        public delegate void ExecutionFunctionType();

        public CallbackTimer(ExecutionFunctionType function, double delay, bool repeat)
        {
            mFunction = function;
            this.delay = delay;
            this.repeat = repeat;
            this.currentTimer = delay;
        }

        void Execute()
        {
            if (mFunction != null) mFunction();
        }
        internal void Stop()
        {
            done = true;
        }

        internal bool IsDone()
        {
            return done;
        }

        internal void Update(double delta)
        {
            if (!done)
            {
                currentTimer -= delta;
                if (currentTimer <= 0)
                {
                    Execute();
                    if (repeat)
                    {
                        currentTimer = delay;
                    }
                    else
                    {
                        done = true;
                    }
                }
            }
        }

        internal void Reset()
        {
            currentTimer = delay;
            done = false;
        }
    }
}
