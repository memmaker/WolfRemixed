using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace XNAHelper
{
    public class SpriteSheet
    {
        private Color[] mTexData;
        private int mNumberOfFrames;
        public Texture2D Texture { get; protected set; }

        public int FrameHeight { get; private set; }

        public int FrameWidth { get; private set; }

        public Rectangle[] TextureOpaqueRect { get; set; }

        public int NumberOfFrames
        {
            get { return mNumberOfFrames; }
        }

        public SpriteSheet(Texture2D texture, int frameWidth, int frameHeight)
        {
            Texture = texture;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            mNumberOfFrames = (int)Math.Floor((Texture.Width / (float)FrameWidth) * (Texture.Height / (float)FrameHeight));
            mTexData = new Color[Texture.Width * Texture.Height];

            ReadPixels(texture, mTexData);
            CreateOpaqueRectangles();
        }

        private static void ReadPixels(Texture2D texture, Color[] data)
        {
#if BLAZORGL
            // WebGL can't read textures back directly: draw into a render target and read that
            GraphicsDevice device = texture.GraphicsDevice;
            RenderTargetBinding[] previous = device.GetRenderTargets();
            using RenderTarget2D target = new RenderTarget2D(device, texture.Width, texture.Height);
            device.SetRenderTarget(target);
            device.Clear(Color.Transparent);
            using (SpriteBatch batch = new SpriteBatch(device))
            {
                batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
                batch.Draw(texture, Vector2.Zero, Color.White);
                batch.End();
            }
            device.SetRenderTargets(previous);
            target.GetData(data);
#else
            texture.GetData(data);
#endif
        }

        private void CreateOpaqueRectangles()
        {
            int amountOfFrames = (Texture.Width / FrameWidth) * (Texture.Height / FrameHeight);
            TextureOpaqueRect = new Rectangle[amountOfFrames];
            for (int i = 0; i < amountOfFrames; i++)
            {
                Rectangle sourceRectByIndex = GetSourceRectByIndex(i);

                Rectangle opaqueRect = ShrinkBounds(sourceRectByIndex);
                TextureOpaqueRect[i] = opaqueRect;
            }
        }
        private Rectangle ShrinkBounds(Rectangle sourceRectByIndex)
        {
            Rectangle shrunkRect = sourceRectByIndex;

            // from top
            for (int y = sourceRectByIndex.Top; y < sourceRectByIndex.Bottom; y++)
            {
                if (IsRowOpaque(sourceRectByIndex.Left, y))
                {
                    break;
                }
                // still transparent, shrink and continue
                shrunkRect.Height--;
                shrunkRect.Y++;
            }

            // from bottom
            for (int y = sourceRectByIndex.Bottom - 1; y >= sourceRectByIndex.Top; y--)
            {
                if (IsRowOpaque(sourceRectByIndex.Left, y))
                {
                    break;
                }
                // still transparent, shrink and continue
                shrunkRect.Height--;
            }

            // from left
            for (int x = sourceRectByIndex.Left; x < sourceRectByIndex.Right; x++)
            {
                if (IsColOpaque(x, sourceRectByIndex.Top))
                {
                    break;
                }
                // still transparent, shrink and continue
                shrunkRect.Width--;
                shrunkRect.X++;
            }

            // from right
            for (int x = sourceRectByIndex.Right - 1; x >= sourceRectByIndex.Left; x--)
            {
                if (IsColOpaque(x, sourceRectByIndex.Top))
                {
                    break;
                }
                // still transparent, shrink and continue
                shrunkRect.Width--;
            }
            return shrunkRect;
        }

        private bool IsColOpaque(int x, int startY)
        {
            for (int y = startY; y < startY + FrameHeight; y++)
            {
                if (mTexData[GetTextureIndexOf(x, y)].A != 0)
                {
                    // is opaque
                    return true;
                }
            }
            return false;
        }

        private bool IsRowOpaque(int startX, int y)
        {
            for (int x = startX; x < startX + FrameWidth; x++)
            {
                if (mTexData[GetTextureIndexOf(x, y)].A != 0)
                {
                    // is opaque
                    return true;
                }
            }
            return false;
        }


        private int GetTextureIndexOf(int x, int y)
        {
            return Texture.Width * y + x;
        }

        public Rectangle GetSourceRectByIndex(int index)
        {
            int framesX = Texture.Width / FrameWidth;
            int x = (index % framesX) * FrameWidth;
            int y = ((int)Math.Floor(index / (double)framesX)) * FrameHeight;
            return new Rectangle(x, y, FrameWidth, FrameHeight);
        }

        public Color[] GetFrameData(int index)
        {
            int framesX = Texture.Width / FrameWidth;
            int x = (index % framesX) * FrameWidth;
            int y = ((int)Math.Floor(index / (double)framesX)) * FrameHeight;

            Color[] data = new Color[FrameWidth * FrameHeight];
            for (int row = 0; row < FrameHeight; row++)
                Array.Copy(mTexData, (y + row) * Texture.Width + x, data, row * FrameWidth, FrameWidth);
            return data;
        }
    }
}
