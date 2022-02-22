using IndependentResolutionRendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using raycaster;
using XNAGameGui.Gui.Widgets;

namespace XNAGameGui.Gui
{
    public enum GameFont
    {
        Gui,
        LongTexts
    }
    public struct UniScalar
    {
        public UniScalar(float fraction, float offset)
            : this()
        {
            Offset = offset;
            Fraction = fraction;
        }

        public float Fraction { get; set; }
        public float Offset { get; set; }
    }
    public struct UniVector
    {

        public UniVector(float x, float y)
            : this()
        {
            X = new UniScalar(0, x);
            Y = new UniScalar(0, y);
        }
        public UniVector(UniScalar x, UniScalar y)
            : this()
        {
            X = x;
            Y = y;
        }

        public UniScalar X { get; set; }
        public UniScalar Y { get; set; }
    }
    public struct UniRectangle
    {
        public UniRectangle(float x, float y, float width, float height)
            : this()
        {
            Location = new UniVector(x, y);
            Size = new UniVector(width, height);
        }
        public UniRectangle(UniScalar x, UniScalar y, float width, float height)
            : this()
        {
            Location = new UniVector(x, y);
            Size = new UniVector(width, height);
        }

        public UniRectangle(float x, float y, UniScalar width, UniScalar height)
            : this()
        {
            Location = new UniVector(x, y);
            Size = new UniVector(width, height);
        }

        public UniRectangle(UniScalar x, UniScalar y, UniScalar width, UniScalar height)
            : this()
        {
            Location = new UniVector(x, y);
            Size = new UniVector(width, height);
        }

        public UniVector Size { get; set; }
        public UniVector Location { get; set; }
    }
    public class GameGui
    {
        public static BaseWidget RootWidget { get; set; }
        public static Texture2D WhiteRectangle { get; private set; }
        private static Viewport mViewport;
        public static Dictionary<GameFont, SpriteFont> Fonts { get; private set; }

        public static Viewport Viewport
        {
            get { return mViewport; }
            set
            {
                mViewport = value;
                RootWidget.Bounds = new UniRectangle(0, 0, mViewport.Width, mViewport.Height);
                RootWidget.PropagateResolutionChange();
            }
        }

        public static RenderTarget2D GuiLayer;

        public GameGui()
        {
            RootWidget = new BaseWidget();
            Fonts = new Dictionary<GameFont, SpriteFont>();
            
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SpriteFont guiFont, SpriteFont longTextFont)
        {
            WhiteRectangle = new Texture2D(graphicsDevice, 1, 1);
            WhiteRectangle.SetData(new[] { Color.White });
            Fonts[GameFont.Gui] = guiFont;
            Fonts[GameFont.LongTexts] = longTextFont;
        }

        public void MouseMoved(Point mousePosition)
        {
            RootWidget.PropagateMouseMovement(mousePosition);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Resolution.getTransformationMatrix());
            RootWidget.Draw(spriteBatch, this);
            spriteBatch.End();
        }

        public void UnloadContent()
        {
            if (WhiteRectangle != null)
                WhiteRectangle.Dispose();
        }

    }
}
