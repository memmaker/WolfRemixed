using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Helper;
using XNAHelper;


namespace Twengine.SubSystems
{
    public class KeyboardControlSystem : EntityProcessingSystem
    {
        protected ComponentMapper<Physics> mVelocityMapper;
        protected ComponentMapper<KeyboardControl> mMovementControlMapper;
        protected ComponentMapper<Transform> mTransformMapper;
        private KeyboardState mKeyboardState;

        public KeyboardControlSystem()
            : base(typeof(KeyboardControl), typeof(Physics), typeof(Transform))
        {

        }

        public override void Initialize()
        {
            mVelocityMapper = new ComponentMapper<Physics>(world);
            mMovementControlMapper = new ComponentMapper<KeyboardControl>(world);
            mTransformMapper = new ComponentMapper<Transform>(world);
        }

        protected override void Begin()
        {
            base.Begin();
            mKeyboardState = Keyboard.GetState();
        }

        public override void Process(Entity e)
        {
            Physics physics = mVelocityMapper.Get(e);
            Transform transform = mTransformMapper.Get(e);
            KeyboardControl movementControl = mMovementControlMapper.Get(e);


            Rotate(movementControl, transform);
            Move(movementControl, physics);
        }

        private bool IsFiring(Keys Firekey)
        {
            return mKeyboardState.IsKeyDown(Firekey);
        }


        private void Move(KeyboardControl keyboardControl, Physics physics)
        {
            Vector2 moveDir = Vector2.Zero;

            if (mKeyboardState.IsKeyDown(keyboardControl.MoveLeft))
            {
                moveDir += -Vector2.UnitX;
            }
            if (mKeyboardState.IsKeyDown(keyboardControl.MoveRight))
            {
                moveDir += Vector2.UnitX;
            }
            if (mKeyboardState.IsKeyDown(keyboardControl.MoveUp))
            {
                moveDir += -Vector2.UnitY;
            }
            if (mKeyboardState.IsKeyDown(keyboardControl.MoveDown))
            {
                moveDir += Vector2.UnitY;
            }

            physics.IntendedMovementDirection = moveDir;
        }

        private void Rotate(KeyboardControl padNumber, Transform transform)
        {
            Vector2 lookDir = Vector2.Zero;
            if (lookDir != Vector2.Zero)
            {
                transform.Rotation = TwenMath.DirectionVectorToRotation(lookDir);
            }
        }


        private Vector2 ExtendDeadZone(Vector2 inputVal)
        {
            if (TwenMath.IsAtPosition(Vector2.Zero, inputVal, 0.2f))
            {
                return Vector2.Zero;
            }
            return inputVal;
        }

        private static float GetNormalizedVal(int value)
        {
            return MathHelper.Clamp((value / (float)short.MaxValue) - 1, -1f, 1f);
        }
    }
}
