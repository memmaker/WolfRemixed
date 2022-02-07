using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Twengine.Components;
using Twengine.Helper;
using XNAHelper;


namespace Twengine.Managers
{
    public class AssetManager
    {
        private static readonly AssetManager sInstance = new AssetManager();
        private ContentManager mContentManager;
        private readonly Dictionary<string, Texture2D> mTextureCache;
        private readonly Dictionary<string, SpriteSheet> mSpriteSheetCache;
        private readonly Dictionary<Texture2D, List<Sprite>> mSpriteTextureMap;
        private string mContentpath;
        private GraphicsDevice mGraphicsDevice;

        public static AssetManager Default
        {
            get { return sInstance; }
        }

        private AssetManager()
        {
            mSpriteTextureMap = new Dictionary<Texture2D, List<Sprite>>();
            mTextureCache = new Dictionary<string, Texture2D>();
            mSpriteSheetCache = new Dictionary<string, SpriteSheet>();
        }

        internal void Init(Game twengine)
        {
            mContentManager = twengine.Content;
            mGraphicsDevice = twengine.GraphicsDevice;
        }

        public void Init(ContentManager contentManager, GraphicsDevice device)
        {
            mContentManager = contentManager;
            mGraphicsDevice = device;
        }

        internal void RemoveSprite(Sprite s)
        {
            mSpriteTextureMap[s.SpriteSheet.Texture].Remove(s);
        }

        internal void RegisterSprite(Sprite s)
        {
            if (!mSpriteTextureMap.ContainsKey(s.SpriteSheet.Texture))
            {
                mSpriteTextureMap[s.SpriteSheet.Texture] = new List<Sprite>();
            }
            mSpriteTextureMap[s.SpriteSheet.Texture].Add(s);
        }

        public List<Sprite> GetSpritesSortedByTexture()
        {
            var l = new List<Sprite>();
            foreach (var kvp in mSpriteTextureMap)
            {
                l.AddRange(kvp.Value);
            }
            return l;
        }

        public SpriteFont LoadFont(string fontname)
        {
            SpriteFont sf = mContentManager.Load<SpriteFont>(fontname);
            return sf;
        }

        public Texture2D LoadTexture(string assetname)
        {
            if (!mTextureCache.ContainsKey(assetname))
            {
                FileStream fs = new FileStream(mContentManager.RootDirectory + "/" + assetname, FileMode.Open);
                mTextureCache[assetname] = Texture2D.FromStream(mGraphicsDevice, fs);
                fs.Close();
            }
            return mTextureCache[assetname];
        }

        public SpriteSheet LoadSpriteSheet(string assetname, int frameWidth, int frameHeight)
        {
            if (!mSpriteSheetCache.ContainsKey(assetname))
            {
                mSpriteSheetCache.Add(assetname, new SpriteSheet(LoadTexture(assetname), frameWidth, frameHeight));
            }
            return mSpriteSheetCache[assetname];
        }

        public string ContentPath
        {
            get { return mContentpath; }
            set
            {
                mContentpath = value + "/";
            }
        }

    }
}