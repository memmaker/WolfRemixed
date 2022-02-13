using Microsoft.Xna.Framework;
using XNAGameGui.Gui.Widgets;

namespace raycaster.StoryTelling
{
    public class ShowTextEvent : StoryTellingEvent
    {
        private string mText;
        private LabelWidget mLabelWidget;
        private float mFractionalWidth;
        private float mFractionalHeigth;
        private bool mCentered;

        public ShowTextEvent(string text, float fractionalWidth, float fractionalHeigth, bool centered)
        {
            mText = text;
            mFractionalWidth = fractionalWidth;
            mFractionalHeigth = fractionalHeigth;
            mCentered = centered;
        }

        public override bool IsFinished()
        {
            return false;
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Begin()
        {
            mLabelWidget = RaycastGame.CreateTextWidget(mText, mFractionalWidth, mFractionalHeigth);
            mLabelWidget.IsTextCentered = mCentered;
        }

        public override void End()
        {
            mLabelWidget.Destroy();
        }

        public override void Reset()
        {

        }
    }
}
