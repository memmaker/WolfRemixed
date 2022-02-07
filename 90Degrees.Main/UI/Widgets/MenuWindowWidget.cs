using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAGameGui.Gui.Widgets
{
    public class MenuWindowWidget : WindowWidget
    {
        public Dictionary<string, ButtonWidget> Buttons { get; private set; }
        int mCurrentSelection;
        public MenuWindowWidget(IEnumerable<string> items, int windowWidth, int buttonWidth)
        {
            Buttons = new Dictionary<string, ButtonWidget>();
            InitializeComponent(items, windowWidth, buttonWidth);
            
        }

        public ButtonWidget AddButton(string label, UniRectangle bounds)
        {
            ButtonWidget buttonWidget = new ButtonWidget { Text = label, Bounds = bounds };
            Buttons[label] = buttonWidget;
            AddChild(buttonWidget);
            return buttonWidget;
        }

        private void InitializeComponent(IEnumerable<string> items, int windowWidth, int buttonWidth)
        {
            SpriteFont font = GameGui.Fonts[Font];
            
            Bounds = new UniRectangle(new UniScalar(0.5f, -(windowWidth / 2)),
                                      new UniScalar(0.5f, -142),
                                      windowWidth,
                                      284.0f);
            int i = 0;
            foreach (string item in items)
            {
                Vector2 stringSize = font.MeasureString(item);
                AddButton(item, new UniRectangle(new UniScalar(0.5f, -(buttonWidth/2)), new UniScalar(0.5f, 10.0f + (i*(stringSize.Y + 5))), buttonWidth, stringSize.Y));
                i++;
            }

        }
       
       
    }

   
}
