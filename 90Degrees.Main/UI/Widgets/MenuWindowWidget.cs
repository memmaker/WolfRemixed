using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace XNAGameGui.Gui.Widgets
{
    public class MenuWindowWidget : WindowWidget
    {
        public List<ButtonWidget> Buttons { get; private set; }
        int mCurrentSelection = 0;
        public MenuWindowWidget(IEnumerable<string> items, int windowWidth, int buttonWidth)
        {
            Buttons = new List<ButtonWidget>();
            InitializeComponent(items, windowWidth, buttonWidth);
            Buttons[mCurrentSelection].IsSelected = true;
        }

        public ButtonWidget AddButton(string label, UniRectangle bounds)
        {
            ButtonWidget buttonWidget = new ButtonWidget { Text = label, Bounds = bounds };
            Buttons.Add(buttonWidget);
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
                AddButton(item, new UniRectangle(new UniScalar(0.5f, -(buttonWidth / 2)), new UniScalar(0.5f, 10.0f + (i * (stringSize.Y + 5))), buttonWidth, stringSize.Y));
                i++;
            }

        }


        public ButtonWidget GetSelectedButton()
        {
            return Buttons[this.mCurrentSelection];
        }

        public void SelectPrevious()
        {
            Buttons[mCurrentSelection].IsSelected = false;
            mCurrentSelection--;
            if (mCurrentSelection < 0)
            {
                mCurrentSelection = Buttons.Count - 1;
            }
            Buttons[mCurrentSelection].IsSelected = true;
        }

        public void SelectNext()
        {
            Buttons[mCurrentSelection].IsSelected = false;
            mCurrentSelection = (mCurrentSelection + 1) % Buttons.Count;
            Buttons[mCurrentSelection].IsSelected = true;
        }
    }


}
