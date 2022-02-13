using Artemis.Interface;

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
    }
}
