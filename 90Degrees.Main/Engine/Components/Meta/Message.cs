using System;
using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace Twengine.Components.Meta
{
    public class Message : IComponent
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
