using System;
using Microsoft.Xna.Framework;
using Twengine.Constants;
using Twengine.GameObjects.Components;
using Twengine.Helper;
using Twengine.Objects;

namespace Twengine.GameObjects
{
    public delegate void GameObjectUpdateHandler(GameObject source);

    /// <summary>
    /// The Global Top Level State of a GameObject. Will get diversified later on.
    /// </summary>
    public enum GameObjectState : byte
    {
        Idle,
        Shooting,
        Colliding
    }

    public class GameObject
    {
        public event GameObjectUpdateHandler StateChanged;
        public event GameObjectUpdateHandler Moved;

        public virtual string Tag { get; set; }

        #region component based design
        public bool IsSprite { get { return Sprite != null; } }

        public bool IsCollider { get { return Collider != null; } }
        
        public ICollidable Collider { get; set; }

        public Sprite Sprite { get; set; }
        #endregion


        #region Position

        public Vector2 Position { get; set; }

        public Vector2 Forward { get; set; }

        private float mRotation;

        /// <summary>
        /// In radians.
        /// </summary>
        public float Rotation
        {
            get { return mRotation; }
            set
            {
                mRotation = value;
                mRotationMatrix = Matrix.CreateRotationZ(Rotation);
                Forward = Vector2.Transform(-Vector2.UnitY, mRotationMatrix);
                //Forward.Normalize();
            }
        }

        public float RotationDegree { get { return MathHelper.ToDegrees(Rotation); } set { Rotation = MathHelper.ToRadians(value); } }

        public Vector2 Velocity { get; set; }

        private Matrix mRotationMatrix;

        #endregion

        public bool IsDrawingBoundingBox { get; set; }

        public bool IsPlayerCharacter { get; set; }

        public bool IsMoving { get; private set; }

        public GameObjectState State { get; set; }

        public float Speed { get; set; }

        public GameObject()
        {
            Forward = -Vector2.UnitY;
            Rotation = 0f;
            Speed = 100f;
            State = GameObjectState.Idle;
            IsMoving = false;
        }

        public void Move(Vector2 directedVelocity)
        {
            if (Vector2.Zero.Equals(directedVelocity))
            {
                IsMoving = false;
                Stop();
            }
            else
            {
                Velocity = directedVelocity;
                IsMoving = true;
            }
            OnMovement();
        }

        public void MoveForward()
        {
            Move(Forward);
        }
        public void MoveTowards(Vector2 position)
        {
            RotateTo(position);
            Vector2 targetDirection = position - Position;
            targetDirection.Normalize();
            Move(targetDirection);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (IsMoving)
            {
                Position += Velocity * (float)(Speed * gameTime.ElapsedGameTime.TotalSeconds * TwengineGame.TimeDilationFactor);
            }
            if (IsCollider && IsDrawingBoundingBox)
            {
                if (Collider.UseRect)
                {
                    DebugDrawer.DrawRectangle(Collider.BoundingBox);
                }
                else
                {
                    DebugDrawer.DrawCircle(Collider.BoundingCircle);
                }
            }
        }

        public void Stop()
        {
            
        }


        public void UpdateState(GameObjectState gameObjectState)
        {
            State = gameObjectState;
            OnStateChange();
        }

        public void RotateTo(int x, int y)
        {
            RotateTo(new Vector2(x,y));
        }

        public void RotateTo(Vector2 pos)
        {
            Vector2 spritePos = new Vector2(Position.X, Position.Y);
            Rotation = TwenMath.RadianAngleBetween2DVectors(pos, spritePos);
        }

        public virtual void ExecuteGameCommand(GameObjectAction clientGameCommand)
        {
            GameObjectState oldState = State;
            switch (clientGameCommand)
            {
                case GameObjectAction.Fire:
                    State = GameObjectState.Shooting;
                    break;
                case GameObjectAction.StopFire:
                    State = GameObjectState.Idle;
                    break;
            }
            if (State != oldState)
            {
                OnStateChange();
            }
        }

        private void OnStateChange()
        {
            if (StateChanged != null)
            {
                StateChanged(this);
            }
        }

        private void OnMovement()
        {
            if (Moved != null)
            {
                Moved(this);
            }
        }
        
        #region component based design
        public void AttachSprite(Sprite sprite)
        {
            Sprite = sprite;
            sprite.ParentGameObject = this;
        }

        public void AttachCollider(ICollidable collider)
        {
            Collider = collider;
            collider.ParentGameObject = this;
        }
        #endregion

        public override string ToString()
        {
            return State.ToString();
        }

        public virtual void OnCollide(ICollidable collider)
        {
            //Sprite.FlashForOneSecond();
        }

        protected void BumpAway(ICollidable collider)
        {
            Vector2 awayDir = Position - collider.Position;
            awayDir.Normalize();
            Move(awayDir);
        }
    }
}