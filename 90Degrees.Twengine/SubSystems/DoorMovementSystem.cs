using System;
using System.Collections.Generic;
using System.Text;
using Artemis;
using Microsoft.Xna.Framework;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using XNAHelper;

namespace Twengine.SubSystems
{
    public class DoorMovementSystem : EntityProcessingSystem
    {

        private ComponentMapper<Door> mDoorMapper;
        private ComponentMapper<Transform> mTransformMapper;
        private Tilemap mTilemap;

        public DoorMovementSystem(Tilemap tilemap)
            : base(typeof(Door), typeof(Transform))
        {
            mTilemap = tilemap;
        }

        public override void Initialize()
        {
            mDoorMapper = new ComponentMapper<Door>(world);
            mTransformMapper = new ComponentMapper<Transform>(world);
        }

        public override void Process(Entity e)
        {
            float doorSpeed = 1f;
            Door door = mDoorMapper.Get(e);
            Transform transform = mTransformMapper.Get(e);
            
            if (door.StartAnimating)   // we need to start animating..
            {
                door.StartAnimating = false;
                if (door.IsOpen)
                    door.IsClosing = true;
                else
                    door.IsOpening = true;
            }
            
            if ((door.IsOpening && TwenMath.IsAtPosition(transform.Position, door.OpenPosition, 0.01f)) || (door.IsClosing && TwenMath.IsAtPosition(transform.Position, door.SpawnPos, 0.01f))) // we have a target and are not there..
            {
                StopAnimating(door);
                return;
            }
            
            if (door.IsOpening)
            {
                MoveDoorTowardsPosition(transform, door.OpenPosition, doorSpeed);
            }
            else if (door.IsClosing)
            {
                MoveDoorTowardsPosition(transform, door.SpawnPos, doorSpeed);
            }
            
            if (door.CloseTimer > 0)
            {
                CheckForAutomatedClose(door, transform);
            }
        }

        private void CheckForAutomatedClose(Door door, Transform transform)
        {
            door.CloseTimer -= world.Delta;

            int y = (int)transform.OldPosition.Y;
            int x = (int)transform.OldPosition.X;

            List<Entity> entitiesOnCell = mTilemap.Entities[y, x];
         
            if (entitiesOnCell.Count > 0)
                door.ResetCloseTimer();
            else if (door.CloseTimer < 0)
            {
                door.StartCloseDoor();
            }
        }

        

        private void StopAnimating(Door door)
        {
            if (door.IsOpening)
            {
                door.OnFinishedOpen();
                door.IsOpening = false;
                door.IsOpen = true;
            }
            else if (door.IsClosing)
            {
                door.OnFinishedClose();
                door.IsClosing = false;
                door.IsOpen = false;
            }
        }

        private void MoveDoorTowardsPosition(Transform doorTransform, Vector2 targetPosition, float doorSpeed)
        {
            Vector2 direction = targetPosition - doorTransform.Position;
            direction.Normalize();
            doorTransform.Position += direction * doorSpeed * world.Delta;
        }
    }
}