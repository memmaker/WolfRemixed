using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAGameGui.Gui.Widgets
{
    public delegate string CounterToString(int counter);



    public class ButtonWidget : LabelWidget
    {
        private int mCounterMax;
        public int CounterMax
        {
            get { return mCounterMax; }
            set
            {
                mCounterMax = value;
                Counter = mCounter;
            }
        }

        private int mCounterMin;
        public int CounterMin
        {
            get { return mCounterMin; }
            set
            {
                mCounterMin = value;
                Counter = mCounter;
            }
        }

        public Color SelectionColor { get; set; }
        public bool IsSelected { get; set; }
        public bool AppendString { get; set; }
        public string AppendedString { get; set; }
        public bool IsMarked { get; set; }
        public bool IsMarkable { get; set; }
        public bool AppendCounter { get; set; }
        private int mCounter;


        public int Counter
        {
            get { return mCounter; }
            set
            {
                mCounter = (int)MathHelper.Clamp(value, CounterMin, CounterMax);
            }
        }

        public int YTextOffset { get; set; }
        public CounterToString CounterToStringFunction { get; set; }

        public string Suffix { get; set; }


        public ButtonWidget() : base()
        {
            SelectionColor = Color.Red;
            Counter = 1;
            AppendCounter = false;
            CounterToStringFunction = CounterToString;
            YTextOffset = 0;
            IsMarked = false;
            IsMarkable = false;
        }

        public override void Draw(SpriteBatch spriteBatch, GameGui gui)
        {
            spriteBatch.Draw(GameGui.WhiteRectangle, mDestinationRectangle, IsSelected ? SelectionColor : LabelColor);
            string textToRender = Text;
            if (IsMarked)
            {
                textToRender = "* " + textToRender;
            }
            if (AppendCounter)
            {
                textToRender += CounterToStringFunction(Counter);
            }
            if (AppendString)
            {
                textToRender += AppendedString;
            }
            if (Suffix != "")
            {
                textToRender += Suffix;
            }
            if (textToRender != "")
            {
                SpriteFont font = GameGui.Fonts[Font];
                Vector2 stringSize = font.MeasureString(textToRender);
                int x = mDestinationRectangle.X + (mDestinationRectangle.Width / 2);
                int y = mDestinationRectangle.Y + (mDestinationRectangle.Height / 2) + YTextOffset;

                spriteBatch.DrawString(font, textToRender, new Vector2(x, y), TextColor, 0, stringSize / 2, 1f, SpriteEffects.None, 0);
            }
        }

        private static string CounterToString(int counter)
        {
            return counter.ToString();
        }



        public void Decrease()
        {
            Counter--;
        }
        public void Increase()
        {
            Counter++;
        }


        public void SetCounterBounds(int min, int max)
        {
            CounterMin = min;
            CounterMax = max;
            Counter = min;
        }

        public override void PropagateMouseMovement(Point mousePosition)
        {
            base.PropagateMouseMovement(mousePosition);
            if (Contains(mousePosition))
            {
                IsSelected = true;
            }
            else
            {
                IsSelected = false;
            }
        }

        private bool Contains(Point mousePosition)
        {
            Vector2 pos = GetScreenOffset();
            Vector2 size = GetAbsoluteSize();
            Rectangle absoluteRect = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
            return absoluteRect.Contains(mousePosition);
        }

    }
}
