using Artemis.Interface;

namespace Twengine.Components
{
    public class Expires : IComponent
    {
        private float mLifeTime;

        public Expires() { }
        public DefaultCallback ExpiredCallback { get; set; }


        public Expires(float lifeTime)
        {
            this.mLifeTime = lifeTime;
        }

        public float LifeTime
        {
            get { return mLifeTime; }
            set { mLifeTime = value; }
        }

        public void ReduceLifeTime(float lifeTime)
        {
            this.mLifeTime -= lifeTime;
        }

        public bool IsExpired
        {
            get { return mLifeTime <= 0; }
        }
    }

    public delegate void DefaultCallback();
}