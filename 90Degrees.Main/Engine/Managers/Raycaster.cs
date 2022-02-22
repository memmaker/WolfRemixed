using Artemis;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using raycaster;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Datastructures;
using XNAHelper;

// TODO: Add Push Walls and more from here:
// https://lodev.org/cgtutor/raycasting4.html#Secrets

namespace Twengine.Managers
{
    public enum WallSide
    {
        North,
        East,
        South,
        West
    }

    public struct BasicRaycastHitInfo
    {
        public int X;
        public int MapX;
        public int MapY;
        public int WallType;
        public WallSide VisibleWallSide;
        public double DistToWall;
        public double WallXOffset;
        public int TexX;
        public int LineHeight;
        public int ScreenLineHeight;
        public int DrawStartY;
    }
    public struct SpriteStripe
    {
        public Rectangle ScreenRect { get; set; }
        public Rectangle TextureRect { get; set; }
    }

    public class Raycaster
    {
        public Camera Camera { get; private set; }

        private int[,] mMap;
        private int mScreenWidth;
        private int mScreenHeight;
        private int mMapWidth;
        private int mMapHeight;
        private double[] mZBuffer;
        private List<Entity> mVisibleEntities;
        private Color[] mFrameBuffer = new Color[Const.InternalRenderResolutionHeight * Const.InternalRenderResolutionWidth];

        public BasicRaycastHitInfo[] LastHits { get; set; }
        public Point TargetedWall { get; set; }

        private double mWallTextureWidth;
        /// <summary>
        /// what direction to step in y-direction (either +1 or -1)
        /// </summary>
        private int stepY;
        /// <summary>
        /// what direction to step in x-direction (either +1 or -1)
        /// </summary>
        private int stepX;
        /// <summary>
        /// length of ray from current position to next x-side
        /// </summary>
        private double sideDistX;
        /// <summary>
        /// length of ray from current position to next y-side
        /// </summary>
        private double sideDistY;

        private Tilemap mTilemap;
        private readonly Color[] mTexDataCeil;
        private readonly Color[] mTexDataFloor;

        public Vector2 Position
        {
            get => Camera.Position;
            set => Camera.Position = value;
        }

        public Vector2 Direction => Camera.Direction;

        public Vector2 ProjectionPlane => Camera.ProjectionPlane;

        public int Resolution { get; set; }

        public List<Entity>[] TargetedEntities { get; private set; }



        public bool[,] FogOfWarMap { get; private set; }

        public Raycaster(Tilemap map, int screenWidth, int screenHeight, float fov, Vector2 startPos, Vector2 viewDir)
        {
            SetScreenDependentValues(screenWidth, screenHeight);

            ChangeMap(map);

            mWallTextureWidth = 64;

            Camera = new Camera(startPos, fov, viewDir);

            Resolution = 1;

            mTexDataFloor = mTilemap.WallTextures.GetFrameData(36);
            mTexDataCeil = mTilemap.WallTextures.GetFrameData(12);
        }
        
        public void ViewportChanged(int screenWidth, int screenHeight)
        {
            SetScreenDependentValues(screenWidth, screenHeight);
        }
        private void SetScreenDependentValues(int screenWidth, int screenHeight)
        {
            LastHits = new BasicRaycastHitInfo[screenWidth];
            mZBuffer = new double[screenWidth];
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            InitMapArrays();
            InitLastHitsAndTargetedEntities();
        }
        private void InitLastHitsAndTargetedEntities()
        {
            for (int x = 0; x < ScreenWidth; x++)
            {
                LastHits[x] = new BasicRaycastHitInfo();
                TargetedEntities[x] = new List<Entity>();
            }
        }


        private void InitMapArrays()
        {

            mVisibleEntities = new List<Entity>();
            TargetedEntities = new List<Entity>[mScreenWidth];
            TargetedWall = Point.Zero;
            FogOfWarMap = new bool[mMapHeight, mMapWidth];
            for (int y = 0; y < mMapHeight; y++)
            {
                for (int x = 0; x < mMapWidth; x++)
                {
                    FogOfWarMap[y, x] = false;
                }
            }

        }

        public List<Entity> SpriteRaycasting()
        {
            //DebugDrawer.DrawString("CamPos: " + Camera.Position);
            foreach (List<Entity> targetedEntityList in TargetedEntities)
            {
                targetedEntityList.Clear();
            }

            mVisibleEntities.Sort(DistToPlayerComparer);

            foreach (Entity entity in mVisibleEntities)
            {


                RaycastSprite sprite = entity.GetComponent<RaycastSprite>();
                Transform transform = entity.GetComponent<Transform>();
                SpriteAnimator animator = entity.GetComponent<SpriteAnimator>();


                sprite.Stripes.Clear();

                if (sprite.Orientation == Orientation.None)
                {
                    BillBoardSpriteRaycasting(entity, sprite, transform, animator);
                }
                else
                {
                    OrientedSpriteRaycasting(sprite, transform);
                }



            }
            return mVisibleEntities;
        }

        private void OrientedSpriteRaycasting(RaycastSprite sprite, Transform transform)
        {
            int spriteMapX = (int)transform.Position.X;
            int spriteMapY = (int)transform.Position.Y;

            int texWidth = sprite.SpriteSheet.FrameWidth;

            double objectOffset;

            Orientation spriteOrientation = sprite.Orientation;


            //Find the sprite offset, that is how deep is the sprite inside the block depending on the orientation X or Y
            if (spriteOrientation == Orientation.Horizontal)
            {
                if (Camera.ProjectionPlane.X > 0)
                    objectOffset = (transform.Position.Y - (int)transform.Position.Y);
                else
                    objectOffset = 1 - (transform.Position.Y - (int)transform.Position.Y);
            }
            else
            {
                if (Camera.ProjectionPlane.Y < 0)
                    objectOffset = (transform.Position.X - (int)transform.Position.X);
                else
                    objectOffset = 1 - (transform.Position.X - (int)transform.Position.X);
            }


            double transformY;
            double transformStartY;
            double transformEndY;
            long drawStartX;
            long drawEndX;
            Vector2 spritePos = new Vector2(transform.Position.X, transform.Position.Y);
            GetSpriteScreenX(transform.Position, out transformY);

            // add or substract 0.5 because the position of the sprite is the middle
            Line doorLine;
            if (spriteOrientation == Orientation.Horizontal)
            {
                drawStartX = GetSpriteScreenX(new Vector2(spritePos.X - 0.5f, spritePos.Y), out transformStartY);
                drawEndX = GetSpriteScreenX(new Vector2(spritePos.X + 0.5f, spritePos.Y), out transformEndY);
                doorLine = new Line(spritePos.X - 0.5f, spritePos.Y, spritePos.X + 0.5f, spritePos.Y);
            }
            else
            {
                drawStartX = GetSpriteScreenX(new Vector2(spritePos.X, spritePos.Y - 0.5f), out transformStartY);
                drawEndX = GetSpriteScreenX(new Vector2(spritePos.X, spritePos.Y + 0.5f), out transformEndY);
                doorLine = new Line(spritePos.X, spritePos.Y - 0.5f, spritePos.X, spritePos.Y + 0.5f);
            }

            //validate sprite's start and end
            if (drawStartX < 0) drawStartX = 0;
            if (drawStartX > ScreenWidth) drawStartX = ScreenWidth;

            if (drawEndX < 0) drawEndX = 0;
            if (drawEndX > ScreenWidth) drawEndX = ScreenWidth;

            //switch variables depending on the orientation of the camera

            if (drawStartX > drawEndX)
            {
                long tempX = drawStartX;
                drawStartX = drawEndX;
                drawEndX = tempX;
            }



            //loop through every vertical stripe of the sprite on screen
            for (long x = drawStartX; x < drawEndX; x++)
            {
                double cameraX = ((2.0 * x) / ScreenWidth) - 1; //x-coordinate in camera space

                double rayPosX = Camera.Position.X;
                double rayPosY = Camera.Position.Y;
                double rayDirX = Camera.Direction.X + Camera.ProjectionPlane.X * cameraX;
                double rayDirY = Camera.Direction.Y + Camera.ProjectionPlane.Y * cameraX;

                double perpWallDist = 0;

                Line rayLine = new Line(Camera.Position.X, Camera.Position.Y, (float)(rayPosX + rayDirX * 100), (float)(rayPosY + rayDirY * 100));

                //what direction to step in x or y-direction (either +1 or -1)
                stepX = 0;
                stepY = 0;

                int side = spriteOrientation == Orientation.Horizontal ? 1 : 0;

                //calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                }
                else
                {
                    stepX = 1;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                }
                else
                {
                    stepY = 1;
                }

                //Calculate distance projected on camera direction (oblique distance will give fisheye effect!)
                if (side == 0)
                {
                    float diffX = spriteMapX - Camera.Position.X;

                    if (diffX < 0 && diffX > -1)
                    {
                        Vector2 intersectionPoint = new Vector2();
                        if (TwenMath.DoLinesIntersect(rayLine, doorLine, ref intersectionPoint))
                        {
                            perpWallDist = Vector2.Distance(Camera.Position, intersectionPoint);
                        }
                    }
                    else
                    {
                        double diffPlusStepX = diffX + (1d - stepX) / 2.0d;
                        perpWallDist = Math.Abs(diffPlusStepX / rayDirX) + Math.Abs(objectOffset / rayDirX);
                    }
                }
                else
                {
                    float diffY = spriteMapY - Camera.Position.Y;
                    if (diffY < 0 && diffY > -1)
                    {
                        Vector2 intersectionPoint = new Vector2();
                        if (TwenMath.DoLinesIntersect(rayLine, doorLine, ref intersectionPoint))
                        {
                            perpWallDist = Vector2.Distance(Camera.Position, intersectionPoint);
                        }
                    }
                    else
                    {
                        double diffPlusStepY = diffY + (1 - stepY) / 2.0;
                        perpWallDist = Math.Abs(diffPlusStepY / rayDirY) + Math.Abs(objectOffset / rayDirY);
                    }
                }


                //Calculate height of line to draw on screen
                int lineHeight = (int)Math.Abs(ScreenHeight / perpWallDist);

                //calculate value of wallX
                //where exactly the wall was hit
                double wallX;
                if (side == 0)
                {
                    //vertical
                    wallX = rayPosY + (((spriteMapX - rayPosX + (1 - stepX) / 2.0) / rayDirX) + Math.Abs(objectOffset / rayDirX)) * rayDirY;
                }
                else
                {
                    //horizontal
                    wallX = rayPosX + (((spriteMapY - rayPosY + (1 - stepY) / 2.0) / rayDirY) + Math.Abs(objectOffset / rayDirY)) * rayDirX;
                }

                wallX -= Math.Floor(wallX);

                double spriteCenter;

                //Find the sprite center relative to the block containg it
                if (spriteOrientation == Orientation.Horizontal)
                    spriteCenter = (transform.Position.X - (int)transform.Position.X);
                else
                    spriteCenter = (transform.Position.Y - (int)transform.Position.Y);

                //Map correctly the texture depending on center of the sprite. 
                //By default the center is an offset of 0.5 in X or Y axis
                if (spriteCenter > 0.5)
                    wallX = wallX - (spriteCenter - 0.5);
                else if (spriteCenter < 0.5)
                    wallX = wallX - (spriteCenter + 0.5);

                if (wallX < 0) wallX = 1 + wallX;

                //x coordinate on the texture
                int texX = (int)(wallX * texWidth);

                Rectangle sourceRectByIndex = sprite.SourceRect;

                if (perpWallDist > objectOffset && x >= 0 && x < ScreenWidth && perpWallDist < LastHits[x].DistToWall)// && transformY > objectOffset)
                {

                    Rectangle textureSourceRect = new Rectangle(sourceRectByIndex.X + texX, sourceRectByIndex.Y + 0, 1, sprite.SpriteSheet.FrameHeight);

                    int drawStartY = (int)(((ScreenHeight / 2) + (Camera.EyeHeight / perpWallDist)) - (lineHeight / 2));

                    Rectangle spriteDestRect = new Rectangle((int)x, drawStartY, 1, lineHeight);
                    sprite.Stripes.Add(new SpriteStripe() { TextureRect = textureSourceRect, ScreenRect = spriteDestRect });
                }

            }

        }

        private void BillBoardSpriteRaycasting(Entity entity, RaycastSprite sprite, Transform transform, SpriteAnimator animator)
        {
            //translate sprite position to relative to camera
            double spriteDepthZ;
            long spriteScreenX = GetSpriteScreenX(transform.Position, out spriteDepthZ);

            //calculate height of the sprite on screen
            int spriteHeight = (int)Math.Abs(ScreenHeight / spriteDepthZ); //using "transformY" instead of the real distance prevents fisheye

            //calculate lowest and highest pixel to fill in current stripe
            int drawStartY = (int)(-spriteHeight / 2 + ((ScreenHeight / 2) + (Camera.EyeHeight / spriteDepthZ)));
            //if (drawStartY < 0) drawStartY = 0;
            //int drawEndY = spriteHeight/2 + mScreenHeight/2;
            //if (drawEndY >= mScreenHeight) drawEndY = mScreenHeight - 1;

            //calculate width of the sprite
            int spriteWidth = (int)Math.Abs(ScreenHeight / spriteDepthZ);
            long drawStartX = -spriteWidth / 2 + spriteScreenX;
            if (drawStartX < 0) drawStartX = 0;
            long drawEndX = spriteWidth / 2 + spriteScreenX;
            if (drawEndX >= ScreenWidth) drawEndX = ScreenWidth - 1;


            //loop through every vertical stripe of the sprite on screen
            Rectangle sourceRectByIndex = sprite.SourceRect;
            for (long x = drawStartX; x < drawEndX; x++)
            {
                int texX = (int)((256.0 * (x - (-spriteWidth / 2.0 + spriteScreenX)) * sprite.SpriteSheet.FrameWidth / spriteWidth) / 256.0);
                //the conditions in the if are:
                //1) it's in front of camera plane so you don't see things behind you
                //2) it's on the screen (left)
                //3) it's on the screen (right)
                //4) ZBuffer, with perpendicular distance

                Rectangle textureSourceRect = new Rectangle(sourceRectByIndex.X + texX, sourceRectByIndex.Y + 0, 1, sprite.SpriteSheet.FrameHeight);

                if (spriteDepthZ > 0 && x > 0 && x < ScreenWidth && spriteDepthZ < mZBuffer[x] && sprite.SpriteSheet.TextureOpaqueRect[sprite.FrameIndex].Intersects(textureSourceRect))
                {
                    Rectangle spriteDestRect = new Rectangle((int)x, drawStartY, 1, spriteHeight);
                    sprite.Stripes.Add(new SpriteStripe() { ScreenRect = spriteDestRect, TextureRect = textureSourceRect });
                    TargetedEntities[x].Add(entity);
                }
            }
            sprite.Depth = (float)spriteDepthZ;
        }


        /// <summary>
        /// Translates the spritePos by the Camera position, so do not this beforehand.
        /// </summary>
        /// <param name="spritePos">The position of the sprite that will be mapped to the screen.</param>
        /// <param name="transformY">Will contain the depth of the sprite, the Z value.</param>
        /// <returns>The X coordinate on the screen in pixels, where this spritePos should be drawn.</returns>
        private long GetSpriteScreenX(Vector2 spritePos, out double transformY)
        {
            double spriteX = spritePos.X - Position.X;
            double spriteY = spritePos.Y - Position.Y;

            double invDet = 1.0 / (ProjectionPlane.X * Direction.Y - Direction.X * ProjectionPlane.Y);

            double transformX = invDet * (Direction.Y * spriteX - Direction.X * spriteY);
            transformY = invDet * (-ProjectionPlane.Y * spriteX + ProjectionPlane.X * spriteY);
            if (transformY < 0)
            {
                transformY = 0.0001;
            }
            long spriteScreenX = (long)((ScreenWidth / 2.0) * (1 + transformX / transformY));
            return spriteScreenX;
        }

        private int DistToPlayerComparer(Entity first, Entity second)
        {
            float distFirst = Vector2.DistanceSquared(first.GetComponent<Transform>().Position, Position);
            float distSecond = Vector2.DistanceSquared(second.GetComponent<Transform>().Position, Position);
            return distSecond.CompareTo(distFirst);
        }

        public BasicRaycastHitInfo[] Raycasting()
        {
            mVisibleEntities.Clear();

            FogOfWarMap[(int)Camera.Position.Y, (int)Camera.Position.X] = true;  // own field is visible..

            int rays = (int)Math.Ceiling(ScreenWidth / (decimal)Resolution);


            for (int x = 0; x < rays; x++)
            {
                #region setup ray and camera

                double cameraX = ((2.0 * x) / rays) - 1; //x-coordinate in camera space
                double rayDirX = Camera.Direction.X + Camera.ProjectionPlane.X * cameraX;
                double rayDirY = Camera.Direction.Y + Camera.ProjectionPlane.Y * cameraX;

                //which box of the map we're currently in  
                int mapX = (int)Camera.Position.X;
                int mapY = (int)Camera.Position.Y;

                #endregion

                // check sprites in starting field of cam..
                IsSpriteObstructing(mapX, mapY);

                //length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                double deltaDistY = Math.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));

                CalcStepSizeAndSideDist(mapX, mapY, rayDirX, rayDirY, deltaDistX, deltaDistY);  // modifies member variables..


                bool wallHit = false; //was there a wall hit?
                bool doorHit = false; //was there a door hit?
                bool obstructing = false;
                int side = 0; //was a NS or a EW wall hit?
                double perpObstacleDist;

                #region perform DDA

                while (!wallHit)
                {
                    //jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }


                    if (PositionIsOutOfMapBounds(mapX, mapY))
                    {
                        break;
                    }


                    //Check if ray has hit a wall
                    // TODO: Idea - Decide on a wall depending on the part we hit
                    if (mMap[mapY, mapX] > -1)
                    {
                        // && (!(mapX == 43 && mapY == 49))
                        wallHit = true;

                    }
                   

                    if (!obstructing)
                    {
                        FogOfWarMap[mapY, mapX] = true; // used for the map rendering..
                    }
                    else
                    {
                        break;  // stop casting because we hit an obstructing sprite last round..
                    }

                    // check if ray has hit a sprite
                    obstructing = IsSpriteObstructing(mapX, mapY) ? true : obstructing; // if the ray was obstructed by a sprite (usually a door) mark just this one square as seen and then stop..

                    doorHit = UpdateDoorHit(x, mapX, mapY, doorHit);
                }
                #endregion

                // we found a wall casting a ray at x at mapX, mapY
                if (PositionIsOutOfMapBounds(mapX, mapY))
                {
                    break;
                }
                
                if (x == ScreenWidth / 2) // For Game use, targeting ray
                {
                    if (!doorHit)
                    {
                        TargetedDoor = Point.Zero;
                    }
                    TargetedWall = new Point(mapX, mapY);
                }

                //Calculate distance projected on camera direction (oblique distance will give fisheye effect!)

                // TODO: Idea - What happens when we change this?
                perpObstacleDist = GetPerpWallDist(rayDirX, rayDirY, mapX, mapY, side);

                mZBuffer[x] = perpObstacleDist;

                //if (doorHit) continue; // doors are treated as sprites.. see orientedspriteraycasting

                //Calculate height of line to draw on screen
                int lineHeight = (int)Math.Abs(ScreenHeight / perpObstacleDist);

                //calculate lowest and highest pixel to fill in current stripe

                int drawStart = (int)(-lineHeight / 2.0f + ((ScreenHeight / 2.0f) + (Camera.EyeHeight / perpObstacleDist)));
                int realDrawStart = drawStart;
                int drawEnd = (int)(lineHeight / 2.0f + ((ScreenHeight / 2.0f) + (Camera.EyeHeight / perpObstacleDist)));
                int realHeight = drawEnd - drawStart;
                if (drawStart < 0 && drawEnd >= ScreenHeight)
                {
                    drawStart = 0;
                    drawEnd = ScreenHeight;
                }

                double wallX; //where exactly the wall was hit
                if (side == 0) wallX = Camera.Position.Y + perpObstacleDist * rayDirY;
                else wallX = Camera.Position.X + perpObstacleDist * rayDirX;
                wallX -= Math.Floor(wallX);

                #region fill return info object

                LastHits[x].DistToWall = perpObstacleDist;
                LastHits[x].MapX = mapX;
                LastHits[x].MapY = mapY;
                LastHits[x].WallXOffset = wallX;
                LastHits[x].VisibleWallSide = GetWallSideHit(rayDirX, rayDirY, side);
                LastHits[x].LineHeight = realHeight;
                LastHits[x].X = x;
                LastHits[x].DrawStartY = realDrawStart;
                LastHits[x].ScreenLineHeight = drawEnd - drawStart;
                LastHits[x].TexX = (int)(wallX * mWallTextureWidth);
                LastHits[x].WallType = mMap[mapY, mapX];


                #endregion

            }

            return LastHits;
        }

        private bool UpdateDoorHit(int x, int mapX, int mapY, bool doorHit)
        {
            if (x == ScreenWidth / 2) // only check for doors when they are in the middle of the screen
            {
                if (!doorHit) // we only want to mark the first door we encounter..
                {
                    doorHit = CheckForDoor(mapX, mapY);
                    if (doorHit)
                    {
                        //first door hit..
                        TargetedDoor = new Point(mapX, mapY); // save it for game mechanic to access..
                    }
                }
            }
            return doorHit;
        }

        private bool CheckForDoor(int mapX, int mapY)
        {
            char mapMetaData = mTilemap.GetCellMetaDataByPosition(mapX, mapY);
            return (mapMetaData == 'd');
        }

        public Point TargetedDoor { get; set; }

        public int ScreenWidth
        {
            get { return mScreenWidth; }
            set { mScreenWidth = value; }
        }

        public int ScreenHeight
        {
            get { return mScreenHeight; }
            set { mScreenHeight = value; }
        }

        private double GetPerpWallDist(double rayDirX, double rayDirY, int mapX, int mapY, int side)
        {
            double perpWallDist;
            if (side == 0)
            {
                perpWallDist = Math.Abs((mapX - Camera.Position.X + (1 - stepX) / 2.0) / rayDirX);
            }
            else
            {
                perpWallDist = Math.Abs((mapY - Camera.Position.Y + (1 - stepY) / 2.0) / rayDirY);
            }
            
            return perpWallDist;
        }

        /// <summary>
        /// This updates the member variables stepX & sideDistX or stepY & sideDistY respectively
        /// </summary>
        /// <param name="mapX"></param>
        /// <param name="mapY"></param>
        /// <param name="rayDirX"></param>
        /// <param name="rayDirY"></param>
        /// <param name="deltaDistX"></param>
        /// <param name="deltaDistY"></param>
        private void CalcStepSizeAndSideDist(int mapX, int mapY, double rayDirX, double rayDirY, double deltaDistX, double deltaDistY)
        {
            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (Camera.Position.X - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0 - Camera.Position.X) * deltaDistX;
            }
            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (Camera.Position.Y - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0 - Camera.Position.Y) * deltaDistY;
            }
        }

        private WallSide GetWallSideHit(double rayDirX, double rayDirY, int side)
        {
            WallSide wallSideHit;
            if (side == 0 && rayDirX > 0)
            {
                wallSideHit = WallSide.West;
            }
            else if (side == 0 && rayDirX < 0)
            {
                wallSideHit = WallSide.East;
            }
            else if (side == 1 && rayDirY > 0)
            {
                wallSideHit = WallSide.North;
            }
            else
            {
                wallSideHit = WallSide.South;
            }
            return wallSideHit;
        }

        private bool PositionIsOutOfMapBounds(int mapX, int mapY)
        {
            return mapX < 0 || mapX >= mMapWidth || mapY < 0 || mapY >= mMapHeight;
        }

        public void SyncRaycasterCamToPlayer(Transform transform)
        {
            Position = transform.Position;
            Camera.Direction = transform.Forward;
        }
        
        private bool IsSpriteObstructing(int mapX, int mapY)
        {
            List<Entity> visibleSprites = mTilemap.Entities[mapY, mapX];
            foreach (Entity visibleSprite in visibleSprites)
            {
                if (!mVisibleEntities.Contains(visibleSprite) && visibleSprite.Tag != "LocalPlayer")
                    mVisibleEntities.Add(visibleSprite);
                if (visibleSprite.Group == "Door")
                {
                    Door door = visibleSprite.GetComponent<Door>();
                    if (!door.IsOpening && !door.IsOpen)
                        return true;
                }
            }
            return false;
        }



        public double HorizontalToDist(int y)
        {
            return ScreenHeight / (2.0 * y - ScreenHeight);
        }

        public void ChangeMap(Tilemap tilemap)
        {
            mTilemap = tilemap;
            mMap = tilemap.GetMapData();
            mMapWidth = tilemap.MapWidth;
            mMapHeight = tilemap.MapHeight;

            InitMapArrays();
            InitLastHitsAndTargetedEntities();
        }

        public Color[] FloorCasting()
        {
            

            int screenHeight = Const.InternalRenderResolutionHeight;
            int screenWidth = Const.InternalRenderResolutionWidth;
            int texWidth = 64;
            int texHeight = 64;
            int h = screenHeight;
            
            //FLOOR CASTING
            for (int y = 0; y < h; y++)
            {
                
                // rayDir for leftmost ray (x = 0) and rightmost ray (x = w)
                float rayDirX0 = Camera.Direction.X - Camera.ProjectionPlane.X;
                float rayDirY0 = Camera.Direction.Y - Camera.ProjectionPlane.Y;
                float rayDirX1 = Camera.Direction.X + Camera.ProjectionPlane.X;
                float rayDirY1 = Camera.Direction.Y + Camera.ProjectionPlane.Y;
                
                // Current y position compared to the center of the screen (the horizon)
                int p = y - screenHeight / 2;

                // Vertical position of the camera.
                float posZ = 0.5f * screenHeight;

                // Horizontal distance from the camera to the floor for the current row.
                // 0.5 is the z position exactly in the middle between floor and ceiling.
                float rowDistance = posZ / p;
                
                
                // calculate the real world step vector we have to add for each x (parallel to camera plane)
                // adding step by step avoids multiplications with a weight in the inner loop
                float du = rowDistance * (rayDirX1 - rayDirX0) / screenWidth;
                float dv = rowDistance * (rayDirY1 - rayDirY0) / screenWidth;

                // real world coordinates of the leftmost column. This will be updated as we step to the right.
                float gu = Camera.Position.X + rowDistance * rayDirX0;
                float gv = Camera.Position.Y + rowDistance * rayDirY0;
                
                for (int x = 0; x < screenWidth; ++x)
                {
                    // the cell coord is simply got from the integer parts of floorX and floorY
                    int cellX = (int)(gu);
                    int cellY = (int)(gv);

                    // get the texture coordinate from the fractional part
                    float uFraction = (gu - cellX);
                    float vFraction = (gv - cellY);
                    int u = (int)(texWidth * uFraction) & (texWidth - 1);
                    int v = (int)(texHeight * vFraction) & (texHeight - 1);
                    

                    gu += du; // gu += du
                    gv += dv;

                    int colorTexIndex = texWidth * v + u;
                    Color pixelColorFloor = mTexDataFloor[colorTexIndex];
                    Color pixelColorCeiling = mTexDataCeil[colorTexIndex];
                    /*
                    float factor = 1.0f;
                    float absoluteDist = rowDistance < 0 ? -rowDistance : rowDistance;
                    if (absoluteDist > 10f)
                    {
                        factor = 0.0f;
                    }
                    else if (absoluteDist > 5f)
                    {
                        factor = 0.2f;
                    }
                    else if (absoluteDist > 3f)
                    {
                        factor = 0.3f;
                    }
                    else if (absoluteDist > 2f)
                    {
                        factor = 0.4f;
                    }
                    pixelColorFloor.R = (byte)(pixelColorFloor.R * factor);
                    pixelColorFloor.G = (byte)(pixelColorFloor.G * factor);
                    pixelColorFloor.B = (byte)(pixelColorFloor.B * factor);

                    pixelColorCeiling.R = (byte)(pixelColorCeiling.R * factor);
                    pixelColorCeiling.G = (byte)(pixelColorCeiling.G * factor);
                    pixelColorCeiling.B = (byte)(pixelColorCeiling.B * factor);
                    //Color floorShade = mTilemap.GetShadingColor(pixelColorFloor, rowDistance);
                    //Color ceilingShade = mTilemap.GetShadingColor(pixelColorCeiling, rowDistance);
                    */
                    var floorBufferIndex = y * Const.InternalRenderResolutionWidth + x;
                    var ceilingBufferIndex = (screenHeight - y - 1) * Const.InternalRenderResolutionWidth + x;

                    mFrameBuffer[floorBufferIndex] = pixelColorFloor;
                    mFrameBuffer[ceilingBufferIndex] = pixelColorCeiling;
                    
                }
            }

            return mFrameBuffer;
        }

    }
}
