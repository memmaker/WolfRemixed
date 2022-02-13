using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;

namespace Twengine.Managers
{
    public class SpawnEntityEventArgs : EventArgs
    {
        public Point Position { get; set; }
        public int TileIndex { get; set; }
        public TiledMapProperties Properties { get; set; }
    }
    public class MapManager
    {
        public event EventHandler<SpawnEntityEventArgs> CreateWalls;
        public event EventHandler<SpawnEntityEventArgs> CreateItems;
        public event EventHandler<SpawnEntityEventArgs> CreateEnemies;
        public event EventHandler<SpawnEntityEventArgs> CreateMetaInfo;

        protected TiledMap mMap;
        private ContentManager mContentManager;
        private Dictionary<string, Texture2D> mTextures;

        public MapManager(ContentManager content)
        {
            mContentManager = content;
            mTextures = new Dictionary<string, Texture2D>();
        }


        public Texture2D GetTileSheet(string tileSheetId)
        {
            return mTextures[tileSheetId];
        }

        public void LoadMap(string map)
        {
            Dictionary<string, string> tileSetNames = new Dictionary<string, string>() {
                { "Maps\\wolfenemies_sheet_0", "Enemies"},
                { "Icons\\meta_sheet_0", "MetaSheet"},
                { "Maps\\wolfwalls_sheet_0", "WallTextures"},
                { "Maps\\wolfitems_sheet_ext_0", "WolfItems"},
            };
            mMap = mContentManager.Load<TiledMap>(map);
            foreach (TiledMapTileset tileSheet in mMap.Tilesets)
            {
                mTextures[tileSetNames[tileSheet.Name]] = tileSheet.Texture;

            }
            TiledMapTileLayer wallLayer = mMap.GetLayer<TiledMapTileLayer>("Walls");
            //CollisionDetector.RegisterCollisionLayer(MapViewport, collisionLayer);
            if (CreateWalls != null)
            {
                ProcessAllTiles(mMap, wallLayer, CreateWalls);
            }
            TiledMapTileLayer itemLayer = mMap.GetLayer<TiledMapTileLayer>("Items");

            if (CreateItems != null)
            {
                ProcessAllTiles(mMap, itemLayer, CreateItems);
            }

            TiledMapTileLayer metaLayer = mMap.GetLayer<TiledMapTileLayer>("MetaInfo");
            if (CreateMetaInfo != null)
            {
                ProcessAllTiles(mMap, metaLayer, CreateMetaInfo);
            }

            TiledMapTileLayer enemyLayer = mMap.GetLayer<TiledMapTileLayer>("Enemies");
            if (CreateEnemies != null)
            {
                ProcessAllTiles(mMap, enemyLayer, CreateEnemies);
            }
        }

        private void ProcessAllTiles(TiledMap map, TiledMapTileLayer layer, EventHandler<SpawnEntityEventArgs> function)
        {
            TiledMapObjectLayer objectLayer = map.ObjectLayers[0];
            SortedList<int, TiledMapTileset> tilesets = new System.Collections.Generic.SortedList<int, TiledMapTileset>();
            foreach (TiledMapTileset tileSheet in mMap.Tilesets)
            {
                int tileSheetStartIndex = mMap.GetTilesetFirstGlobalIdentifier(tileSheet);
                tilesets.Add(tileSheetStartIndex, tileSheet);
            }

            for (ushort x = 0; x < layer.Width; x++)
            {
                for (ushort y = 0; y < layer.Height; y++)
                {
                    TiledMapTile? outTile = null;
                    if (!layer.TryGetTile(x, y, out outTile)) continue;
                    TiledMapTile tile = outTile.Value;
                    //Rectangle tileDisplayRectangle = layer.GetTileDisplayRectangle(MapViewport, new Location(x, y));
                    //Vector2 pos = new Vector2(tileDisplayRectangle.X + (tileDisplayRectangle.Width/2), tileDisplayRectangle.Y + (tileDisplayRectangle.Height/2));
                    //Vector2 pos = new Vector2(x+0.5f,y+0.5f);
                    int tileIndex = tile.GlobalIdentifier;

                    foreach (var kvp in tilesets)
                    {
                        var tileSheet = kvp.Value;
                        int tileSheetStartIndex = mMap.GetTilesetFirstGlobalIdentifier(tileSheet);
                        if (tileSheetStartIndex <= tile.GlobalIdentifier)
                        {
                            tileIndex = tile.GlobalIdentifier - tileSheetStartIndex;
                        }
                    }

                    Point pos = new Point(x, y);
                    TiledMapProperties props = new TiledMapProperties();
                    foreach (var thing in objectLayer.Objects)
                    {
                        int propX = (int)(thing.Position.X / thing.Size.Width);
                        int propY = (int)(thing.Position.Y / thing.Size.Height);
                        if (propX == x && propY == y)
                        {
                            props = thing.Properties;
                        }
                    }
                    if (!tile.IsBlank)
                        function(this, new SpawnEntityEventArgs() { Position = pos, TileIndex = tileIndex, Properties = props });
                }
            }
        }

    }
}
