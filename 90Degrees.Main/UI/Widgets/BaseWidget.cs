using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace XNAGameGui.Gui.Widgets
{
    public class BaseWidget
    {
        protected List<BaseWidget> mChildren;
        public virtual UniRectangle Bounds { get; set; }
        public BaseWidget Parent { get; set; }


        public BaseWidget()
        {
            mChildren = new List<BaseWidget>();
        }

        public void AddChild(BaseWidget widget)
        {
            mChildren.Add(widget);
            widget.Parent = this;
            widget.PropagateResolutionChange();
        }

        public void RemoveChild(BaseWidget widget)
        {
            mChildren.Remove(widget);
        }
        public virtual void Draw(SpriteBatch spriteBatch, GameGui gui)
        {
            foreach (BaseWidget child in mChildren)
            {
                child.Draw(spriteBatch, gui);
            }
        }

        protected Vector2 GetAbsoluteSize()
        {
            if (Parent == null)
                return new Vector2(Bounds.Size.X.Offset, Bounds.Size.Y.Offset);

            Vector2 parentAbsolutSize = Parent.GetAbsoluteSize();
            return new Vector2((Bounds.Size.X.Fraction * parentAbsolutSize.X) + Bounds.Size.X.Offset, (Bounds.Size.Y.Fraction * parentAbsolutSize.Y) + Bounds.Size.Y.Offset);
        }

        protected Vector2 GetScreenOffset()
        {
            Vector2 parentOffset = Parent != null ? Parent.GetScreenOffset() : Vector2.Zero;
            Vector2 absoluteParentSize = Parent != null ? Parent.GetAbsoluteSize() : Vector2.Zero;

            Vector2 offset = new Vector2(parentOffset.X + (absoluteParentSize.X * Bounds.Location.X.Fraction) + Bounds.Location.X.Offset,
                                         parentOffset.Y + (absoluteParentSize.Y * Bounds.Location.Y.Fraction) + Bounds.Location.Y.Offset);
            return offset;
        }


        public virtual void PropagateResolutionChange()
        {
            foreach (BaseWidget child in mChildren)
            {
                child.PropagateResolutionChange();
            }
        }

        public virtual void PropagateMouseMovement(Point mousePosition)
        {
            foreach (BaseWidget child in mChildren)
            {
                child.PropagateMouseMovement(mousePosition);
            }
        }

        public void Destroy()
        {
            Parent.RemoveChild(this);
            Parent = null;
            for (int i = mChildren.Count - 1; i >= 0; i--)
            {
                mChildren[i].Destroy();
            }
            mChildren.Clear();
        }


    }

}
