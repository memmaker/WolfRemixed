using Microsoft.Xna.Framework;

namespace XNAGameGui.Gui.Widgets
{
    public class WindowWidget : LabelWidget
    {
        public string Title { get; set; }

        public WindowWidget()
        {
            LabelColor = Color.Gainsboro;
        }

    }
}
