using System;
using System.Collections.Generic;
using Artemis;
using Microsoft.Xna.Framework;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using Twengine.Managers;

namespace Twengine.SubSystems
{
    public class PositionedEventArgs : EventArgs
    {
        public Vector2 PositionOfEvent{ get; set;}
    }
    public class TilemapCollisionSystem : EntityProcessingSystem
    {
        public event EventHandler<PositionedEventArgs> PlayerFoundSecret;
        private ComponentMapper<Transform> mTransformMapper;
        private ComponentMapper<Collider> mColliderMapper;
        private List<Point> mRelativeNeighbors;
        private List<Rectangle> mNeighborTiles;
        private int mMapWidth;
        private int mMapHeight;
        private Raycaster mRaycaster;
        private Tilemap mTileMap;
        private HashSet<Point> mSecretWalls;
        public TilemapCollisionSystem(Tilemap map, Raycaster raycaster)
            : base(typeof(Transform),typeof(Collider))
        {
            ChangeMap(map);
            mRaycaster = raycaster;
            mRelativeNeighbors = new List<Point>() { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1), new Point(1, 1) };
            mNeighborTiles = new List<Rectangle>(8);
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
                    if (mTileMap.GetCellMetaDataByPosition(x,y) == '§')
                    {
                        mSecretWalls.Add(new Point(x, y));
                    }
                }
            }
        }

        public override void Initialize()
        {
            mTransformMapper = new ComponentMapper<Transform>(world);
            mColliderMapper = new ComponentMapper<Collider>(world);
        }

        private void OnPlayerFoundSecret(Vector2 position)
        {
            if (PlayerFoundSecret == null) return;
            PlayerFoundSecret(this,new PositionedEventArgs(){PositionOfEvent = position});
        }

        public override void Process(Entity e)
        {
            Transform transform = mTransformMapper.Get(e);
            if (!transform.CollideWithMap) return;

            Collider collider = mColliderMapper.Get(e);
            
            transform.Position = CollideWithMap(collider, transform.Position);
            if (e.Group == "Player")
            {
                mRaycaster.SyncRaycasterCamToPlayer(transform);
                Point playerCell = new Point((int) transform.Position.X, (int) transform.Position.Y);
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
                if (mTileMap.GetCellDataByPosition(new Point(neighborTile.X, neighborTile.Y)) > 0 && metaData != 's')  // is a wall and no secret wall
                    mNeighborTiles.Add(neighborTile);
            }
            return mNeighborTiles;
        }
    }
}
