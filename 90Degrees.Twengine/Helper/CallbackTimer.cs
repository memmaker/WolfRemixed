using Artemis;

namespace Twengine.Helper
{
    public class CallbackTimer : Timer
    {
        private ExecutionFunctionType mFunction;

        public delegate void ExecutionFunctionType();

        public CallbackTimer(ExecutionFunctionType function, int delay, bool repeat)
            : base(delay, repeat)
        {
            mFunction = function;
        }

        public override void Execute()
        {
            if (mFunction != null) mFunction();
        }
    }
}
