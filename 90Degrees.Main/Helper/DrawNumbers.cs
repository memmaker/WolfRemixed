using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAHelper
{
    public static class DrawNumbers
    {

        #region DrawGarbageFreeInt
        #region DrawIntXX Fields
        /// <summary>
        /// Draws a int or long without generating garbage ( = w/o using .ToString() )
        /// </summary>
        
        private static string[] digits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        private static string[] intCharBuffer = new string[10];
        private static float[] intXPositionBuffer = new float[10];
        private static readonly string intMinValue = Int32.MinValue.ToString(CultureInfo.InvariantCulture);


        private static string[] longCharBuffer = new string[20];
        private static float[] longXPositionBuffer = new float[20];
        private static readonly string longMinValue = Int64.MinValue.ToString(CultureInfo.InvariantCulture);
        #endregion

        #region DrawInt32
        /// <summary>  
        /// Extension method for SpriteBatch that draws an integer without allocating  
        /// any memory. This function avoids garbage collections that are normally caused  
        /// by calling Int32.ToString or String.Format.  
        /// </summary>  
        /// <param name="spriteBatch">The SpriteBatch instance whose DrawString method will be invoked.</param>  
        /// <param name="spriteFont">The SpriteFont to draw the integer value with.</param>  
        /// <param name="value">The integer value to draw.</param>  
        /// <param name="position">The screen position specifying where to draw the value.</param>  
        /// <param name="color">The color of the text drawn.</param>  
        /// <returns>The next position on the line to draw text. This value uses position.Y and position.X plus the equivalent of calling spriteFont.MeasureString on value.ToString(CultureInfo.InvariantCulture).</returns>  
        public static Vector2 DrawInt32(
            this SpriteBatch spriteBatch,
            SpriteFont spriteFont,
            int value,
            Vector2 position,
            Color color,
            float layerDepth)
        {
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }

            if (spriteFont == null)
            {
                throw new ArgumentNullException("spriteFont");
            }

            Vector2 nextPosition = position;

            if (value == Int32.MinValue)
            {
                nextPosition.X = nextPosition.X + spriteFont.MeasureString(intMinValue).X;
                spriteBatch.DrawString(spriteFont, intMinValue, position, color);
                position = nextPosition;
            }
            else
            {
                if (value < 0)
                {
                    nextPosition.X = nextPosition.X + spriteFont.MeasureString("-").X;
                    spriteBatch.DrawString(spriteFont, "-", position, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
                    value = -value;
                    position = nextPosition;
                }

                int index = 0;

                do
                {
                    int modulus = value % 10;
                    value = value / 10;

                    intCharBuffer[index] = digits[modulus];
                    intXPositionBuffer[index] = spriteFont.MeasureString(digits[modulus]).X;
                    index += 1;
                } while (value > 0);

                for (int i = index - 1; i >= 0; --i)
                {
                    nextPosition.X = nextPosition.X + intXPositionBuffer[i];
                    spriteBatch.DrawString(spriteFont, intCharBuffer[i], position, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
                    position = nextPosition;
                }
            }
            return position;
        }
        #endregion

        #region DrawInt64
        /// <summary>  
        /// Extension method for SpriteBatch that draws a long integer without allocating  
        /// any memory. This function avoids garbage collections that are normally caused  
        /// by calling Int64.ToString or String.Format.  
        /// </summary>  
        /// <param name="spriteBatch">The SpriteBatch instance whose DrawString method will be invoked.</param>  
        /// <param name="spriteFont">The SpriteFont to draw the integer value with.</param>  
        /// <param name="value">The long value to draw.</param>  
        /// <param name="position">The screen position specifying where to draw the value.</param>  
        /// <param name="color">The color of the text drawn.</param>  
        /// <returns>The next position on the line to draw text. This value uses position.Y and position.X plus the equivalent of calling spriteFont.MeasureString on value.ToString(CultureInfo.InvariantCulture).</returns>  
        public static Vector2 DrawInt64(
            this SpriteBatch spriteBatch,
            SpriteFont spriteFont,
            long value,
            Vector2 position,
            Color color,
            float layerDepth)
        {
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }

            if (spriteFont == null)
            {
                throw new ArgumentNullException("spriteFont");
            }

            Vector2 nextPosition = position;

            if (value == Int64.MinValue)
            {
                nextPosition.X = nextPosition.X + spriteFont.MeasureString(longMinValue).X;
                spriteBatch.DrawString(spriteFont, longMinValue, position, color);
                position = nextPosition;
            }
            else
            {
                if (value < 0)
                {
                    nextPosition.X = nextPosition.X + spriteFont.MeasureString("-").X;
                    spriteBatch.DrawString(spriteFont, "-", position, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
                    value = -value;
                    position = nextPosition;
                }

                int index = 0;

                do
                {
                    long modulus = value % 10;
                    value = value / 10;

                    longCharBuffer[index] = digits[modulus];
                    longXPositionBuffer[index] = spriteFont.MeasureString(digits[modulus]).X;
                    index += 1;
                } while (value > 0);

                for (int i = index - 1; i >= 0; --i)
                {
                    nextPosition.X += longXPositionBuffer[i];
                    spriteBatch.DrawString(spriteFont, longCharBuffer[i], position, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
                    position = nextPosition;
                }
            }
            return position;
        }
        #endregion
        #endregion

        #region Shadowed String
        /// <summary>
        /// Draws shadowed string
        /// </summary>
        public static void DrawShadowedString(this SpriteBatch sb, SpriteFont font, string text, Vector2 position)
        {
            sb.DrawString(font, text, position, Color.Black, 0f, font.MeasureString(text) / 2f, 1.05f, SpriteEffects.None, 1f); // BLACK
            sb.DrawString(font, text, position, Color.White, 0f, font.MeasureString(text) / 2f, 1f, SpriteEffects.None, 1f); // WHITE
        }
        #endregion
    }
}
