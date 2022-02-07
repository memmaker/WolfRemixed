using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using Twengine.Helper;
using Twengine.Managers;
using XNAHelper;

namespace Twengine.Datastructures
{
    public struct MessageTriggerInfo
    {
        public string Text { get; set; }
        public bool IsCentered { get; set; }
    }

    public class Tilemap
    {
        private readonly int mMapWidth;
        private readonly int mMapHeight;

        private readonly int[,] mMapData;
        private readonly char[,] mMapMetaData;
        public List<Entity>[,] Entities { get; private set; }

        public List<Entity> AllEntities { get; private set; }

        public Vector2 PlayerSpawn { get; set; }
        public Vector2 PlayerViewDirection { get; set; }

        private HashSet<Point> mPlayerPositionHistory;
        
        public bool PseudoShading
        {
            get { return mPseudoShading; }
        }

        public Color FloorColor { get; set; }
        public Color CeilingColor { get; set; }

        public int MapHeight
        {
            get { return mMapHeight; }
        }

        public int MapWidth
        {
            get { return mMapWidth; }
        }

        public SpriteSheet SpriteTextures { get; set; }
        public SpriteSheet WallTextures { get; set; }
        public Dictionary<Point, int> DoorSpawnPoints { get; set; }
        public Dictionary<Vector2, int> ItemSpawnPoints { get; set; }
        public Dictionary<Vector2, int> EnemySpawnPoints { get; set; }
        public Dictionary<Point, MessageTriggerInfo> MessageTriggers { get; set; }

        private bool mPseudoShading;
        private int mShadingCutOff;
        private float mShadingDivisor;
        private List<Point> mCurrentLine;
        private List<List<Point>> mLineOfSightList;

        public Tilemap(int[,] mapData, char[,] metaData, bool pseudoShading, int shadingCutOff, float shadingDivisor)
        {
            mShadingCutOff = shadingCutOff;
            mShadingDivisor = shadingDivisor;
            mPseudoShading = pseudoShading;
            mCurrentLine = new List<Point>();
            mLineOfSightList = new List<List<Point>>();
            mMapData = mapData;
            mMapMetaData = metaData;
            mMapHeight = mMapData.GetLength(0);
            mMapWidth = mMapData.GetLength(1);
            mPlayerPositionHistory = new HashSet<Point>();

            DoorSpawnPoints = new Dictionary<Point, int>();
            ItemSpawnPoints = new Dictionary<Vector2, int>();
            EnemySpawnPoints = new Dictionary<Vector2, int>();
            MessageTriggers = new Dictionary<Point, MessageTriggerInfo>();
            FloorColor = Color.White;
            CeilingColor = Color.White;
            AllEntities = new List<Entity>();
            Entities = new List<Entity>[mMapHeight, mMapWidth];
            for (int y = 0; y < mMapHeight; y++)
            {
                for (int x = 0; x < mMapWidth; x++)
                {
                    Entities[y, x] = new List<Entity>();
                }
            }
        }

        public Tilemap(int[,] mapData, char[,] metaData) : this(mapData, metaData,true,8,18f)
        {
            
        }

        

        public int[,] GetMapData()
        {
            return mMapData;
        }
        private bool PositionIsOutOfMapBounds(int mapX, int mapY)
        {
            return mapX < 0 || mapX >= mMapWidth || mapY < 0 || mapY >= mMapHeight;
        }
        public int GetCellDataByPosition(Point cell)
        {
            return (PositionIsOutOfMapBounds(cell.X,cell.Y)) ? 0 : mMapData[cell.Y, cell.X];
        }

        public int GetCellDataByPosition(Vector2 spawnPos)
        {
            return mMapData[(int) spawnPos.Y, (int) spawnPos.X];
        }
        public char GetCellMetaDataByPosition(Vector2 position)
        {
            return GetCellMetaDataByPosition((int) position.X, (int) position.Y);
        }


        public char GetCellMetaDataByPosition(Point position)
        {
            return GetCellMetaDataByPosition(position.X, position.Y);
        }

        public char GetCellMetaDataByPosition(int mapX, int mapY)
        {
            if (PositionIsOutOfMapBounds(mapX,mapY))
                return ' ';
            return mMapMetaData[mapY, mapX];
        }

        public HashSet<Entity> GetEntitiesInRange(Vector2 sourcePosition, float range)
        {
            HashSet<Entity> inRange = new HashSet<Entity>();
            int minX = (int) MathHelper.Clamp((float) Math.Floor(sourcePosition.X - range), 0, mMapWidth - 1);
            int maxX = (int) MathHelper.Clamp((float) Math.Ceiling(sourcePosition.X + range), 0, mMapWidth - 1);

            int minY = (int) MathHelper.Clamp((float) Math.Floor(sourcePosition.Y - range), 0, mMapHeight - 1);
            int maxY = (int) MathHelper.Clamp((float) Math.Ceiling(sourcePosition.Y + range), 0, mMapHeight - 1);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    foreach (Entity entity in Entities[y, x])
                    {
                        Transform transform = entity.GetComponent<Transform>();
                        if (Vector2.Distance(sourcePosition,transform.Position) <= range)
                        {
                            inRange.Add(entity);
                        }
                    }
                }
            }
            return inRange;
        }

        public Color GetShadingColor(Color source, double dist)
        {
            if (dist > mShadingCutOff) dist = mShadingCutOff;

            float lerpFact = (float)(dist / mShadingDivisor);
            return Color.Lerp(source, Color.Black, lerpFact);
        }

        public Color GetShadingColor(double dist)
        {
            return GetShadingColor(Color.White, dist);
        }


        public static Tilemap FromScratch(int mapWidth, int mapHeight)
        {
            int[,] mapdataArray = new int[mapHeight, mapWidth];
            char[,] mapMetaData = new char[mapHeight, mapWidth];
            Tilemap newMap = new Tilemap(mapdataArray, mapMetaData, true, 8, 18) { CeilingColor = new Color(2, 19, 48), FloorColor = new Color(128, 128, 128), PlayerSpawn = Vector2.Zero, PlayerViewDirection = Vector2.UnitY };
            return newMap;
        }

        public static Tilemap FromFile(string filename)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(filename);

            List<List<char>> mapdata = new List<List<char>>();

            bool readingMapData = false;

            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("#")) continue;
                if (line.Trim().ToLower() == "[mapdata]")
                {
                    readingMapData = true;
                    continue;
                }

                if (!readingMapData)
                {
                    if (!line.Contains("=")) continue;
                    string[] keyValue = line.Split('=');
                    string key = keyValue[0].Trim().ToLower();
                    string value = keyValue[1].Trim();
                    settings[key] = value;
                }
                else if (line.Trim().Count() > 0)
                {
                    List<char> row = line.ToList();
                    //row.Reverse();
                    mapdata.Add(row);
                }
            }

            int mapHeight = mapdata.Count;
            int mapWidth = MaxCount(mapdata);
            Vector2 startPos = Vector2.Zero;
            Dictionary<Vector2,char> spawns = new Dictionary<Vector2, char>();

            int[,] mapdataArray = new int[mapHeight, mapWidth];
            char[,] mapMetaData = new char[mapHeight,mapWidth];
            HashSet<char> spawnChars = new HashSet<char>() { '"', '!', '%', '+', '#', '-', '=', '/', '(', ')', '{', '[','Ü','Ö','Ä','ü' };
            HashSet<char> extendedTextureChars = new HashSet<char>() { 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', }; // o is 111 in ascii
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int result;
                    if (x > mapdata[y].Count - 1)
                    {
                        // need padding..
                        break;
                    }
                    char c = mapdata[y][x];
                    if (extendedTextureChars.Contains(c))
                    {
                        mapdataArray[y, x] = c - 100;
                    }
                    else if (int.TryParse(c.ToString(), out result))
                    {
                        mapdataArray[y, x] = result == 0 ? 10 : result;
                    }
                    else
                    {
                        switch (c)
                        {
                            case '*':
                                startPos = new Vector2(x + 0.5f, y + 0.5f);
                                break;
                            case '=':
                            case '#':
                                mapMetaData[y, x] = c;
                                spawns[new Vector2(x + 0.5f, y + 0.5f)] = c;
                                break;
                            default:
                                if (Char.IsLetter(c) || spawnChars.Contains(c)) // things
                                {
                                    spawns[new Vector2(x + 0.5f, y + 0.5f)] = c;
                                }
                                else if (c != ' ') // metadata, eg. patrol markers
                                {
                                    mapMetaData[y, x] = c;
                                    if (c == '§')   // secret wall uses tex of neighbor walls
                                    {
                                        int neighborWallTex = mapdataArray[y - 1, x] > 0 ? mapdataArray[y - 1, x] : mapdataArray[y, x - 1];
                                        mapdataArray[y, x] = neighborWallTex;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            bool moreTextures = true;
            int texIndex = 1;
            List<Texture2D> textures = new List<Texture2D>();
            while (moreTextures)
            {
                if (!settings.ContainsKey("tex"+texIndex))
                {
                    moreTextures = false;
                    continue;
                }
                textures.Add(AssetManager.Default.LoadTexture(settings["tex" + texIndex]));
                texIndex++;
            }

            bool pseudoshading = bool.Parse(settings["shading"]);
            int shadingcutoff = int.Parse(settings["shadingcutoff"]);
            float shadingdivisor = float.Parse(settings["shadingdivisor"]);
            Color ceilingColor = ParseColor(settings["colorceiling"]);
            Color floorColor = ParseColor(settings["colorfloor"]);
            //Vector2 startPos = ParseVector2(settings["playerposition"]);
            //startPos.X = mapWidth - startPos.X;
            Vector2 lookdir = ParseVector2(settings["playerlook"]);

            //bool pseudoshading = bool.Parse(settings["Shading"]);
            Tilemap newMap = new Tilemap(mapdataArray,mapMetaData, pseudoshading, shadingcutoff, shadingdivisor)
                                 {CeilingColor = ceilingColor, FloorColor = floorColor, PlayerSpawn = startPos, PlayerViewDirection = lookdir};
            return newMap;
        }


        private static int MaxCount(List<List<char>> mapdata)
        {
            int maxCount = 0;
            foreach (List<char> list in mapdata)
            {
                if (list.Count > maxCount)
                    maxCount = list.Count;
            }
            return maxCount;
        }

        private static Vector2 ParseVector2(string s)
        {
            string[] components = s.Split(',');
            return new Vector2(float.Parse(components[0]), float.Parse(components[1]));
        }

        private static Color ParseColor(string s)
        {
            string[] components = s.Split(',');
            return new Color(int.Parse(components[0]), int.Parse(components[1]), int.Parse(components[2]));
        }


        public bool HasLineOfSight(Vector2 sourcePos, Vector2 lookAtPosition, Vector2 lookDir, int fovInDegrees)
        {
            bool looksInDirectionOf = TwenMath.IsInViewCone(sourcePos, lookAtPosition, lookDir, fovInDegrees);
            if (!looksInDirectionOf) return false;
            mCurrentLine.Clear();
            bool hasLineOfSight = TwenMath.GridRayTrace(sourcePos.X, sourcePos.Y, lookAtPosition.X, lookAtPosition.Y, IsTransparent);
            mLineOfSightList.Add(mCurrentLine);
            return hasLineOfSight;
        }

        private bool IsTransparent(int x, int y)
        {
            mCurrentLine.Add(new Point(x, y));
            if (Entities[y, x].Count > 0)
            {
                List<Entity> entities = Entities[y, x];
                foreach (Entity entity in entities)
                {
                    if (entity.Group == "Door")
                    {
                        Door door = entity.GetComponent<Door>();
                        if (!door.IsOpen) return false;
                    }
                }
            }
            return mMapData[y, x] == 0;
        }

        public List<List<Point>> GetLastLineOfSightLines()
        {
            List<List<Point>> lastLineOfSightLines = new List<List<Point>>(mLineOfSightList);
            mLineOfSightList.Clear();
            return lastLineOfSightLines;
        }
        private void UpdateEntityPosition(Entity entity)
        {
            Transform transform = entity.GetComponent<Transform>();

            Entities[transform.LastCellPosition.Y, transform.LastCellPosition.X].Remove(entity);
            

            int y = (int)transform.Position.Y;
            int x = (int)transform.Position.X;

            Entities[y, x].Add(entity);
            
            

            transform.LastCellPosition = new Point(x, y);
        }

        private void DebugPrintPlayerPositions()
        {
            foreach (Point point in mPlayerPositionHistory)
            {
                string entityString = "at " + point + ": " + ComponentTwengine.GetEntityString(Entities[point.Y, point.X]);
                DebugDrawer.DrawString(entityString);
            }
        }

        public void AddEntity(Entity entity)
        {
            Transform transform = entity.GetComponent<Transform>();
            Entities[(int)transform.Position.Y, (int)transform.Position.X].Add(entity);
            transform.LastCellPosition = new Point((int)transform.Position.X, (int)transform.Position.Y);
            AllEntities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            Transform transform = entity.GetComponent<Transform>();
            Entities[transform.LastCellPosition.Y, transform.LastCellPosition.X].Remove(entity);
            AllEntities.Remove(entity);
        }


        public void RemoveAllEntities()
        {
            for (int i = AllEntities.Count - 1; i >= 0; i--)
            {
                Entity toDelete = AllEntities[i];
                RemoveEntity(toDelete);
            }
        }

        public void UpdateEntities()
        {
            for (int i = AllEntities.Count - 1; i >= 0; i--)
            {
                Entity entity = AllEntities[i];
                UpdateEntityPosition(entity);
            }
        }

        public void CreateWall(Point point, int wallIndex)
        {
            mMapData[point.Y, point.X] = wallIndex + 1;
        }

        public void CreateDoor(Point position, int wallIndex)
        {
            mMapMetaData[position.Y, position.X] = 'd';
            DoorSpawnPoints[position] = wallIndex;
        }

        public void CreateMessageTrigger(Point cell, MessageTriggerInfo info)
        {
            mMapMetaData[cell.Y, cell.X] = 'm';
            MessageTriggers[cell] = info;
        }

        public void CreateSecretWall(Point point)
        {
            mMapMetaData[point.Y, point.X] = 's';
            int neighborWallTex = mMapData[point.Y - 1, point.X] > 0 ? mMapData[point.Y - 1, point.X] : mMapData[point.Y, point.X - 1];
            mMapData[point.Y, point.X] = neighborWallTex;
        }

        public void CreatePatrolTurnPoint(Point point, char dir)
        {
            mMapMetaData[point.Y, point.X] = dir;
        }


        public void DestroyWall(Point cell)
        {
            mMapData[cell.Y, cell.X] = 0;
        }


        public void SetKeyNeed(Point position, string keyName)
        {
            switch (keyName)
            {
                case "Gold":
                    mMapMetaData[position.Y, position.X] = 'g';
                    break;
                case "Silver":
                    mMapMetaData[position.Y, position.X] = 'h';
                    break;
            }
        }
    }
}
