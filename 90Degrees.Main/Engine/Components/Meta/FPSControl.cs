using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework.Input;

namespace Twengine.Components.Meta
{
    public class FPSControl : IComponent
    {

        public Keys MoveForward { get; set; }
        public Keys MoveBackward { get; set; }
        public Keys MoveLeft { get; set; }
        public Keys MoveRight { get; set; }

        public Keys ToggleMap { get; set; }
        public Keys MapZoomIn { get; set; }
        public Keys MapZoomOut { get; set; }

        public Keys NextWeapon { get; set; }
    }
}