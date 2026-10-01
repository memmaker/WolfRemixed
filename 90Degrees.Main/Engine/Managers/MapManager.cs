using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if BLAZORGL
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using TiledMapProperties = System.Collections.Generic.Dictionary<string, string>;
#else
using MonoGame.Extended.Tiled;
#endif
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

#if !BLAZORGL
        protected TiledMap mMap;
#endif
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

#if BLAZORGL
        // MonoGame.Extended does not run on KNI, so the web build reads the .tmx directly.
        // ponytail: handles only what our maps use (base64+zlib tile layers, object props, object templates)
        private static readonly Dictionary<string, string> sTileSheetAssets = new Dictionary<string, string>() {
            { "Enemies", "Maps/wolfenemies_sheet"},
            { "MetaSheet", "Icons/meta_sheet"},
            { "WallTextures", "Maps/wolfwalls_sheet"},
            { "WolfItems", "Maps/wolfitems_sheet_ext"},
        };

        private XElement LoadXml(string path)
        {
            using Stream s = TitleContainer.OpenStream(mContentManager.RootDirectory + "/" + path);
            return XDocument.Load(s).Root;
        }

        public void LoadMap(string map)
        {
            XElement root = LoadXml(map + ".tmx");
            string mapDir = Path.GetDirectoryName(map).Replace('\\', '/');
            int width = (int)root.Attribute("width");
            var firstGids = new SortedList<int, string>();
            foreach (XElement ts in root.Elements("tileset"))
            {
                string name = Path.GetFileNameWithoutExtension((string)ts.Attribute("source"));
                firstGids.Add((int)ts.Attribute("firstgid"), name);
                mTextures[name] = mContentManager.Load<Texture2D>(sTileSheetAssets[name]);
            }

            var objects = new List<(Point cell, TiledMapProperties props)>();
            foreach (XElement obj in root.Elements("objectgroup").Take(1).SelectMany(g => g.Elements("object")))
            {
                XElement tpl = obj.Attribute("template") != null ? LoadXml(mapDir + "/" + (string)obj.Attribute("template")).Element("object") : null;
                float Attr(string n) => (float?)obj.Attribute(n) ?? (float?)tpl?.Attribute(n) ?? 0f;
                var props = new TiledMapProperties();
                foreach (XElement src in new[] { tpl, obj })
                    foreach (XElement p in src?.Element("properties")?.Elements("property") ?? Enumerable.Empty<XElement>())
                        props[(string)p.Attribute("name")] = (string)p.Attribute("value");
                objects.Add((new Point((int)(Attr("x") / Attr("width")), (int)(Attr("y") / Attr("height"))), props));
            }

            foreach (var (layerName, handler) in new[] { ("Walls", CreateWalls), ("Items", CreateItems), ("MetaInfo", CreateMetaInfo), ("Enemies", CreateEnemies) })
            {
                XElement layer = root.Elements("layer").FirstOrDefault(l => (string)l.Attribute("name") == layerName);
                if (handler == null || layer == null) continue;
                byte[] raw = Convert.FromBase64String(layer.Element("data").Value.Trim());
                using var z = new ZLibStream(new MemoryStream(raw), CompressionMode.Decompress);
                using var ms = new MemoryStream();
                z.CopyTo(ms);
                byte[] data = ms.ToArray();
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < data.Length / 4 / width; y++)
                    {
                        int gid = (int)(BitConverter.ToUInt32(data, (y * width + x) * 4) & 0x1FFFFFFF);
                        if (gid == 0) continue;
                        int tileIndex = gid;
                        foreach (int start in firstGids.Keys)
                            if (start <= gid) tileIndex = gid - start;
                        Point pos = new Point(x, y);
                        TiledMapProperties props = objects.LastOrDefault(o => o.cell == pos).props ?? new TiledMapProperties();
                        handler(this, new SpawnEntityEventArgs() { Position = pos, TileIndex = tileIndex, Properties = props });
                    }
                }
            }
        }
#else
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

#endif
    }
}
