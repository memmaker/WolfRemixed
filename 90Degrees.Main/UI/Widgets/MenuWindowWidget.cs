using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace XNAGameGui.Gui.Widgets
{
    public class MenuWindowWidget : WindowWidget
    {
        public List<ButtonWidget> Buttons { get; private set; }
        int mCurrentSelection = 0;
        private readonly int mButtonWidth;
        public float MenuButtonBeginFraction { get; set; }

        public MenuWindowWidget(int windowWidth, int buttonWidth)
        {
            mButtonWidth = buttonWidth;
            Buttons = new List<ButtonWidget>();
            InitializeComponent(windowWidth, buttonWidth);
            MenuButtonBeginFraction = 0.5f;
        }

        public void AddButton(ButtonWidget button)
        {
            int i = Buttons.Count;
            SpriteFont font = GameGui.Fonts[Font];
            Vector2 stringSize = font.MeasureString(button.Text);
            button.Bounds = new UniRectangle(new UniScalar(0.5f, -(mButtonWidth / 2)),
                new UniScalar(MenuButtonBeginFraction, 10.0f + (i * (stringSize.Y + 5))), mButtonWidth, stringSize.Y);
            Buttons.Add(button);
            AddChild(button);
            if (Buttons.Count == 1 && mCurrentSelection == 0)
            {
                Buttons[mCurrentSelection].IsSelected = true;
            }
        }

        public void AddSimpleButtons(IEnumerable<string> items)
        {
            SpriteFont font = GameGui.Fonts[Font];
            foreach (string item in items)
            {
                ButtonWidget button = new ButtonWidget()
                {
                    Text = item
                };
                AddButton(button);
            }

            Buttons[mCurrentSelection].IsSelected = true;
        }

        private void InitializeComponent(int windowWidth, int buttonWidth)
        {
            Bounds = new UniRectangle(new UniScalar(0.5f, -(windowWidth / 2)),
                new UniScalar(0.5f, -142),
                windowWidth,
                284.0f);
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
