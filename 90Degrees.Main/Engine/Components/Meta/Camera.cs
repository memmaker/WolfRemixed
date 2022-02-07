using System;
using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace Twengine.Components.Meta
{
    public class Camera : IComponent
    {
        public int EyeHeight { get; set; }
        public Vector2 Position { get; set; }
        private Vector2 mDirection;
        public Vector2 Direction
        {
            get { return mDirection; }
            set
            {
                mDirection = Vector2.Normalize(value);
                SetProjectionPlaneFromFov(FoV);
            }
        }

        public Vector2 ProjectionPlane { get; set; }
        public float FoV { get; set; }

        /// <summary>
        /// Creates a new 2D camera
        /// </summary>
        /// <param name="position">Position of camera</param>
        /// <param name="fov">Field of view (in degree)</param>
        /// <param name="viewDirection">Direction the camera views</param>
        public Camera(Vector2 position, float fov, Vector2 viewDirection)
        {
            EyeHeight = 0;
            FoV = fov;

            Position = position;

            Direction = Vector2.Normalize(viewDirection);

            // must be orthogonal to the direction vector, length determines fov
            SetProjectionPlaneFromFov(fov);
        }

        private void SetProjectionPlaneFromFov(float fov)
        {
            float camwidth = (float)Math.Tan(MathHelper.ToRadians(fov) / 2);
            ProjectionPlane = Vector2.Normalize(new Vector2(-Direction.Y, Direction.X)) * camwidth;
        }

        public void Rotate(float angle)
        {
            Matrix rotationMatrix = Matrix.CreateRotationZ(angle);
            Direction = Vector2.Transform(Direction, rotationMatrix);
            ProjectionPlane = Vector2.Transform(ProjectionPlane, rotationMatrix);
        }

        public void ChangeFov(float fovInDegree)
        {
            FoV = fovInDegree;
            SetProjectionPlaneFromFov(fovInDegree);
        }
    }
}
