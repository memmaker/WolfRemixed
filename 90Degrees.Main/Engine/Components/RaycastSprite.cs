using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Twengine.Managers;
using XNAHelper;

namespace Twengine.Components
{
    public enum Orientation
    {
        None,
        Horizontal,
        Vertical
    }
    public class RaycastSprite : Sprite
    {
        public Color DrawColor { get; set; }
        public bool IsFlashing { get; set; }

        public RaycastSprite(SpriteSheet sheet, int index)
            : base(sheet, index)
        {
            Stripes = new List<SpriteStripe>();
            IsFlashing = false;

        }

        public List<SpriteStripe> Stripes { get; set; }

        public void FlashColor(Color color)
        {
            DrawColor = color;
            IsFlashing = true;
        }


    }
}