using System;
using System.Collections.Generic;
using Artemis;
using Artemis.System;
using Microsoft.Xna.Framework;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using Twengine.Managers;

namespace Twengine.SubSystems
{

    public class TilemapEntityCollisionSystem : EntityComponentProcessingSystem<Collider, Transform>
    {
        private List<Point> mRelativeNeighbors;
        private List<Rectangle> mNeighborTiles;
        private List<Entity>[,] mMapData;
        private int mMapWidth;
        private int mMapHeight;
        private Raycaster mRaycaster;
        private Tilemap mTilemap;
        

        public TilemapEntityCollisionSystem(Raycaster raycaster, Tilemap map)
            : base()
        {
            mRaycaster = raycaster;
            ChangeMap(map);
            mRelativeNeighbors = new List<Point>() {new Point(0,0), new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1), new Point(1, 1) };
            mNeighborTiles = new List<Rectangle>(9);
        }

        public void ChangeMap(Tilemap tilemap)
        {
            mMapWidth = tilemap.MapWidth;
            mMapHeight = tilemap.MapHeight;
            mTilemap = tilemap;
        }
        protected override void Begin()
        {
            base.Begin();
            mMapData = mTilemap.Entities;
        }
      
        public override void Process(Entity e, Collider collider, Transform transform)
        {
            if (!transform.CollideWithEntityMap) return;
            
            
            transform.Position = CollideWithEntitiesOnMap(collider, transform.Position, e, transform);

            if (e.Group == "Player")
                mRaycaster.SyncRaycasterCamToPlayer(transform);
        }

        public Vector2 CollideWithEntitiesOnMap(Collider collider, Vector2 position, Entity entity, Transform transform)
        {
            Vector2 newPos = position;

            List<Entity> possibleBlockingEntities = GetBlockingEntities(position, entity);

            foreach (Entity possibleBlockingEntity in possibleBlockingEntities)
            {
                if (entity.Group == "Projectile" && possibleBlockingEntity.Group == "Pickup") continue; // dont collide proejctile with pickups
                if (entity.Group == "Enemy" && possibleBlockingEntity.Group == "Pickup" || entity.Group == "Pickup" && possibleBlockingEntity.Group == "Enemy") continue; // dont collide enemy with pickups

                Transform otherTransform = possibleBlockingEntity.GetComponent<Transform>();
                Collider otherCollider = possibleBlockingEntity.GetComponent<Collider>();
                
                float distance = Vector2.Distance(position,otherTransform.Position);
                if (distance < collider.Radius + otherCollider.Radius)
                {
                    // collision
                    if (entity.Group == "Player" && possibleBlockingEntity.Group != "Pickup") // never push back the player while colliding with pickups
                    {

                        Vector2 outwardsDir = position - otherTransform.Position;
                        float length = (collider.Radius + otherCollider.Radius) - distance;
                        outwardsDir.Normalize();
                        outwardsDir *= length;

                        newPos = position + outwardsDir;
                    }
                    else if (entity.Group == "Enemy")
                    {
                        newPos = transform.OldPosition;
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

            foreach (Point neighborPos in mRelativeNeighbors)
            {
                Rectangle neighborTile = new Rectangle(startTile.X + neighborPos.X, startTile.Y + neighborPos.Y, 1, 1);
                if (neighborTile.X > mMapWidth - 1 || neighborTile.X < 0 || neighborTile.Y > mMapHeight - 1 || neighborTile.Y < 0)
                    continue;


                foreach (Entity other in mMapData[neighborTile.Y, neighborTile.X])
                {
                    if (other == entity) continue;
                    if (other.Group == "Enemy" || other.Group == "Player" || other.Group == "Door" || other.Group == "Obstacle" || other.Group == "Hazard" || other.Group == "Pickup")// || other.Group == "Projectile"
                        blockingEntities.Add(other);
                }
            }
            return blockingEntities;
        }

        private List<Rectangle> GetBlockingNeighbors(Vector2 position, Entity entity)
        {
            mNeighborTiles.Clear();
            Rectangle startTile = new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), 1, 1);

            foreach (Point neighborPos in mRelativeNeighbors)
            {
                Rectangle neighborTile = new Rectangle(startTile.X + neighborPos.X, startTile.Y + neighborPos.Y, 1, 1);
                if (neighborTile.X > mMapWidth - 1 || neighborTile.X < 0 || neighborTile.Y > mMapHeight - 1 || neighborTile.Y < 0)
                    continue;

                if (GetBlockingEntity(neighborTile, entity) != null)
                    mNeighborTiles.Add(neighborTile);
            }
            return mNeighborTiles;
        }

        private Entity GetBlockingEntity(Rectangle neighborTile, Entity entity)
        {
            foreach (Entity other in mMapData[neighborTile.Y, neighborTile.X])
            {
                if (other == entity) continue;
                if (other.Group == "Enemy" || other.Group == "Player" || other.Group == "Door" || other.Group == "Obstacle")
                    return other;
            }
            return null;
        }
    }
}
