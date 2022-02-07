using System.Timers;

namespace XNAHelper
{
    public delegate void TimedEventHandler();
    public class TimedEvent
    {
        private TimedEventHandler mHandler;
        private Timer mTimer;
        
        public TimedEvent(TimedEventHandler handler, float secondsTillFire)
            : this(handler, secondsTillFire, false)
        {

        }
        public TimedEvent(TimedEventHandler handler, float secondsTillFire, bool repeat)
        {
            mHandler = handler;
            mTimer = new Timer(secondsTillFire*1000) {AutoReset = repeat, Enabled = true};
            mTimer.Elapsed += (o, args) => mHandler();
        }

       
    }
}
