using Artemis;
using Artemis.System;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Managers;

namespace Twengine.SubSystems
{
    public class PositionedEventArgs : EventArgs
    {
        public Vector2 PositionOfEvent { get; set; }
    }
    public class CollisionSystem : EntityComponentProcessingSystem<Collider, Transform>
    {
        public event EventHandler<PositionedEventArgs> PlayerFoundSecret;
        private List<Point> mRelativeNeighbors;
        private List<Rectangle> mNeighborTiles;
        private int mMapWidth;
        private int mMapHeight;
        private Raycaster mRaycaster;
        private Tilemap mTileMap;
        private HashSet<Point> mSecretWalls;
        private List<Entity>[,] mMapData;

        public CollisionSystem(Tilemap map, Raycaster raycaster)
            : base()
        {
            ChangeMap(map);
            mRaycaster = raycaster;
            mRelativeNeighbors = new List<Point>() { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1), new Point(1, 1) };
            mNeighborTiles = new List<Rectangle>(8);
        }

        protected override void Begin()
        {
            base.Begin();
            mMapData = mTileMap.Entities;
        }

        public void ChangeMap(Tilemap tilemap)
        {
            mMapWidth = tilemap.MapWidth;
            mMapHeight = tilemap.MapHeight;
            mTileMap = tilemap;
            mSecretWalls = new HashSet<Point>();
            for (int y = 0; y < mMapHeight; y++)
            {
                for (int x = 0; x < mMapWidth; x++)
                {
                    if (mTileMap.GetCellMetaDataByPosition(x, y) == '§')
                    {
                        mSecretWalls.Add(new Point(x, y));
                    }
                }
            }
        }

        private void OnPlayerFoundSecret(Vector2 position)
        {
            if (PlayerFoundSecret == null) return;
            PlayerFoundSecret(this, new PositionedEventArgs() { PositionOfEvent = position });
        }

        public override void Process(Entity e, Collider collider, Transform transform)
        {
            if (!transform.CollideWithMap) return;

            transform.Position = CollideWithMap(collider, transform.Position);
            transform.Position = CollideWithEntitiesOnMap(collider, transform.Position, e, transform);

            if (e.Group == "Player")
            {
                mRaycaster.SyncRaycasterCamToPlayer(transform);
                Point playerCell = new Point((int)transform.Position.X, (int)transform.Position.Y);
                if (mSecretWalls.Contains(playerCell))
                {
                    // player walks through fake/secret wall
                    OnPlayerFoundSecret(transform.Position);
                    mSecretWalls.Remove(playerCell);
                }
            }

        }

        public Vector2 CollideWithMap(Collider collider, Vector2 position)
        {
            bool collidedWithWall = false;
            Vector2 newPos = position;
            //Debug.WriteLine("Pos: " + position);
            float collisionRadius = collider.Radius;

            List<Rectangle> blockingTiles = GetBlockingNeighbors(position);
            //Debug.WriteLine("blockingTiles: " + blockingTiles.Count);

            foreach (Rectangle collisionRect in blockingTiles)
            {

                if (newPos.X + collisionRadius > collisionRect.Left && (int)Math.Floor(newPos.Y) == collisionRect.Top && (int)Math.Ceiling(newPos.X) == collisionRect.Left)
                {
                    collidedWithWall = true;
                    newPos.X = collisionRect.Left - collisionRadius;
                }

                if (newPos.X - collisionRadius < collisionRect.Right && (int)Math.Floor(newPos.Y) == collisionRect.Top && (int)Math.Floor(newPos.X) == collisionRect.Right)
                {
                    collidedWithWall = true;
                    newPos.X = collisionRect.Right + collisionRadius;
                }

                if (newPos.Y + collisionRadius > collisionRect.Top && (int)Math.Floor(newPos.X) == collisionRect.Left && (int)Math.Ceiling(newPos.Y) == collisionRect.Top)
                {
                    collidedWithWall = true;
                    newPos.Y = collisionRect.Top - collisionRadius;
                }

                if (newPos.Y - collisionRadius < collisionRect.Bottom && (int)Math.Floor(newPos.X) == collisionRect.Left && (int)Math.Floor(newPos.Y) == collisionRect.Bottom)
                {
                    collidedWithWall = true;
                    newPos.Y = collisionRect.Bottom + collisionRadius;
                }


            }
            //Debug.WriteLine("newPos: " + newPos);

            if (newPos.X + (1 + collisionRadius) > mMapWidth)
            {
                collidedWithWall = true;
                newPos.X = mMapWidth - (1 + collisionRadius);
            }
            if (newPos.Y + (1 + collisionRadius) > mMapHeight)
            {
                collidedWithWall = true;
                newPos.Y = mMapHeight - (1 + collisionRadius);
            }

            if (newPos.X - (1 + collisionRadius) < 0)
            {
                collidedWithWall = true;
                newPos.X = (1 + collisionRadius);
            }
            if (newPos.Y - (1 + collisionRadius) < 0)
            {
                collidedWithWall = true;
                newPos.Y = (1 + collisionRadius);
            }

            if (collidedWithWall)
                collider.OnCollisionWithWall();
            return newPos;
        }

        private List<Rectangle> GetBlockingNeighbors(Vector2 position)
        {
            mNeighborTiles.Clear();
            Rectangle startTile = new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), 1, 1);

            foreach (Point neighborPos in mRelativeNeighbors)
            {
                Rectangle neighborTile = new Rectangle(startTile.X + neighborPos.X, startTile.Y + neighborPos.Y, 1, 1);
                if (neighborTile.X > mMapWidth - 1 || neighborTile.X < 0 || neighborTile.Y > mMapHeight - 1 || neighborTile.Y < 0)
                    continue;

                char metaData = mTileMap.GetCellMetaDataByPosition(neighborTile.X, neighborTile.Y);
                if (mTileMap.GetCellDataByPosition(new Point(neighborTile.X, neighborTile.Y)) > -1 &&
                    metaData != 's') // is a wall and no secret wall
                {

                    mNeighborTiles.Add(neighborTile);
                }
                else if (metaData == 'd' && mTileMap.Entities[neighborTile.Y, neighborTile.X].Count > 0)
                {
                    // is a door
                    var entities = mTileMap.Entities[neighborTile.Y, neighborTile.X];
                    var door = entities.Find(e => e.IsActive && e.IsEnabled && e.HasComponent<Door>())?.GetComponent<Door>();
                    if (door is { IsOpen: false })
                    {
                        mNeighborTiles.Add(neighborTile);
                    }
                }
            }
            return mNeighborTiles;
        }

        public Vector2 CollideWithEntitiesOnMap(Collider collider, Vector2 position, Entity entity, Transform transform)
        {
            Vector2 newPos = position;

            List<Entity> possibleBlockingEntities = GetBlockingEntities(position, entity);

            foreach (Entity possibleBlockingEntity in possibleBlockingEntities)
            {
                if ((possibleBlockingEntity.Group == "Pickup" || entity.Group == "Pickup") && ((possibleBlockingEntity.Group != "Player" && entity.Group != "Player"))) continue; // dont collide pickups with anything other than the player
                if (entity.Group == "Projectile" && possibleBlockingEntity.Group == "Projectile") continue;
                // if (entity.Group == "Enemy" &&  && possibleBlockingEntity.Group == "Enemy") continue; // dont collide enemy with pickups

                Transform otherTransform = possibleBlockingEntity.GetComponent<Transform>();
                Collider otherCollider = possibleBlockingEntity.GetComponent<Collider>();

                float distance = Vector2.Distance(position, otherTransform.Position);
                if (distance < collider.Radius + otherCollider.Radius)
                {
                    // collision
                    if ((entity.Group == "Player" || entity.Group == "Enemy") && possibleBlockingEntity.Group != "Pickup") // never push back the player while colliding with pickups
                    {

                        Vector2 outwardsDir = position - otherTransform.Position;
                        float length = (collider.Radius + otherCollider.Radius) - distance;
                        outwardsDir.Normalize();
                        outwardsDir *= length;

                        newPos = position + outwardsDir;
                    }

                    collider.OnCollisionWithEntity(possibleBlockingEntity);
                    otherCollider.OnCollisionWithEntity(entity);
                }
            }

            return newPos;
        }

        private List<Entity> GetBlockingEntities(Vector2 position, Entity entity)
        {
            List<Entity> blockingEntities = new List<Entity>();
            Rectangle startTile = new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), 1, 1);

            foreach (Point neighborPos in mRelativeNeighbors.Append(new Point(0, 0)))
            {
                Rectangle neighborTile = new Rectangle(startTile.X + neighborPos.X, startTile.Y + neighborPos.Y, 1, 1);
                if (neighborTile.X > mMapWidth - 1 || neighborTile.X < 0 || neighborTile.Y > mMapHeight - 1 || neighborTile.Y < 0)
                    continue;


                foreach (Entity other in mMapData[neighborTile.Y, neighborTile.X])
                {
                    if (other == entity || !other.HasComponent<Collider>()) continue;
                    //if (other.Group == "Enemy" || other.Group == "Player" || other.Group == "Door" || other.Group == "Obstacle" || other.Group == "Hazard" || other.Group == "Pickup")// || other.Group == "Projectile"
                    blockingEntities.Add(other);
                }
            }
            return blockingEntities;
        }

    }
}
