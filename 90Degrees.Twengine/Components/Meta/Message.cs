using System;
using Artemis;
using Microsoft.Xna.Framework;

namespace Twengine.Components.Meta
{
    public class Message : Component
    {
        public Message(string messageText)
        {
            Text = messageText;
        }

        public string Text { get; set; }

        public bool IsCentered { get; set; }

        public Color Color { get; set; }

        public bool IsMovingUp { get; set; }

        public bool IsFadingOut { get; set; }
    }
}
