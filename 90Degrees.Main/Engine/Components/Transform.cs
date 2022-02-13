using Artemis.Interface;
using Microsoft.Xna.Framework;
using System;
using XNAHelper;

namespace Twengine.Components
{
    public class Transform : IComponent
    {
        private Vector2 mPosition;
        private float mRotation;
        public event EventHandler<EventArgs> BeginInvisibility;
        public event EventHandler<EventArgs> EndInvisibility;
        public float MaxSpeed { get; set; }
        public Transform() { }

        public Transform(int x, int y, float rotation) : this(new Vector2(x, y), rotation)
        {
        }

        public Transform(Vector2 position, float rotation)
        {
            Rotation = rotation;
            mPosition = position;
            OldPosition = mPosition;
            IsRotationDependentOnVelocity = false;
            mIsVisible = true;
        }

        public Vector2 Position { get { return mPosition; } set { mPosition = value; } }
        public Vector2 OldPosition { get; set; }

        public float X
        {
            get { return mPosition.X; }
            set { mPosition.X = value; }
        }

        public float Y
        {
            get { return mPosition.Y; }
            set { mPosition.Y = value; }
        }

        public void SetLocation(float x, float y)
        {
            this.mPosition.X = x;
            this.mPosition.Y = y;
        }

        /// <summary>
        /// In Radians..
        /// </summary>
        public float Rotation
        {
            get { return mRotation; }
            set
            {
                mRotation = value;
                Forward = Vector2.Normalize(TwenMath.RotationToDirectionVector(mRotation));
                //Forward.Normalize();
            }
        }

        public Vector2 Forward { get; set; }

        public float RotationInDegrees { get { return MathHelper.ToDegrees(mRotation); } }

        public bool IsRotationDependentOnVelocity { get; set; }

        public Point LastCellPosition { get; set; }

        public bool CollideWithMap { get; set; }
        public bool CollideWithEntityMap { get; set; }

        private bool mIsVisible;
        public bool IsVisible
        {
            get { return mIsVisible; }
            set
            {
                mIsVisible = value;
                if (mIsVisible)
                    OnEndInvisibility();
                else
                    OnBeginInvisibility();
            }
        }



        private void OnEndInvisibility()
        {
            if (EndInvisibility == null) return;
            EndInvisibility(this, new EventArgs());
        }

        private void OnBeginInvisibility()
        {
            if (BeginInvisibility == null) return;
            BeginInvisibility(this, new EventArgs());
        }


        /// <summary>
        /// In degrees..
        /// </summary>
        /// <param name="angle"></param>
        public void AddRotation(float angle)
        {
            Rotation += MathHelper.ToRadians(angle);
        }

        public float DistanceTo(Transform t)
        {
            return Vector2.Distance(new Vector2(X, Y), new Vector2(t.X, t.Y));
        }
    }
}