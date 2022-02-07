using System;
using System.Collections;
using System.Collections.Generic;
using Artemis;
using Artemis.System;
using Artemis.Utils;
using Microsoft.Xna.Framework;
using Twengine.Components;
using Twengine.Helper;
using XNAHelper;

namespace Twengine.SubSystems
{
    public struct CellIndex
    {
        public int X;
        public int Y;
    }
    public class CollisionSystem : EntityProcessingSystem
    {
        private Dictionary<string, HashSet<string>> mCollisionMatrix;
        private bool[,] mCollisionMatrixNew;
        private string[] mCollisionLayer;

        private Circle mFirstCircleCache;
        private Circle mSecondCircleCache;

        private List<Entity>[,] mGrid;
        public int WorldWidth { get; set; }
        public int WorldHeight { get; set; }
        private int mGridSize;
        private bool[] mCollisionQuickCheck;
        private Bag<Entity> mArrayCache;

        public CollisionSystem()
            : base(Aspect.Empty())
        {
            mFirstCircleCache = new Circle(Vector2.Zero,0);
            mSecondCircleCache = new Circle(Vector2.Zero, 0);
            
            mCollisionMatrix = new Dictionary<string, HashSet<string>>();
            mCollisionMatrixNew = new bool[10, 10];
            mCollisionQuickCheck = new bool[10];
            mCollisionLayer = new string[10];
            mArrayCache = new Bag<Entity>(300);
        }
        public void SetWorldSize(int width, int height, int gridSize)
        {
            WorldWidth = width;
            WorldHeight = height;
            mGridSize = gridSize;
            mGrid = new List<Entity>[WorldWidth / mGridSize, WorldHeight / mGridSize];
        }

        public override void OnAdded(Entity e)
        {
            base.OnAdded(e);
            Transform transform = e.GetComponent<Transform>();
            Collider collider = e.GetComponent<Collider>();
            RepositionInGrid(e, collider, transform);
        }

        public override void OnRemoved(Entity e)
        {
            Collider collider = e.GetComponent<Collider>();
            foreach (CellIndex cell in collider.OccupiedGridCells)
            {
                mGrid[cell.X, cell.Y].Remove(e);
            }
            collider.OccupiedGridCells.Clear();
            base.OnRemoved(e);
        }

        private Rectangle GetCurrentColliderRect(Transform transform, Collider collider)
        {
            return new Rectangle((int) (transform.X - collider.Radius), (int) (transform.Y-collider.Radius),(int) (collider.Radius*2),(int) (collider.Radius*2));
        }

        private Rectangle GetCurrentColliderRect(Vector2 pos, Collider collider)
        {
            return new Rectangle((int)(pos.X - collider.Radius), (int)(pos.Y - collider.Radius), (int) (collider.Radius * 2), (int) (collider.Radius * 2));
        }

        private Rectangle GetOldColliderRect(Transform transform, Collider collider)
        {
            return new Rectangle((int)(transform.OldPosition.X - collider.Radius), (int)(transform.OldPosition.Y - collider.Radius), (int) (collider.Radius * 2), (int) (collider.Radius * 2));
        }

        private CellIndex GetFromPosition(float x, float y)
        {
            CellIndex index = new CellIndex();
            int maxX = WorldWidth/mGridSize;
            int maxY = WorldHeight/mGridSize;
            index.X = (int) MathHelper.Clamp((float) Math.Floor(x/mGridSize), 0, maxX-1);
            index.Y = (int) MathHelper.Clamp((float) Math.Floor(y/mGridSize), 0, maxY-1);
            return index;
        }

        private Rectangle GetCellRectangle(int indexX, int indexY)
        {
            int x = indexX * mGridSize;
            int y = indexY * mGridSize;
            return new Rectangle(x,y,mGridSize,mGridSize);
        }

        private void RepositionInGrid(Entity e, Collider collider, Transform transform)
        {
            HashSet<CellIndex> occupiedCells = new HashSet<CellIndex>();
            foreach (CellIndex cell in collider.OccupiedGridCells)
            {
                mGrid[cell.X, cell.Y].Remove(e);
            }
            collider.OccupiedGridCells.Clear();
            mFirstCircleCache.Center = transform.Position;
            mFirstCircleCache.Radius = collider.Radius;

            CellIndex topleft = GetFromPosition(mFirstCircleCache.Center.X - mFirstCircleCache.Radius, mFirstCircleCache.Center.Y - mFirstCircleCache.Radius);
            CellIndex topright = GetFromPosition(mFirstCircleCache.Center.X + mFirstCircleCache.Radius, mFirstCircleCache.Center.Y - mFirstCircleCache.Radius);
            CellIndex bottomleft = GetFromPosition(mFirstCircleCache.Center.X - mFirstCircleCache.Radius, mFirstCircleCache.Center.Y + mFirstCircleCache.Radius);
            CellIndex bottomright = GetFromPosition(mFirstCircleCache.Center.X + mFirstCircleCache.Radius, mFirstCircleCache.Center.Y + mFirstCircleCache.Radius);
            occupiedCells.Add(topleft);
            occupiedCells.Add(topright);
            occupiedCells.Add(bottomleft);
            occupiedCells.Add(bottomright);
            foreach (CellIndex cell in occupiedCells)
            {
                if (mGrid[cell.X, cell.Y] == null)
                {
                    mGrid[cell.X, cell.Y] = new List<Entity>();
                }
                mGrid[cell.X, cell.Y].Add(e);
                collider.OccupiedGridCells.Add(cell);
                //DebugDrawer.DrawRectangle(GetCellRectangle(cell.X, cell.Y));
            }
            //Console.WriteLine(e.Group + " Occupying " + collider.OccupiedGridCells.Count + " Cells.");
        }
        /// <summary>
        /// Define the tags of two groups which should be checked for Collisions.
        /// </summary>
        /// <param name="groupName1"></param>
        /// <param name="groupName2"></param>
        public void RegisterCollisionTest(string groupName1, string groupName2)
        {

            if (!mCollisionMatrix.ContainsKey(groupName1))
            {
                mCollisionMatrix[groupName1] = new HashSet<string>();
            }
            mCollisionMatrix[groupName1].Add(groupName2);
        }

        public void RegisterCollisionTestNew(int groupOne, int groupTwo)
        {
            mCollisionQuickCheck[groupOne] = true;
            mCollisionMatrixNew[groupOne, groupTwo] = true;
        }

        protected override void ProcessEntities(IDictionary<int, Entity> entities)
        {
            int comparison = 0;
            Bag<Entity> forLoop = GetArray(entities.Values);
            for (int j = 0; j < entities.Count; j++)
            {
                Entity entity = forLoop.Get(j);
                if (!entity.HasComponent<Collider>()) continue;

                Collider entityCollider = entity.GetComponent<Collider>();

                if (!mCollisionQuickCheck[entityCollider.CollisionGroup]) continue;

                
                Transform entityTransform = entity.GetComponent<Transform>();
                Physics entityPhysics = entity.GetComponent<Physics>();

                RepositionInGrid(entity, entityCollider, entityTransform);

                //DebugDrawer.DrawRectangle(GetCurrentColliderRect(entityTransform,entityCollider));

                Vector2 entityPos = entityTransform.Position;
                

                List<Entity> colliderInNeighborCells = GetColliderInNeighborCells(entity, entityCollider);
                foreach (Entity other in colliderInNeighborCells)
                {
                    if (!mCollisionMatrixNew[entityCollider.CollisionGroup, entityCollider.CollisionGroup]) continue;

                    Collider otherCollider = other.GetComponent<Collider>();        // performance hit
                    Transform otherTransform = other.GetComponent<Transform>();
                    Physics otherPhysics = other.GetComponent<Physics>();

                    Vector2 otherPos = otherTransform.Position;
                    
                    comparison++;
                    //Console.WriteLine("Checking " + entity.CollisionGroup + " against " + other.CollisionGroup);
                    if (ShapesCollide(entityCollider, entityPos, otherCollider, otherPos))
                    {
                        //Rectangle firstRect = GetCurrentColliderRect(entityPos, entityCollider);
                        //Rectangle secondRect = GetCurrentColliderRect(otherPos, otherCollider);
                        
                        //DebugDrawer.DrawRectangle(firstRect, Color.Red);
                        //DebugDrawer.DrawRectangle(secondRect, Color.Red);
                        
                        entityCollider.OnCollisionWithEntity(other);
                        otherCollider.OnCollisionWithEntity(entity);

                        if (!entityCollider.IsTrigger && !otherCollider.IsTrigger)  // none is trigger
                        {
                            // bump both

                            //Console.WriteLine("Bumping");
                            BumpBoth(entityTransform, entityPhysics, otherTransform, otherPhysics);
                            //SetBack(entityTransform, entityMovementPhysics, entityCollider, otherTransform, otherMovementPhysics, otherCollider);
                            //SimpleSetBack(entityTransform, otherTransform);
                        }
                    }
                }
            }
            //Console.WriteLine("used " + comparison + " comparisons.");
        }

        private void SimpleSetBack(Transform entityTransform, Transform otherTransform)
        {
            entityTransform.Position = entityTransform.OldPosition;
            otherTransform.Position = otherTransform.OldPosition;
        }

        private void BumpBoth(Transform entityTransform, Physics entityPhysics, Transform otherTransform, Physics otherPhysics)
        {
            
            if (otherPhysics != null)
            {
                Vector2 otherBumpDir = otherPhysics.Velocity * -1;
                if (otherPhysics.VelocityIsIntendedDirection)
                {
                    otherPhysics.IntendedMovementDirection = otherBumpDir;
                }
                else
                {
                    otherTransform.Position = otherTransform.OldPosition;
                    //otherPhysics.Velocity = otherBumpDir;
                }
                if (otherTransform.IsRotationDependentOnVelocity)
                {
                    otherTransform.Rotation = TwenMath.DirectionVectorToRotation(otherPhysics.Velocity);
                }
            }
            if (entityPhysics != null)
            {
                Vector2 entityBumpDir = entityPhysics.Velocity * -1;
                if (entityPhysics.VelocityIsIntendedDirection)
                {
                    entityPhysics.IntendedMovementDirection = entityBumpDir;
                }
                else
                {
                    entityTransform.Position = entityTransform.OldPosition;
                    //entityPhysics.Velocity = entityBumpDir;
                }
                if (entityTransform.IsRotationDependentOnVelocity)
                {
                    entityTransform.Rotation = TwenMath.DirectionVectorToRotation(entityPhysics.Velocity);
                }
            }
        }

        public override void Process(Entity e) { throw new NotImplementedException("Not needed.."); }

        private Bag<Entity> GetArray(ICollection<Entity> values)
        {
            int counter = 0;
            foreach (Entity entity in values)
            {
                mArrayCache.Set(counter, entity);
                counter++;
            }
            return mArrayCache;
        }

        private List<Entity> GetColliderInNeighborCells(Entity other, Collider entityCollider)
        {
            List<Entity> listOfCandidates = new List<Entity>();
            foreach (CellIndex cell in entityCollider.OccupiedGridCells)
            {
                foreach (Entity entity in mGrid[cell.X, cell.Y])
                {
                    if (entity == other) continue;
                    listOfCandidates.Add(entity);
                }
            }
            return listOfCandidates;
        }


        private bool ShapesCollide(Collider collider, Vector2 pos, Collider otherCollider, Vector2 otherPos)
        {
            /*
            Rectangle firstRect = GetCurrentColliderRect(pos, collider);
            Rectangle secondRect = GetCurrentColliderRect(ohterPos, otherCollider);
            //DebugDrawer.DrawRectangle(firstRect);
            //DebugDrawer.DrawRectangle(secondRect);

            return firstRect.Intersects(secondRect);
             * */
            // Circle vs. Circle
            mFirstCircleCache.Center = pos;
            mFirstCircleCache.Radius = collider.Radius;
            mSecondCircleCache.Center = otherPos;
            mSecondCircleCache.Radius = otherCollider.Radius;
            return mFirstCircleCache.Intersects(mSecondCircleCache);
            
        }
    }
}
