using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace XNAGameGui.Gui.Widgets
{
    public class MenuBarWidget : WindowWidget
    {
        public Dictionary<string, ButtonWidget> Buttons { get; private set; }
        private string[] mValues;
        private int mSelectionIndex;
        private List<string> mItems;
        public int VerticalPadding { get; set; }
        public int HorizontalPadding { get; set; }
        public MenuBarWidget(List<string> items)
        {
            mItems = items;
            Buttons = new Dictionary<string, ButtonWidget>();
            mSelectionIndex = -1;
            InitializeComponent();
        }
        public void ApplyChanges()
        {
            foreach (KeyValuePair<string, ButtonWidget> buttonWidget in Buttons)
            {
                buttonWidget.Value.Destroy();
            }
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            SpriteFont font = GameGui.Fonts[Font];
            int lineHeight = font.LineSpacing;
            Bounds = new UniRectangle(0, 0, new UniScalar(1.0f, 0), new UniScalar(1.0f, 0));
            int i = 0;
            int lastX = 0;
            foreach (string item in mItems)
            {
                Vector2 measureString = font.MeasureString(item);
                int height = (int)(measureString.Y) + VerticalPadding;
                Buttons[item] = new ButtonWidget
                {
                    Text = item,
                    Bounds = new UniRectangle(new UniScalar(0, lastX), new UniScalar(1f, -height), measureString.X + HorizontalPadding, height)
                };
                lastX += (int)measureString.X + HorizontalPadding + 5;
            }

            mValues = new string[Buttons.Count];

            i = 0;
            foreach (KeyValuePair<string, ButtonWidget> keyValuePair in Buttons)
            {
                AddChild(keyValuePair.Value);
                mValues[i] = keyValuePair.Key;
                i++;
            }
            if (Buttons.Count > 0)
                SelectionLeft();
        }

        public string GetMouseSelectedItem()
        {
            foreach (KeyValuePair<string, ButtonWidget> buttonWidget in Buttons)
            {
                if (buttonWidget.Value.IsSelected)
                    return buttonWidget.Key;
            }
            return "";
        }

        public void SelectionLeft()
        {
            if (Buttons.Count == 0)
            {
                mSelectionIndex = -1;
                return;
            }

            if (mSelectionIndex >= 0 && mSelectionIndex < Buttons.Count)
            {
                Buttons[mValues[mSelectionIndex]].IsSelected = false;
            }

            mSelectionIndex--;
            if (mSelectionIndex < 0) mSelectionIndex = 0;

            Buttons[mValues[mSelectionIndex]].IsSelected = true;
        }

        public void SelectionRight()
        {
            if (Buttons.Count == 0)
            {
                mSelectionIndex = -1;
                return;
            }

            if (mSelectionIndex >= 0 && mSelectionIndex < Buttons.Count)
            {
                Buttons[mValues[mSelectionIndex]].IsSelected = false;
            }

            mSelectionIndex++;
            if (mSelectionIndex > Buttons.Count - 1) mSelectionIndex = Buttons.Count - 1;

            Buttons[mValues[mSelectionIndex]].IsSelected = true;
        }
    }


}
