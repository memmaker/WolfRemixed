using Microsoft.Xna.Framework;

namespace Engine.GameStates
{
    internal interface IUpdateable
    {
        bool Enabled { get; }
        void Update(GameTime gameTime);
    }
}