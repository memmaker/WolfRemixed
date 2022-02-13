using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace XNAHelper
{
    public struct Line
    {
        public Line(float x1, float y1, float x2, float y2) : this()
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
    }
    public static class DebugDrawer
    {
        private static SpriteBatch sSpriteBatch;
        private static List<Circle> mBufferedCircles;
        private static Dictionary<Rectangle, Color> mBufferedRectangles;
        private static Dictionary<Line, Color> mBufferedLines;
        private static List<string> mBufferedText;
        private static SpriteFont sDebugFont;

        public static void Init(SpriteBatch spriteBatch, SpriteFont font)
        {
            mBufferedCircles = new List<Circle>();
            mBufferedRectangles = new Dictionary<Rectangle, Color>();
            mBufferedLines = new Dictionary<Line, Color>();
            mBufferedText = new List<string>();
            sDebugFont = font;
            sSpriteBatch = spriteBatch;
        }

        public static void DrawString(string msg)
        {
            mBufferedText.Add(msg);
        }

        public static void DrawRectangle(Rectangle rect)
        {
            mBufferedRectangles[rect] = Color.Green;
        }

        public static void DrawRectangle(Rectangle rect, Color color)
        {
            mBufferedRectangles[rect] = color;
        }

        public static void DrawLine(Line line, Color color)
        {
            mBufferedLines[line] = color;
        }

        public static void DrawCircle(Circle circle)
        {
            mBufferedCircles.Add(circle);
        }

        public static void Draw()
        {
            sSpriteBatch.Begin();
            foreach (KeyValuePair<Rectangle, Color> keyValuePair in mBufferedRectangles)
            {
                DeferredDrawRectangle(keyValuePair.Key, keyValuePair.Value);
            }
            mBufferedRectangles.Clear();
            foreach (Circle circle in mBufferedCircles)
            {
                DeferredDrawCircle(circle);
            }
            mBufferedCircles.Clear();
            foreach (KeyValuePair<Line, Color> keyValuePair in mBufferedLines)
            {
                DeferredDrawLine(keyValuePair.Key, keyValuePair.Value);
            }
            mBufferedLines.Clear();

            int index = 0;
            foreach (string text in mBufferedText)
            {
                DeferredDrawString(text, index);
                index++;
            }
            mBufferedText.Clear();

            sSpriteBatch.End();
        }

        private static void DeferredDrawString(string text, int index)
        {
            Vector2 measureString = sDebugFont.MeasureString(text);
            Vector2 origin = new Vector2(sSpriteBatch.GraphicsDevice.Viewport.Width / 2, 0);
            Vector2 pos = origin + (Vector2.UnitY * index * sDebugFont.LineSpacing);
            sSpriteBatch.DrawString(sDebugFont, text, pos, Color.White);
        }

        private static void DeferredDrawLine(Line line, Color color)
        {
            sSpriteBatch.DrawLine(line.X1, line.Y1, line.X2, line.Y2, color, 2);
        }

        private static void DeferredDrawRectangle(Rectangle rect, Color color)
        {
            sSpriteBatch.DrawRectangle(rect, color, 2);
        }

        private static void DeferredDrawCircle(Circle circle)
        {
            Vector2 screenpos = circle.Center;
            sSpriteBatch.DrawCircle(screenpos, circle.Radius, 16, Color.Green);
        }
    }
}