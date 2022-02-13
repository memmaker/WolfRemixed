using Microsoft.Xna.Framework;
using XNAHelper;

namespace XNAGameGui.Gui
{
    public class HudIcon
    {
        public SpriteSheet SourceSheet { get; private set; }
        public int FrameIndex { get; set; }
        public Vector2 ScreenPosition { get; set; }
        public float Scale { get; set; }
        public HudIcon(Vector2 screenPos, SpriteSheet spriteSheet, int startFrameIndex)
        {
            SourceSheet = spriteSheet;
            FrameIndex = startFrameIndex;
            ScreenPosition = screenPos;
            Scale = 1f;
        }
    }
}
