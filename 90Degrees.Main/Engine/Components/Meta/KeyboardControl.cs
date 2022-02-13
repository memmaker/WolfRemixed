using Artemis.Interface;
using Microsoft.Xna.Framework.Input;

namespace Twengine.Components.Meta
{
    public class KeyboardControl : IComponent
    {

        public KeyboardControl()
        {
            MoveUp = Keys.W;
            MoveDown = Keys.S;
            MoveLeft = Keys.A;
            MoveRight = Keys.D;
        }

        public Keys Fire { get; set; }
        public Keys MoveUp { get; set; }
        public Keys MoveDown { get; set; }
        public Keys MoveLeft { get; set; }
        public Keys MoveRight { get; set; }


    }
}
