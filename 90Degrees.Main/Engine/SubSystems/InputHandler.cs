using Artemis;
using Artemis.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Degrees.Main.Engine.Managers;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Managers;

namespace Twengine.SubSystems.Raycast
{
    public class ChangeWeaponEventArgs : EventArgs
    {
        public int WeaponIndex { get; set; }
    }
    public class InputHandler : EntityComponentProcessingSystem<FPSControl, Transform>
    {
        public event EventHandler<EventArgs> PlayerPressedFire;
        public event EventHandler<EventArgs> NextWeapon;
        public event EventHandler<ChangeWeaponEventArgs> ChangeWeapon;
        public event EventHandler<EventArgs> PlayerUsed;
        public event EventHandler<EventArgs> ToggleMap;
        public event EventHandler<EventArgs> MapZoomIn;
        public event EventHandler<EventArgs> MapZoomOut;
        public event EventHandler<EventArgs> PlayerMovedIntoTile;
        public event EventHandler<EventArgs> ActivatedWeaponCheat;
        private KeyboardState mKeyboardState;
        private KeyboardState mLastKeyboardState;
        
        private MouseState mMouseState;
        private int mScreenWidth;
        private int mScreenHeight;
        private Raycaster mRaycaster;
        private List<Keys> mKeyBuffer;
        public InputHandler(Raycaster raycaster, int screenWidth, int screenHeight)
            : base()
        {
            mScreenWidth = screenWidth;
            mScreenHeight = screenHeight;
            mRaycaster = raycaster;
            // constants..
            mLastKeyboardState = Keyboard.GetState();
            mKeyBuffer = new List<Keys>();
        }

        protected override void Begin()
        {
            base.Begin();
            mKeyboardState = Keyboard.GetState();
            mMouseState = Mouse.GetState();
        }
        protected override void End()
        {
            CenterMouse();
            mLastKeyboardState = mKeyboardState;
        }

        private void OnPlayerUsed()
        {
            if (PlayerUsed == null) return;
            PlayerUsed(this, new EventArgs());
        }
        private void OnPlayerFired()
        {
            if (PlayerPressedFire == null) return;
            PlayerPressedFire(this, new EventArgs());
        }

        private void OnPlayerMovedIntoTile()
        {
            if (PlayerMovedIntoTile == null) return;
            PlayerMovedIntoTile(this, new EventArgs());
        }

        public void CenterMouse()
        {
            Mouse.SetPosition(mScreenWidth / 2, mScreenHeight / 2);
        }
        public override void Process(Entity e, FPSControl movementControl, Transform transform)
        {
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            Rotate(movementControl, transform, deltaTimeInSeconds);
            Move(movementControl, transform, deltaTimeInSeconds);

            CheckFire();
            CheckUsed();
            CheckChangeWeapon();
            CheckNextWeapon(movementControl);
            CheckMap(movementControl);

            CheckForCheats(e);

            mRaycaster.SyncRaycasterCamToPlayer(transform);
        }

        private void CheckChangeWeapon()
        {
            if (mLastKeyboardState.IsKeyUp(Keys.D1) && mKeyboardState.IsKeyDown(Keys.D1))
            {
                OnPlayerChangeWeapon(1);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D2) && mKeyboardState.IsKeyDown(Keys.D2))
            {
                OnPlayerChangeWeapon(2);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D3) && mKeyboardState.IsKeyDown(Keys.D3))
            {
                OnPlayerChangeWeapon(3);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D4) && mKeyboardState.IsKeyDown(Keys.D4))
            {
                OnPlayerChangeWeapon(4);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D5) && mKeyboardState.IsKeyDown(Keys.D5))
            {
                OnPlayerChangeWeapon(5);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D6) && mKeyboardState.IsKeyDown(Keys.D6))
            {
                OnPlayerChangeWeapon(6);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D7) && mKeyboardState.IsKeyDown(Keys.D7))
            {
                OnPlayerChangeWeapon(7);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D8) && mKeyboardState.IsKeyDown(Keys.D8))
            {
                OnPlayerChangeWeapon(8);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D9) && mKeyboardState.IsKeyDown(Keys.D9))
            {
                OnPlayerChangeWeapon(9);
            }
            else if (mLastKeyboardState.IsKeyUp(Keys.D0) && mKeyboardState.IsKeyDown(Keys.D0))
            {
                OnPlayerChangeWeapon(0);
            }
        }

        private void CheckNextWeapon(FPSControl fpsControl)
        {
            if (mLastKeyboardState.IsKeyUp(fpsControl.NextWeapon) && mKeyboardState.IsKeyDown(fpsControl.NextWeapon))
            {
                OnPlayerNextWeapon();
            }
        }

        private void CheckForCheats(Entity controlledEntity)
        {
            foreach (Keys pressedKey in mKeyboardState.GetPressedKeys())
            {
                if (mLastKeyboardState.IsKeyUp(pressedKey))
                {
                    mKeyBuffer.Add(pressedKey);
                }
            }


            string bufferString = BuildString(mKeyBuffer);
            //DebugDrawer.DrawString("KeyBuffer: " + bufferString);
            if (bufferString.Contains("fxdqd"))
            {
                HealthPoints playerHp = controlledEntity.GetComponent<HealthPoints>();
                playerHp.IsImmortal = !playerHp.IsImmortal;
                mKeyBuffer.Clear();
            }
            if (bufferString.Contains("fxvanitar"))
            {
                Transform playerTransform = controlledEntity.GetComponent<Transform>();
                playerTransform.IsVisible = !playerTransform.IsVisible;
                mKeyBuffer.Clear();
            }
            if (bufferString.Contains("fxguns"))
            {
                OnPlayerActivatedWeaponCheat();
                mKeyBuffer.Clear();
            }
            if (mKeyBuffer.Count > 200)
                mKeyBuffer.Clear();
        }

        private string BuildString(List<Keys> keyBuffer)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Keys key in keyBuffer)
            {
                string s = Enum.GetName(typeof(Keys), key).ToLower();
                if (s.Length == 1)
                    sb.Append(s);
            }
            return sb.ToString();
        }
        private void CheckMap(FPSControl fpsControl)
        {
            if (mLastKeyboardState.IsKeyUp(fpsControl.ToggleMap) && mKeyboardState.IsKeyDown(fpsControl.ToggleMap))
            {
                OnPlayerToggleMap();
            }
            if (mLastKeyboardState.IsKeyUp(fpsControl.MapZoomIn) && mKeyboardState.IsKeyDown(fpsControl.MapZoomIn))
            {
                OnPlayerMapZoomIn();
            }
            if (mLastKeyboardState.IsKeyUp(fpsControl.MapZoomOut) && mKeyboardState.IsKeyDown(fpsControl.MapZoomOut))
            {
                OnPlayerMapZoomOut();
            }
        }
        private void OnPlayerActivatedWeaponCheat()
        {
            if (ActivatedWeaponCheat == null) return;
            ActivatedWeaponCheat(this, new EventArgs());
        }
        private void OnPlayerChangeWeapon(int weaponIndex)
        {
            if (ChangeWeapon == null) return;
            ChangeWeapon(this, new ChangeWeaponEventArgs() { WeaponIndex = weaponIndex });
        }

        private void OnPlayerNextWeapon()
        {
            if (NextWeapon == null) return;
            NextWeapon(this, new EventArgs());
        }

        private void OnPlayerMapZoomIn()
        {
            if (MapZoomIn == null) return;
            MapZoomIn(this, new EventArgs());
        }

        private void OnPlayerMapZoomOut()
        {
            if (MapZoomOut == null) return;
            MapZoomOut(this, new EventArgs());
        }

        private void OnPlayerToggleMap()
        {
            if (ToggleMap == null) return;
            ToggleMap(this, new EventArgs());
        }

        private void CheckFire()
        {
            if (mMouseState.LeftButton == ButtonState.Pressed)
            {
                OnPlayerFired();
            }
        }

        private void CheckUsed()
        {
            if (mMouseState.RightButton == ButtonState.Pressed)
            {
                OnPlayerUsed();
            }
        }


        private void Move(FPSControl fpsControl, Transform transform, float worldDelta)
        {
            Vector2 movementDir = Vector2.Zero;
            if (mKeyboardState.IsKeyDown(fpsControl.MoveForward))
            {
                movementDir += transform.Forward;
            }
            else if (mKeyboardState.IsKeyDown(fpsControl.MoveBackward))
            {
                movementDir -= transform.Forward;
            }

            if (!Settings.KeyboardOnly)
            {
                if (mKeyboardState.IsKeyDown(fpsControl.MoveRight))
                {
                    Matrix rotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(90));
                    Vector2 rightVector = Vector2.Transform(transform.Forward, rotationMatrix);
                    movementDir += rightVector;
                }
                else if (mKeyboardState.IsKeyDown(fpsControl.MoveLeft))
                {
                    Matrix rotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-90));
                    Vector2 leftVector = Vector2.Transform(transform.Forward, rotationMatrix);
                    movementDir += leftVector;
                }
            }

            transform.OldPosition = transform.Position;
            transform.Position += movementDir * (transform.MaxSpeed * worldDelta);

            if (transform.LastCellPosition != new Point((int)transform.Position.X, (int)transform.Position.Y))
                OnPlayerMovedIntoTile();
            //DebugDrawer.DrawString("Pos: " + transform.LastCellPosition);
        }

        private void Rotate(FPSControl fpsControl, Transform transform, float worldDelta)
        {
            float amount = 0.0f;
            if (Settings.KeyboardOnly)
            {
                if (mKeyboardState.IsKeyDown(fpsControl.MoveRight))
                {
                    Matrix rotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(90));
                    Vector2 rightVector = Vector2.Transform(transform.Forward, rotationMatrix);
                    amount = -3.2f;
                }
                else if (mKeyboardState.IsKeyDown(fpsControl.MoveLeft))
                {
                    Matrix rotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-90));
                    Vector2 leftVector = Vector2.Transform(transform.Forward, rotationMatrix);
                    amount = 3.2f;
                }
            }
            else
            {
                amount = (mScreenWidth / 2 - mMouseState.X) * Settings.MouseSensitivity;
            }
            float rotateSpeed = -(worldDelta * amount);
            transform.Rotation += rotateSpeed;
        }

    }
}
