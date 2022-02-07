using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Text;
using xTile.Dimensions;
using xTile.Display;
using xTile.Tiles;
using Rectangle = xTile.Dimensions.Rectangle;

namespace Engine.Tide
{
    public class XnaDisplayDevice : IDisplayDevice, IDisposable
    {
        private ContentManager m_contentManager;
        private GraphicsDevice m_graphicsDevice;
        private SpriteBatch m_spriteBatchAlpha;
        private SpriteBatch m_spriteBatchAdditive;
        private Dictionary<TileSheet, Texture2D> m_tileSheetTextures;
        private Vector2 m_tilePosition;
        private Microsoft.Xna.Framework.Rectangle m_sourceRectangle;
        private Color m_modulationColour;

        public Color ModulationColour
        {
            get => this.m_modulationColour;
            set => this.m_modulationColour = value;
        }

        public SpriteBatch SpriteBatchAlpha => this.m_spriteBatchAlpha;

        public SpriteBatch SpriteBatchAdditive => this.m_spriteBatchAdditive;

        public XnaDisplayDevice(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            this.m_contentManager = contentManager;
            this.m_graphicsDevice = graphicsDevice;
            this.m_spriteBatchAlpha = new SpriteBatch(graphicsDevice);
            this.m_spriteBatchAdditive = new SpriteBatch(graphicsDevice);
            this.m_tileSheetTextures = new Dictionary<TileSheet, Texture2D>();
            this.m_tilePosition = new Vector2();
            this.m_sourceRectangle = new Microsoft.Xna.Framework.Rectangle();
            this.m_modulationColour = Color.White;
        }

        ~XnaDisplayDevice() => this.Dispose(false);

        public void LoadTileSheet(TileSheet tileSheet)
        {
            Texture2D texture2D = this.m_contentManager.Load<Texture2D>(tileSheet.ImageSource);
            this.m_tileSheetTextures[tileSheet] = texture2D;
        }

        public void DisposeTileSheet(TileSheet tileSheet)
        {
            if (!this.m_tileSheetTextures.ContainsKey(tileSheet))
                return;
            this.m_tileSheetTextures.Remove(tileSheet);
        }

        public void BeginScene()
        {
            this.m_spriteBatchAlpha.Begin((SpriteSortMode)0, BlendState.AlphaBlend);
            this.m_spriteBatchAdditive.Begin((SpriteSortMode)0, BlendState.Additive);
        }

        public void SetClippingRegion(Rectangle clippingRegion)
        {
            int backBufferWidth = this.m_graphicsDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = this.m_graphicsDevice.PresentationParameters.BackBufferHeight;
            int num1 = this.Clamp(clippingRegion.X, 0, backBufferWidth);
            int num2 = this.Clamp(clippingRegion.Y, 0, backBufferHeight);
            int num3 = this.Clamp(clippingRegion.X + clippingRegion.Width, 0, backBufferWidth);
            int num4 = this.Clamp(clippingRegion.Y + clippingRegion.Height, 0, backBufferHeight);
            int num5 = num3 - num1;
            int num6 = num4 - num2;
            this.m_graphicsDevice.Viewport = new Viewport(num1, num2, num5, num6);
        }

        public void DrawTile(Tile tile, Location location)
        {
            if (tile == null)
                return;
            SpriteBatch spriteBatch = tile.BlendMode == BlendMode.Alpha ? this.m_spriteBatchAlpha : this.m_spriteBatchAdditive;
            Rectangle tileImageBounds = tile.TileSheet.GetTileImageBounds(tile.TileIndex);
            Texture2D tileSheetTexture = this.m_tileSheetTextures[tile.TileSheet];
            this.m_tilePosition.X = (float)location.X;
            this.m_tilePosition.Y = (float)location.Y;
            this.m_sourceRectangle.X = tileImageBounds.X;
            this.m_sourceRectangle.Y = tileImageBounds.Y;
            this.m_sourceRectangle.Width = tileImageBounds.Width;
            this.m_sourceRectangle.Height = tileImageBounds.Height;
            spriteBatch.Draw(tileSheetTexture, this.m_tilePosition, new Microsoft.Xna.Framework.Rectangle?(m_sourceRectangle), this.m_modulationColour);
        }

        public void EndScene()
        {
            this.m_spriteBatchAlpha.End();
            this.m_spriteBatchAdditive.End();
        }

        private int Clamp(int nValue, int nMin, int nMax) => Math.Min(Math.Max(nValue, nMin), nMax);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (this.m_spriteBatchAlpha != null)
            {
                ((GraphicsResource)this.m_spriteBatchAlpha).Dispose();
                this.m_spriteBatchAlpha = (SpriteBatch)null;
            }
            if (this.m_spriteBatchAdditive != null)
            {
                ((GraphicsResource)this.m_spriteBatchAdditive).Dispose();
                this.m_spriteBatchAdditive = (SpriteBatch)null;
            }
            this.m_tileSheetTextures.Clear();
        }
    }
}
