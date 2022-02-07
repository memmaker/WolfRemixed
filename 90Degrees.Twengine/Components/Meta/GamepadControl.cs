using Artemis;

namespace Twengine.Components.Meta
{
    public class GamepadControl : Component
    {
        public GamepadControl() : this(0)
        {
        }

        public GamepadControl(int number)
        {
            Number = number;
            
        }

        public int Number { get; private set; }

        public int FireButton { get; set; }

    }
}
