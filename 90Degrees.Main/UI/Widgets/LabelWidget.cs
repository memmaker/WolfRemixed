using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace XNAGameGui.Gui.Widgets
{
    public class LabelWidget : BaseWidget
    {
        private string mText;
        public string Text
        {
            get { return mText; }
            set
            {
                mText = value;
                if (value != "" && mDestinationRectangle.Width > 0)
                    WrapText();
            }
        }
        public Dictionary<string, HudIcon> Icons { get; private set; }
        public bool DrawLabelBackground { get; set; }
        public bool DrawBackgroundShadow { get; set; }
        public Color LabelColor { get; set; }
        public Color TextColor { get; set; }
        public Texture2D Background { get; set; }
        public GameFont Font { get; set; }
        public bool IsVisible { get; set; }
        public bool IsTextCentered { get; set; }
        public bool IsSizeDependingOnContent { get; set; }
        public int BorderSize { get; set; }
        protected Rectangle mDestinationRectangle;
        private string[] mLines;
        public int MaxLines { get; set; }
        public override UniRectangle Bounds
        {
            get { return base.Bounds; }
            set
            {
                base.Bounds = value;
                UpdatePosition();
            }
        }

        public LabelWidget() : this("")
        {

        }

        public LabelWidget(string text)
        {
            Text = text;
            LabelColor = Color.ForestGreen;
            TextColor = Color.WhiteSmoke;
            IsVisible = true;
            BorderSize = 0;
            IsTextCentered = true;
            Font = GameFont.Gui;
            MaxLines = int.MaxValue;
            Icons = new Dictionary<string, HudIcon>();
        }


        private void UpdatePosition()
        {
            mDestinationRectangle = CalculateDestinationRectangle();
            if (mText != "")
                WrapText();
        }

        public override void Draw(SpriteBatch spriteBatch, GameGui gui)
        {
            if (!IsVisible) return;

            if (DrawLabelBackground)
            {
                if (DrawBackgroundShadow)
                {
                    Rectangle shadowRect = new Rectangle(mDestinationRectangle.X - 3, mDestinationRectangle.Y - 4, mDestinationRectangle.Width, mDestinationRectangle.Height);
                    spriteBatch.Draw(GameGui.WhiteRectangle, shadowRect, Color.Black);
                }
                if (Background != null)
                {
                    spriteBatch.Draw(Background, mDestinationRectangle, Color.White);
                }
                else
                {
                    spriteBatch.Draw(GameGui.WhiteRectangle, mDestinationRectangle, LabelColor);
                }

            }

            foreach (KeyValuePair<string, HudIcon> keyValuePair in Icons)
            {
                HudIcon hudIcon = keyValuePair.Value;
                spriteBatch.Draw(hudIcon.SourceSheet.Texture, hudIcon.ScreenPosition,
                                 hudIcon.SourceSheet.GetSourceRectByIndex(hudIcon.FrameIndex), Color.White, 0f, Vector2.Zero, hudIcon.Scale, SpriteEffects.None, 0f);

            }

            if (Text != "")
            {

                float yOffset = 0;
                int startIndex = 0;
                int length = mLines.Length;
                if (length > MaxLines)
                {
                    startIndex = length - MaxLines;
                }
                for (int index = startIndex; index < mLines.Length; index++)
                {
                    string line = mLines[index];
                    Vector2 stringSize = GameGui.Fonts[Font].MeasureString(line);
                    if (IsTextCentered)
                        DrawLineOfTextCentered(spriteBatch, GameGui.Fonts[Font], line, stringSize, yOffset);
                    else
                        DrawLineOfTextLeftAligned(spriteBatch, GameGui.Fonts[Font], line, stringSize, yOffset);
                    yOffset += stringSize.Y;
                }
            }

            base.Draw(spriteBatch, gui);
        }
        private void DrawLineOfTextCentered(SpriteBatch spriteBatch, SpriteFont font, string line, Vector2 stringSize, float yOffset)
        {
            float x = mDestinationRectangle.X + (mDestinationRectangle.Width / 2);
            float y = mDestinationRectangle.Y + BorderSize + (stringSize.Y / 2) + yOffset;

            spriteBatch.DrawString(font, line, new Vector2(x, y), TextColor, 0, stringSize / 2, 1f, SpriteEffects.None, 0);
        }

        private void DrawLineOfTextLeftAligned(SpriteBatch spriteBatch, SpriteFont font, string line, Vector2 stringSize, float yOffset)
        {
            float x = mDestinationRectangle.X + BorderSize;
            float y = mDestinationRectangle.Y + BorderSize + yOffset;

            spriteBatch.DrawString(font, line, new Vector2(x, y), TextColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        private void WrapText()
        {
            mText = mText.Replace("\n\n", "\n \n");
            SpriteFont font = GameGui.Fonts[Font];
            Vector2 stringSize = font.MeasureString(mText);

            int previousIndex = -1;

            while (stringSize.X > mDestinationRectangle.Width - (2 * BorderSize))
            {
                if (previousIndex != -1)
                    mText = ReplaceAtPosition(mText, previousIndex, '_');

                int lastIndexOf = Text.LastIndexOf(' ');
                if (lastIndexOf != -1)
                {
                    mText = ReplaceAtPosition(mText, lastIndexOf, '\n');

                    stringSize = font.MeasureString(mText);

                    previousIndex = lastIndexOf;
                }
                else
                {
                    break;
                }
            }
            mText = Text.Replace('_', ' ');
            mLines = Text.Split('\n');
        }

        private string ReplaceAtPosition(string text, int index, char replacement)
        {
            string result = text.Remove(index, 1);
            return result.Insert(index, replacement.ToString());
        }

        private Rectangle CalculateDestinationRectangle()
        {
            Vector2 screenOffset = GetScreenOffset();

            int x = (int)screenOffset.X;
            int y = (int)screenOffset.Y;

            float parentXOffset = Parent != null ? Parent.Bounds.Size.X.Offset : 0;
            float parentYOffset = Parent != null ? Parent.Bounds.Size.Y.Offset : 0;

            int width = (int)((Bounds.Size.X.Fraction * parentXOffset) + Bounds.Size.X.Offset);
            int height = (int)((Bounds.Size.Y.Fraction * parentYOffset) + Bounds.Size.Y.Offset);

            if (IsSizeDependingOnContent)
            {
                SpriteFont spriteFont = GameGui.Fonts[Font];
                Vector2 contentSize = spriteFont.MeasureString(mText);
                int newWidth = (int)(contentSize.X + (BorderSize * 2));
                int newHeight = (int)(contentSize.Y + (BorderSize * 3));
                int xdiff = width - newWidth;
                int ydiff = height - newHeight;

                width = newWidth;
                height = newHeight;
                x += xdiff / 2;
                y += ydiff / 2;
            }

            Rectangle destinationRectangle = new Rectangle(x, y, width, height);
            return destinationRectangle;
        }

        public override void PropagateResolutionChange()
        {
            UpdatePosition();
            base.PropagateResolutionChange();
        }

        public void AppendLine(string lineOfText)
        {
            Text += lineOfText;
        }
    }
}
