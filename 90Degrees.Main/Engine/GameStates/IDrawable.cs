using Microsoft.Xna.Framework;

namespace Engine.GameStates
{
    internal interface IDrawable
    {
        bool get_Visible();
        void Draw(GameTime gameTime);
    }
}