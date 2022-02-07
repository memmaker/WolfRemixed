using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using raycaster.GameStates;
using raycaster.Scripts;
using raycaster.StoryTelling;
using Twengine;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Datastructures;
using Twengine.Helper;
using Twengine.Managers;
using Twengine.SubSystems;
using Twengine.SubSystems.Raycast;
using XNAGameGui.Gui;
using XNAGameGui.Gui.Widgets;
using System.Configuration;
using XNAHelper;
using Artemis.Manager;
using Artemis.Utils;

namespace raycaster
{

    class RaycastGame : ComponentTwengine
    {
        private static Raycaster mRaycaster;
        private static RaycastRenderSystem sRaycastRenderSystem;
        public static bool FinishedLoading { get; set; }
        private static bool mLowResRaytracing;

        private static RaycastMapRenderer mMapRenderer;
        private static TilemapCollisionSystem mTilemapCollisionSystem;
        private static TilemapEntityCollisionSystem mTilemapEntityCollisionSystem;

        private static Tilemap mTilemap;
        
        private static string[] mMapList;
        private static int mCurrentMapIndex;

        public static Entity Player { get; private set; }
        public static int PlayerAmmo { get; set; }
        public static Dictionary<string, bool> PlayerKeys { get; set; }
        public static HealthPoints PlayerHealthPoints { get; private set; }
        private static Transform mPlayerTransform;
        public static Weapon CurrentWeapon { get; private set; }

        private SpriteFont mHudFont;
        private SpriteFont mMessageFont;

        private static FPSControlSystem mFpsControlSystem;
        protected static CollisionSystem mCollisionSystem;
        private static SpriteAnimator mHudFaceAnimator;
        private SpriteFont mLongTextFont;

        private bool mFullscreen;
        private float mMouseSensitivity;
        private static HashSet<Entity> mEnergyBarriers;
        private static HashSet<Entity> mEnergyReactorsAlive;
        private static HashSet<Entity> mRedLights;
        private static HashSet<Entity> mHiddenMonsters;

        private static List<CallbackTimer> mTimers;
        private static LabelWidget mLoadingScreen;
        private Rectangle mZombieSpawnArea;
        private CallbackTimer mZombieSpawnTimer;
        private Rectangle mZombieTriggerArea;
        private static EventHandler<EventArgs> sResetCallback;
        private bool mSecretWallsVisible;


        public RaycastGame() : base(true, false)
        {
            // XNA
            Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int width, height;
            if (Config.AppSettings.Settings.Count == 0)
            {
                mMouseSensitivity = 0.08f;
                mFullscreen = false;
                mLowResRaytracing = false;
                width = 640;
                height = 360;
                mSecretWallsVisible = false;
            }
            else
            {
                mMouseSensitivity = float.Parse(Config.AppSettings.Settings["MouseSensitivity"].Value);
                mFullscreen = bool.Parse(Config.AppSettings.Settings["Fullscreen"].Value);
                mLowResRaytracing = bool.Parse(Config.AppSettings.Settings["LowResRaycasting"].Value);
                width = int.Parse(Config.AppSettings.Settings["ScreenWidth"].Value);
                height = int.Parse(Config.AppSettings.Settings["ScreenHeight"].Value);
                mSecretWallsVisible = bool.Parse(Config.AppSettings.Settings["SecretWallsVisible"].Value);
            }

            FinishedLoading = false;
            IsMouseVisible = false;
            IsFixedTimeStep = true;
            
            mEnergyBarriers = new HashSet<Entity>();
            mEnergyReactorsAlive = new HashSet<Entity>();
            mRedLights = new HashSet<Entity>();
            mHiddenMonsters = new HashSet<Entity>();
            mTimers = new List<CallbackTimer>();
            // misc 
            ChangeResolution(width, height, mFullscreen);

            mMapList = new[]{"map1"};//,"map02.txt"};
            mCurrentMapIndex = 0;

            PlayerAmmo = 0;
            ResetKeys();
            mZombieSpawnCounter = 0;

            mZombieSpawnArea = new Rectangle(50, 20, 11, 6);
            mZombieTriggerArea = new Rectangle(45, 12, 17, 14);

        }

        protected override void Initialize()
        {
            ChangeResolution(640, 360, false);

            base.Initialize();
        }

        public static new void ChangeResolution(int screenWidth, int screenHeight, bool fullscreen)
        {
            sScreenWidth = screenWidth;
            sScreenHeight = screenHeight;
            Graphics.PreferredBackBufferWidth = sScreenWidth;
            Graphics.PreferredBackBufferHeight = sScreenHeight;
            Graphics.IsFullScreen = fullscreen;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Graphics.ApplyChanges();
            if (Graphics.GraphicsDevice != null) GameGui.Viewport = Graphics.GraphicsDevice.Viewport;

            if (sRaycastRenderSystem != null) sRaycastRenderSystem.ViewportChanged(screenWidth, screenHeight);
        }


        private static void CreateConfiguration()
        {
            Config.AppSettings.Settings.Add("Fullscreen", Graphics.IsFullScreen.ToString());
            Config.AppSettings.Settings.Add("ScreenWidth", sScreenWidth.ToString());
            Config.AppSettings.Settings.Add("ScreenHeight", sScreenHeight.ToString());
            Config.AppSettings.Settings.Add("LowResRaycasting", mLowResRaytracing.ToString());
            Config.AppSettings.Settings.Add("MouseSensitivity", mFpsControlSystem.MouseSensitivity.ToString());
            Config.AppSettings.Settings.Add("SecretWallsVisible", sRaycastRenderSystem.SecretWallsVisible.ToString());
            Config.Save(ConfigurationSaveMode.Full);
        }

        public static void SaveConfiguration()
        {
            if (Config.AppSettings.Settings.Count == 0)
            {
                CreateConfiguration();
                return;
            }
            Config.AppSettings.Settings["Fullscreen"].Value = Graphics.IsFullScreen.ToString();
            Config.AppSettings.Settings["ScreenWidth"].Value = sScreenWidth.ToString();
            Config.AppSettings.Settings["ScreenHeight"].Value = sScreenHeight.ToString();
            Config.AppSettings.Settings["LowResRaycasting"].Value = mLowResRaytracing.ToString();
            Config.AppSettings.Settings["MouseSensitivity"].Value = mFpsControlSystem.MouseSensitivity.ToString();
            Config.AppSettings.Settings["SecretWallsVisible"].Value = sRaycastRenderSystem.SecretWallsVisible.ToString();
            Config.Save(ConfigurationSaveMode.Full);
        }

        public static Configuration Config { get; set; }

        #region initialization

        protected override void AddSubSystems()
        {
            mMapManager.CreateWalls += mMapManager_CreateWalls;
            mMapManager.CreateMetaInfo += mMapManager_CreateMetaInfo;
            mMapManager.CreateItems += mMapManager_CreateItems;
            mMapManager.CreateEnemies += mMapManager_CreateEnemies;

            LoadMap(mMapList[mCurrentMapIndex]);

            sRaycastRenderSystem = new RaycastRenderSystem(mSpriteBatch, AssetManager.Default, mTilemap, sScreenWidth, sScreenHeight, mLowResRaytracing) { SecretWallsVisible = mSecretWallsVisible };
            mRaycaster = sRaycastRenderSystem.Raycaster;
            
            mSystemManager.SetSystem(sRaycastRenderSystem,  GameLoopType.Draw);

            mSystemManager.SetSystem(new RayAnimationSystem(mRaycaster),  GameLoopType.Update);

            mSystemManager.SetSystem(new DoorMovementSystem(mTilemap),  GameLoopType.Update);

            mFpsControlSystem = new FPSControlSystem(mRaycaster, sScreenWidth, sScreenHeight, mMouseSensitivity);
            mFpsControlSystem.PlayerPressedFire += PlayerPressedFire;
            mFpsControlSystem.PlayerUsed += PlayerUsed;
            mFpsControlSystem.ToggleMap += (o, args) => mMapRenderer.DrawMap = !mMapRenderer.DrawMap;

            mFpsControlSystem.MapZoomIn += delegate { mRaycaster.Camera.EyeHeight--; EntitySpawn.CreateMessage("Eyeheight:" + mRaycaster.Camera.EyeHeight); };
            //mFpsControlSystem.MapZoomOut += delegate { mRaycaster.Camera.EyeHeight++; EntitySpawn.CreateMessage("Eyeheight:" + mRaycaster.Camera.EyeHeight); };
            mFpsControlSystem.MapZoomOut += delegate { SpawnInZombieArea(); };
            mFpsControlSystem.ChangeWeapon += (o, args) => ChangePlayerWeapon(args.WeaponIndex);
            mFpsControlSystem.PlayerMovedIntoTile += PlayerMovedIntoTile;
            mFpsControlSystem.ActivatedWeaponCheat += (o, args) => AllWeaponsCheatActivated();

            /*
            mFpsControlSystem.MapZoomIn += (o, args) => mMapRenderer.GridSizeInPixels++;
            mFpsControlSystem.MapZoomOut += (o, args) => mMapRenderer.GridSizeInPixels--;
            */
            mSystemManager.SetSystem(mFpsControlSystem,  GameLoopType.Update);


            mMapRenderer = new RaycastMapRenderer(mSpriteBatch, AssetManager.Default, mTilemap, sScreenWidth, sScreenHeight, mHudFont, mRaycaster);
            mSystemManager.SetSystem(mMapRenderer,  GameLoopType.Draw);

            FpsWeaponSystem fpsWeaponSystem = new FpsWeaponSystem(mRaycaster);
            fpsWeaponSystem.PlayerUsedAmmo += args => PlayerAmmo--;
            fpsWeaponSystem.PlayerWeaponFired += args =>
                                                     {
                                                         if (CurrentWeapon.FireSoundCue != null)
                                                             AudioManager.PlaySound(CurrentWeapon.FireSoundCue);
                                                         if (!CurrentWeapon.IsSilent)
                                                            AlertNonHiddenEnemiesNearPosition(mPlayerTransform.Position);
                                                     };
            fpsWeaponSystem.BulletHit += BulletHit;
            fpsWeaponSystem.DamageDealt += new DamageDealtEventHandler(fpsWeaponSystem_DamageDealt);
            mSystemManager.SetSystem(fpsWeaponSystem,  GameLoopType.Update);

            mSystemManager.SetSystem(new FpsWeaponAnimationSystem(),  GameLoopType.Update);

            mTilemapEntityCollisionSystem = new TilemapEntityCollisionSystem(mRaycaster, mTilemap);
            mSystemManager.SetSystem(mTilemapEntityCollisionSystem, GameLoopType.Update);

            mTilemapCollisionSystem = new TilemapCollisionSystem(mTilemap, mRaycaster);
            mTilemapCollisionSystem.PlayerFoundSecret += PlayerFoundSecret;
            mSystemManager.SetSystem(mTilemapCollisionSystem, GameLoopType.Update);

            mSystemManager.SetSystem(new MessageRenderSystem(mSpriteBatch, mMessageFont),  GameLoopType.Draw);

            mSystemManager.SetSystem(new SpriteRenderSystem(mSpriteBatch, null), GameLoopType.Draw);  // default 2d sprite rendering

            EntitySpawn.Init(sWorld, mTilemap);

            /*
            EntitySpawn.SpawnThings();

            InitialSpawnPlayer();
            
            */

        }

        void fpsWeaponSystem_DamageDealt(DamageDealtEventArgs args)
        {
            foreach (KeyValuePair<Entity, int> keyValuePair in args.DamageDealt)
            {
                EntitySpawn.CreateMessage("Damage: " + keyValuePair.Value);
            }
        }

        static void BulletHit(HitLocationEventArgs args)
        {
            
            EntitySpawn.CreateBulletHitAnimation(args.HitLocation, args.IsEnemyHit);
        }


        public static void SpawnThings()
        {
            int spawnCounter = 0;
            mRedLights.Clear();
            mEnergyReactorsAlive.Clear();
            mEnergyBarriers.Clear();
            if (mTilemap != null)
            {
                foreach (KeyValuePair<Point, int> keyValuePair in mTilemap.DoorSpawnPoints)
                {
                    Point spawnPos = keyValuePair.Key;
                    int tileIndex = keyValuePair.Value;
                    Point leftNeighborPos = new Point(spawnPos.X - 1, spawnPos.Y);
                    if (mTilemap.GetCellDataByPosition(leftNeighborPos) > 0)    // wall left of door
                        EntitySpawn.CreateDoor(spawnPos, Orientation.Horizontal, mTilemap.WallTextures, tileIndex);
                    else
                        EntitySpawn.CreateDoor(spawnPos, Orientation.Vertical, mTilemap.WallTextures, tileIndex);
                    spawnCounter++;
                }
                foreach (KeyValuePair<Vector2, int> keyValuePair in mTilemap.ItemSpawnPoints)
                {
                    Vector2 spawnPos = keyValuePair.Key;
                    int tileIndex = keyValuePair.Value;

                    switch (tileIndex)
                    {
                        case 0: // ROTT Pistol
                            EntitySpawn.CreateWeaponPickup(mTilemap.SpriteTextures, spawnPos, tileIndex, "hmm, a H.U.N.T. Pistol..", 2, EntitySpawn.CreateRottPistol);
                            break;
                        case 1: // Wolf Assault Rifle
                            EntitySpawn.CreateWeaponPickup(mTilemap.SpriteTextures, spawnPos, tileIndex, "Time to mow down some Nazis..", 3, EntitySpawn.CreateWolfRifle);
                            break;
                        case 2: // Wolf Gatling Gun
                            EntitySpawn.CreateWeaponPickup(mTilemap.SpriteTextures, spawnPos, tileIndex, "More lead, more death!", 4, EntitySpawn.CreateWolfGatling);
                            break;
                        case 3: // Pistol
                            EntitySpawn.CreateWeaponPickup(mTilemap.SpriteTextures, spawnPos, tileIndex, "Blake won't need this..", 5, EntitySpawn.CreateBlakeStoneAutoChargePistol);
                            break;
                        case 113: // necronomicon
                            EntitySpawn.CreateWeaponPickup(mTilemap.SpriteTextures, spawnPos, tileIndex, "Klaatu Verata Nektu", 6, EntitySpawn.CreateMagicHand);
                            break;
                        case 4: // ammo
                            EntitySpawn.CreateAmmoPickup(spawnPos);
                            break;
                        case 5: // healing
                            EntitySpawn.CreateFoodPickup(spawnPos, mTilemap.SpriteTextures, tileIndex, 5, "Ieeks, dog food..");
                            break;
                        case 6: // healing
                            EntitySpawn.CreateFoodPickup(spawnPos, mTilemap.SpriteTextures, tileIndex, 15, "You found some food. Yummy!");
                            break;
                        case 7: // healing
                            EntitySpawn.CreateFoodPickup(spawnPos, mTilemap.SpriteTextures, tileIndex, 25, "This was needed.");
                            break;
                        case 11:
                        case 12:
                        case 14:
                        case 16:
                        case 20:
                        case 35:
                        case 36:
                        case 41:
                        case 46:
                        case 51:
                        case 52:
                        case 53:
                        case 112:
                            EntitySpawn.CreateDecoSprite(mTilemap.SpriteTextures, spawnPos, tileIndex, false);
                            break;
                        case 63:
                            Entity redlight = EntitySpawn.CreateTwoStateAnimatedSprite(spawnPos, mTilemap.SpriteTextures,
                                                                                                     new List<int>() {63}, new List<int>() {63, 64},
                                                                                                     false);
                            mRedLights.Add(redlight);
                            break;
                        case 73: // energy barrier
                        case 74:
                        case 75:
                            Entity energyBarrier = EntitySpawn.CreateAnimatedHazard(spawnPos, mTilemap.SpriteTextures, new List<int>() { 73, 74, 75 }, new List<int> { 72 }, true);
                            mEnergyBarriers.Add(energyBarrier);
                            break;
                        case 76:    // reactor
                            Entity reactor = EntitySpawn.CreateDestroyableObstacle(mTilemap.SpriteTextures, spawnPos, tileIndex, 8, 3);
                            HealthPoints healthPoints = reactor.GetComponent<HealthPoints>();
                            healthPoints.Dying += delegate(object o, KilledEventArgs args)
                                                      {
                                                          mEnergyReactorsAlive.Remove(reactor);
                                                          ReactorDestroyed();
                                                      };
                            mEnergyReactorsAlive.Add(reactor);
                            break;
                        case 96: // zelda fire
                        case 97: // zelda fire
                            EntitySpawn.CreateAnimatedHazard(spawnPos, mTilemap.SpriteTextures, new List<int>() { 96, 97 }, new List<int>(), false);
                            break;
                        case 99:
                        case 100:
                        case 101:
                        case 102:
                            EntitySpawn.CreateHuntBody(tileIndex - 99, spawnPos);
                            break;
                        default:
                            EntitySpawn.CreateDecoSprite(mTilemap.SpriteTextures, spawnPos, tileIndex, true);
                            break;
                    }
                    spawnCounter++;
                }
                foreach (KeyValuePair<Vector2, int> keyValuePair in mTilemap.EnemySpawnPoints)
                {
                    Vector2 spawnPos = keyValuePair.Key;
                    int tileIndex = keyValuePair.Value;
                    //Debug.Print("Spawning Enemy at " + spawnPos + " with tileIndex " + tileIndex);
                    switch (tileIndex)
                    {
                        case 24:
                            mHiddenMonsters.Add(EntitySpawn.CreateCabinetGuy(spawnPos));
                            break;
                        case 26:
                            mHiddenMonsters.Add(EntitySpawn.CreateEggGuy(spawnPos));
                            break;
                        case 28:
                            mHiddenMonsters.Add(EntitySpawn.CreateGlassGuy(spawnPos));
                            break;
                        case 30:
                            mHiddenMonsters.Add(EntitySpawn.CreateStatueGuy(spawnPos));
                            break;
                        case 32:
                            mHiddenMonsters.Add(EntitySpawn.CreateTableGuy(spawnPos));
                            break;
                        case 35:
                            EntitySpawn.CreateZeldaOldMan(spawnPos);
                            break;
                        case 36:
                            EntitySpawn.CreateBat(spawnPos);
                            break;
                        case 37:
                            EntitySpawn.CreateBlueMonster(spawnPos);
                            break;
                        default:
                            {
                                Direction lookDir = Direction.Other;
                                switch (tileIndex % 4)
                                {
                                    case 0:
                                        lookDir = Direction.South;
                                        break;
                                    case 1:
                                        lookDir = Direction.West;
                                        break;
                                    case 2:
                                        lookDir = Direction.North;
                                        break;
                                    case 3:
                                        lookDir = Direction.East;
                                        break;
                                }
                                bool isPatrolling = (tileIndex % 8) > 3;
                                int actorIndex = (int)(tileIndex / 8.0f);
                                if (actorIndex == 2)
                                    EntitySpawn.CreateScientist(spawnPos, TwenMath.DirectionToRotation(lookDir), isPatrolling);
                                else
                                    EntitySpawn.CreateGuardDifferent(actorIndex, spawnPos, lookDir, isPatrolling);
                            }
                            break;
                    }
                    spawnCounter++;
                  
                }
            }
            Debug.Print("Spawned " + spawnCounter + " entities..");
            FinishedLoading = true;
        }

        private static void ReactorDestroyed()
        {
            if (mEnergyReactorsAlive.Count == 0)
            {
                AudioManager.PlaySound("AlarmSound");
                foreach (Entity energyBarrier in mEnergyBarriers)
                {
                    energyBarrier.Group = "Deco";
                    SpriteAnimator animator = energyBarrier.GetComponent<SpriteAnimator>();
                    Transform transform = energyBarrier.GetComponent<Transform>();
                    Collider collider = energyBarrier.GetComponent<Collider>();

                    transform.CollideWithEntityMap = false;
                    transform.CollideWithMap = false;

                    collider.CollidedWithEntity -= OnEntityCollidedWithHazard;
                    animator.StartPlay("AlternateState");
                }
                foreach (Entity redLight in mRedLights)
                {
                    SpriteAnimator animator = redLight.GetComponent<SpriteAnimator>();
                    animator.StartPlay("AlternateState");
                }
                foreach (Entity hiddenMonster in mHiddenMonsters)
                {
                    if (hiddenMonster.Group != "Enemy") continue;
                    MetaBehavior metaBehavior = hiddenMonster.GetComponent<MetaBehavior>();
                    ActorStateMachine actorStateMachine = metaBehavior.GetBehavior<ActorStateMachine>();
                    CallbackTimer timer = new CallbackTimer(delegate { actorStateMachine.ActorAlert(); }, TwenMath.Random.Next(4, 30), false);
                    mTimers.Add(timer);
                }
                mHiddenMonsters.Clear();
            }
        }


        /// <summary>
        /// This generates new SpriteSheet with their respective opaqueRectangles. This might take a while.
        /// The map is expected to be max. 64x64 cells big. Also the resolution of all wall and item textures is assumes to be 64x64.
        /// </summary>
        /// <param name="mapname">A filename in the Maps/ directory.</param>
        private static void LoadMap(string mapname)
        {
            mTilemap = Tilemap.FromScratch(64, 64);
            
            mMapManager.LoadMap("Maps/" + mapname);
            Texture2D wallTexture = mMapManager.GetTileSheet("WallTextures");
            mTilemap.WallTextures = new SpriteSheet(wallTexture, 64, 64);
            mTilemap.SpriteTextures = new SpriteSheet(mMapManager.GetTileSheet("WolfItems"), 64, 64);
            
        }

        static void mMapManager_CreateEnemies(object sender, SpawnEntityEventArgs e)
        {
            mTilemap.EnemySpawnPoints.Add(e.Position.ToCellCenteredVector2(), e.TileIndex);
        }

        static void mMapManager_CreateItems(object sender, SpawnEntityEventArgs e)
        {
            
            mTilemap.ItemSpawnPoints.Add(e.Position.ToCellCenteredVector2(), e.TileIndex);
            if (e.Properties.ContainsKey("MessageText"))
            {
                MessageTriggerInfo messageTriggerInfo = new MessageTriggerInfo();
                string message = e.Properties["MessageText"].ToString().Replace("\\n", "\n");
                bool centered = e.Properties["Centered"] == "true" ? true : false;
                messageTriggerInfo.IsCentered = centered;
                messageTriggerInfo.Text = message;
                mTilemap.CreateMessageTrigger(e.Position, messageTriggerInfo);
            }
        }

        static void mMapManager_CreateMetaInfo(object sender, SpawnEntityEventArgs e)
        {
            switch (e.TileIndex)
            {
                case 0:
                    mTilemap.PlayerViewDirection = new Vector2(1,0);
                    mTilemap.PlayerSpawn = e.Position.ToCellCenteredVector2();
                    break;
                case 1:
                    mTilemap.PlayerViewDirection = new Vector2(0,-1);
                    mTilemap.PlayerSpawn = e.Position.ToCellCenteredVector2();
                    break;
                case 2:
                    mTilemap.PlayerViewDirection = new Vector2(0,1);
                    mTilemap.PlayerSpawn = e.Position.ToCellCenteredVector2();
                    break;
                case 3:
                    mTilemap.PlayerViewDirection = new Vector2(-1,0);
                    mTilemap.PlayerSpawn = e.Position.ToCellCenteredVector2();
                    break;
                case 4:
                    mTilemap.CreatePatrolTurnPoint(e.Position, '>');
                    break;
                case 5:
                    mTilemap.CreatePatrolTurnPoint(e.Position, '^');
                    break;
                case 6:
                    mTilemap.CreatePatrolTurnPoint(e.Position, ',');
                    break;
                case 7:
                    mTilemap.CreatePatrolTurnPoint(e.Position, '<');
                    break;
                case 8:
                    MessageTriggerInfo messageTriggerInfo = new MessageTriggerInfo();
                    string message = e.Properties["MessageText"].ToString().Replace("\\n","\n");
                    bool centered = e.Properties["Centered"] == "true" ? true : false;
                    messageTriggerInfo.IsCentered = centered;
                    messageTriggerInfo.Text = message;
                    mTilemap.CreateMessageTrigger(e.Position, messageTriggerInfo);
                    break;
                    
            }
            
        }

        static void mMapManager_CreateWalls(object sender, SpawnEntityEventArgs e)
        {
            switch (e.TileIndex)
            {
                case 4:
                case 16:
                case 18:
                    mTilemap.CreateDoor(e.Position,e.TileIndex);
                    break;
                case 9:
                    mTilemap.CreateSecretWall(e.Position, e.TileIndex);
                    break;
                default:
                    mTilemap.CreateWall(e.Position, e.TileIndex);
                    break;
            }
            if (e.Properties.Count > 0 && e.Properties.ContainsKey("NeedsKey") && e.Properties["NeedsKey"] == "Gold")
                mTilemap.SetKeyNeed(e.Position, "Gold");
        }

        void PlayerMovedIntoTile(object sender, EventArgs e)
        {
            if (mTilemap.GetCellMetaDataByPosition(mPlayerTransform.LastCellPosition) == 's')
            {
                mTilemap.DestroyWall(mPlayerTransform.LastCellPosition);
            }

            if (mZombieTriggerArea.Contains(mPlayerTransform.LastCellPosition))
            {
                if (mZombieSpawnTimer == null)
                    mZombieSpawnTimer = new CallbackTimer(SpawnInZombieArea,3,true);
            }
            else // not in trigger area..
            {
                if (mZombieSpawnTimer != null)
                {
                    mZombieSpawnTimer.Stop();
                    mZombieSpawnTimer = null;
                }

            }
            
            /*
            if (mTilemap.MessageTriggers.ContainsKey(mPlayerTransform.LastCellPosition))
            {
                ShowMessageStory(mTilemap.MessageTriggers[mPlayerTransform.LastCellPosition]);
                mTilemap.MessageTriggers.Remove(mPlayerTransform.LastCellPosition);
            }
             */
        }


        public static void ShowYouDiedStory(EventHandler<EventArgs> callback)
        {
            List<StoryTellingEvent> storyEvents = new List<StoryTellingEvent>();
            Color fadeColor = Color.Red;
            fadeColor.A = 180;
            fadeColor.R = 100;
            FadeToColorEvent fadeToRed = new FadeToColorEvent(fadeColor, 2) {DeferredEnd = true, IsNonBlocking = true };
            storyEvents.Add(fadeToRed);
            storyEvents.Add(new ChangeEyeHeightEvent(mRaycaster, 1, -128));
            storyEvents.Add(new ShowTextEvent("You died.\n*Right Mouse Button* to continue.",0.5f,0.5f,true));
            storyEvents.Add(new CallBackEvent(callback) { IsMandatory = true });
            mGameStateManager.Push(new StoryTellingState(mGameStateManager, storyEvents));
        }

        public void ShowYouWonTheGameStory()
        {
            List<StoryTellingEvent> storyEvents = new List<StoryTellingEvent>();
            Color nearBlack = Color.Black;
            nearBlack.A = 200;
            FadeToColorEvent fadeToBlack = new FadeToColorEvent(nearBlack, 5) { DeferredEnd = true };
            storyEvents.Add(fadeToBlack);
            storyEvents.Add(new ShowTextEvent("You did it. You played through this dream of mine.\n\nThanks. And keep on rolling..", 0.8f, 0.5f, false));
            storyEvents.Add(new ShowTextEvent("Credits\nProgramming: Felix Ruzzoli", 0.8f, 0.5f, true));
            mGameStateManager.Push(new StoryTellingState(mGameStateManager, storyEvents));
        }

        public void ShowMessageStory(MessageTriggerInfo messageTriggerInfo)
        {
            List<StoryTellingEvent> storyEvents = new List<StoryTellingEvent>();
            Color nearBlack = Color.Black;
            nearBlack.A = 200;
            FadeToColorEvent fadeToBlack = new FadeToColorEvent(nearBlack, 5) { DeferredEnd = true };
            storyEvents.Add(fadeToBlack);
            storyEvents.Add(new ShowTextEvent(messageTriggerInfo.Text, 0.8f, 0.5f, messageTriggerInfo.IsCentered));
            storyEvents.Add(new CallBackEvent((o, args) => fadeToBlack.End()));
            storyEvents.Add(new FadeToColorEvent(nearBlack, new Color(0, 0, 0, 0), 5));
            mGameStateManager.Push(new StoryTellingState(mGameStateManager, storyEvents));
        }

        public static void ShowLoadingScreen()
        {
            mLoadingScreen = new LabelWidget();
            List<Texture2D> loadingScreens = new List<Texture2D>(){AssetManager.Default.LoadTexture("Menu/loading_id_widescreen.png"),AssetManager.Default.LoadTexture("Menu/loading_wolf_widescreen.png"),AssetManager.Default.LoadTexture("Menu/loading_bs_widescreen.png")};
            mLoadingScreen.Background = loadingScreens[TwenMath.Random.Next(0,loadingScreens.Count)];
            mLoadingScreen.Bounds = new UniRectangle(0, 0, GameGui.Viewport.Width, GameGui.Viewport.Height);
            mLoadingScreen.DrawLabelBackground = true;
            GameGui.RootWidget.AddChild(mLoadingScreen);
        }

        public static void DestroyLoadingScreen()
        {
            mLoadingScreen.Destroy();
        }

        public static void InitialSpawnPlayer()
        {
            mMultiKillTimer = new CallbackTimer(ResetMultiKills, 2, true);
            mTimers.Add(mMultiKillTimer);
            CreatePlayer(mTilemap.PlayerSpawn, mTilemap.PlayerViewDirection);
            mMapRenderer.Player = Player;
            InitialCreatePlayerWeapon();
        }


        protected override void LoadGame()
        {
            mHudFont = Content.Load<SpriteFont>("Fonts/WolfFont");
            mMessageFont = Content.Load<SpriteFont>("Fonts/WolfFontSmall");
            mLongTextFont = mMessageFont;
            // TODO: FIX // mLongTextFont = Content.Load<SpriteFont>("BitmapFonts/ConsoleFontTexture");
            mLongTextFont.Spacing = 2;
            sGui.LoadContent(Content, GraphicsDevice, mHudFont, mLongTextFont);

            AudioManager.LoadSound("Music/Wolfenstein/wolfmenu.mp3","MenuMusic",true);
            AudioManager.LoadSound("Music/Wolfenstein/wolfintro.mp3", "IntroMusic", true);
            
            AudioManager.LoadSound("Music/Wolfenstein/wolfplay01.mp3", "GamePlayMusic01", true);
            AudioManager.LoadSound("Music/Wolfenstein/wolfplay02.mp3", "GamePlayMusic02", true);
            AudioManager.LoadSound("Music/Wolfenstein/wolfplay03.mp3", "GamePlayMusic03", true);

            // TODO: FIX
            /*
            AudioManager.LoadSound("SoundEffects/Wolfenstein/gunshot1.wav", "Gunshot01", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/gunshot2.wav", "Gunshot02", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/gunshot3.wav", "Gunshot03", false);

            AudioManager.LoadSound("SoundEffects/Rott/pistol1.wav", "Pistol1", false);
            AudioManager.LoadSound("SoundEffects/Rott/pistol2.wav", "Pistol2", false);

            AudioManager.LoadSound("SoundEffects/Rott/bulletRicochet1.wav", "Ricochet1", false);
            AudioManager.LoadSound("SoundEffects/Rott/bulletRicochet2.wav", "Ricochet2", false);
            AudioManager.LoadSound("SoundEffects/Rott/bulletRicochet3.wav", "Ricochet3", false);

            AudioManager.LoadSound("SoundEffects/Wolfenstein/achtung.wav", "Achtung", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/alarm.wav", "Alarm", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/werda.wav", "WerDa", false);

            AudioManager.LoadSound("SoundEffects/Wolfenstein/opendoor.wav", "CloseDoor", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/closedoor.wav", "OpenDoor", false);

            AudioManager.LoadSound("SoundEffects/Wolfenstein/meinLeben.wav", "MeinLeben", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/alarmSound.wav", "AlarmSound", false);
            
            AudioManager.LoadSound("SoundEffects/Wolfenstein/rifleSound2.wav", "Rifle1", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/rifleSound3.wav", "Rifle2", false);

            AudioManager.LoadSound("SoundEffects/Wolfenstein/Machine Gun.wav", "MachineGun", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/Gatling Gun.wav", "GatlingGun", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/Knife.wav", "Knife", false);

            AudioManager.LoadSound("SoundEffects/Wolfenstein/Enemy Pain.wav", "EnemyPain", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/Player Dies.wav", "PlayerDies", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/Player Pain 1.wav", "PlayerPain1", false);
            AudioManager.LoadSound("SoundEffects/Wolfenstein/Player Pain 2.wav", "PlayerPain2", false);
            

            AudioManager.LoadSound("SoundEffects/Narrator/doublekill.wav", "DoubleKill", false);
            AudioManager.LoadSound("SoundEffects/Narrator/triplekill.wav", "TripleKill", false);
            AudioManager.LoadSound("SoundEffects/Narrator/multikill.wav", "MultiKill", false);
            AudioManager.LoadSound("SoundEffects/Narrator/ultrakill.wav", "UltraKill", false);
            AudioManager.LoadSound("SoundEffects/Narrator/monsterkill.wav", "MonsterKill", false);

            */
        }

        public static void CreatePlayer(Vector2 pos, Vector2 playerViewDirection)
        {
            Debug.Print("Created Player Entity");
            Player = sWorld.CreateEntity();
            Player.Group = "Player";
            Entity localPlayer = sWorld.TagManager.GetEntity("LocalPlayer");
            if (localPlayer != null)
            {
                Debug.Print("Strange..!");
            }
            Player.Tag = "LocalPlayer";
            Player.AddComponent(new Collider(0.2f) { CollisionGroup = 0});
            Player.AddComponent(new FPSControl() { MoveBackward = Keys.S, MoveForward = Keys.W, MoveLeft = Keys.A, MoveRight = Keys.D, ToggleMap = Keys.M, MapZoomIn = Keys.OemPlus, MapZoomOut = Keys.OemMinus });
            mPlayerTransform = new Transform(pos, TwenMath.DirectionVectorToRotation(playerViewDirection)) { CollideWithMap = true,CollideWithEntityMap = true, MaxSpeed = 4 };
            mPlayerTransform.BeginInvisibility += new EventHandler<EventArgs>(BeginInvisibility);
            mPlayerTransform.EndInvisibility += new EventHandler<EventArgs>(EndInvisibility);
            Player.AddComponent(mPlayerTransform);

            PlayerHealthPoints = new HealthPoints(100);
            //PlayerHealthPoints.ReceivedDamage += PlayerReceivedDamage; // TODO: enable for damage
            PlayerHealthPoints.Dying += PlayerDied;
            PlayerHealthPoints.BeginImmortality += BeginImmortality;
            PlayerHealthPoints.EndImmortality += EndImmortality;
            Player.AddComponent(PlayerHealthPoints);
            Player.AddComponent(new Inventory(10));
            Player.Refresh();// always call Refresh() when adding/removing components!
            
            mTilemap.AddEntity(Player);
        }


        private static void InitialCreatePlayerWeapon()
        {
            Entity hudFace = EntitySpawn.CreateHudFace(new Point(sScreenWidth, sScreenHeight), sRaycastRenderSystem.StatusbarHeight,
                                                       (o, args) => AnimateFaceIdleAnimation());
            mHudFaceAnimator = hudFace.GetComponent<SpriteAnimator>();

            //GiveWeaponToPlayer(1, EntitySpawn.CreateWolfKnife);
            //GiveWeaponToPlayer(1, EntitySpawn.CreateDoomPunch);
            GiveAllWeponsToPlayer();
            Debug.Print("Spawned 2 Entities (HudFace & StartWeapon)");

            /*
            GiveWeaponToPlayer(6, EntitySpawn.CreateWolfRifle);
            GiveWeaponToPlayer(7, EntitySpawn.CreateWolfGatling);
            GiveWeaponToPlayer(9, EntitySpawn.CreateMagicHand);
            */
            ChangePlayerWeapon(1);
        }

        public static void GiveAllWeponsToPlayer()
        {
            List<Func<Point, int, Entity>> weaponFunctions = new List<Func<Point, int, Entity>>() { null, EntitySpawn.CreateWolfKnife, EntitySpawn.CreateRottPistol, EntitySpawn.CreateWolfRifle, EntitySpawn.CreateWolfGatling, EntitySpawn.CreateBlakeStoneAutoChargePistol, EntitySpawn.CreateMagicHand, EntitySpawn.CreateDoomPunch, EntitySpawn.CreateDoomShotgun, EntitySpawn.CreateDukeShotgun };
            Inventory inventory = Player.GetComponent<Inventory>();
            int index = 0;
            foreach (Func<Point, int, Entity> weaponFunction in weaponFunctions)
            {
                if (weaponFunction != null && inventory.Items[index] == null)
                {
                    GiveWeaponToPlayer(index, weaponFunction);
                }
                index++;
            }
            PlayerAmmo = 99;
        }

        public static void GiveWeaponToPlayer(int inventorySlotIndex, Func<Point, int, Entity> creationFunction)
        {
            Inventory inventory = Player.GetComponent<Inventory>();
            if (inventory.Items[inventorySlotIndex] == null)
            {
                Debug.Print("Created HUD Weapon Entity..");
                inventory.Items[inventorySlotIndex] = creationFunction(new Point(sScreenWidth, sScreenHeight),
                                                                       sRaycastRenderSystem.StatusbarHeight);
                inventory.Items[inventorySlotIndex].IsEnabled = false;
            }
            PlayerAmmo += inventory.Items[inventorySlotIndex].GetComponent<Weapon>().CurrentAmmo;
            AnimateEvilGrinOnHudFace();
            ChangePlayerWeapon(inventorySlotIndex);
        }


        public static void GiveAmmoToPlayer(int amount)
        {
            PlayerAmmo += amount;
        }

        protected override void PostInit()
        {
            if (mTilemap == null) return;
            /*
            mCollisionSystem.SetWorldSize(mTilemap.MapWidth, mTilemap.MapHeight, 1);
            mCollisionSystem.RegisterCollisionTestNew(0, 4);    // player vs. pickups
            mCollisionSystem.RegisterCollisionTestNew(5, 3);    // projectiles vs. enemys
            */
            mGameStateManager.Push(new MainMenuState(sWorld,mSystemManager,mGameStateManager,mFpsControlSystem,mSpriteBatch));
        }

        #endregion

        #region Game Events

        private void PlayerUsed(object sender, EventArgs e)
        {

            if (CheckForUsableEntity())
                return;
            if (CheckForUsableDoor())
                return;
            if (CheckForUsableWall())
                return;
        }

        private bool CheckForUsableWall()
        {
            if (TwenMath.DistanceToTile(mPlayerTransform.Position, mRaycaster.TargetedWall) <= 2) 
            {
                if (mTilemap.GetCellDataByPosition(mRaycaster.TargetedWall) == 6)
                {
                    FinishedMap();
                    return true;
                }
                else if (mTilemap.GetCellMetaDataByPosition(mRaycaster.TargetedWall) == 'g') // gold key needed
                {
                    if (PlayerKeys["Gold"])
                    {
                        mTilemap.DestroyWall(mRaycaster.TargetedWall);
                    }
                    else
                    {
                        ShowMessageStory(new MessageTriggerInfo(){IsCentered = true, Text = "Gold Key needed."});
                    }
                }
            }
            return false;
        }

        private bool CheckForUsableDoor()
        {
            if (mRaycaster.TargetedDoor != Point.Zero && mTilemap.GetCellMetaDataByPosition(mRaycaster.TargetedDoor) == 'd')
            {
                if (TwenMath.DistanceToTile(mPlayerTransform.Position, mRaycaster.TargetedDoor) <= 2)
                {
                    OpenDoor(mRaycaster.TargetedDoor);
                    return true;
                }
            }
            return false;
        }

        private bool CheckForUsableEntity()
        {
            List<Entity> targetedEntities = mRaycaster.TargetedEntities[mRaycaster.ScreenWidth/2];
            if (targetedEntities.Count > 0)
            {
                Transform transform = GetFirstUsableEntityTransform(targetedEntities);
                if (transform != null)
                {
                    if (Vector2.Distance(mPlayerTransform.Position, transform.Position) <= 2)
                    {
                        Point cell = new Point((int) transform.Position.X, (int) transform.Position.Y);
                        ShowMessageStory(mTilemap.MessageTriggers[cell]);
                        return true;
                    }
                }
            }
            return false;
        }

        private Transform GetFirstUsableEntityTransform(List<Entity> targetedEntities)
        {
            for (int index = targetedEntities.Count - 1; index >= 0 ; index--)
            {
                Entity targetedEntity = targetedEntities[index];
                Transform transform = targetedEntity.GetComponent<Transform>();
                if (transform == null) continue;
                if (mTilemap.MessageTriggers.ContainsKey(new Point((int) transform.Position.X,
                                                                   (int) transform.Position.Y)))
                {
                    return transform;
                }
            }
            return null;
        }

        private void OpenDoor(Point targetedDoor)
        {
            List<Entity> entities = mTilemap.Entities[targetedDoor.Y, targetedDoor.X];
            if (entities.Count > 0)
            {
                foreach (Entity entityAtPosition in entities)
                {
                    if (entityAtPosition.Group == "Door")
                    {
                        Door door = entityAtPosition.GetComponent<Door>();
                        if (!door.IsOpening)
                            door.StartOpenDoor();
                    }
                }
            }
        }

        private void PlayerPressedFire(object sender, EventArgs e)
        {
            if (CurrentWeapon.NeedsAmmo)
            {
                if (PlayerAmmo > 0)
                {
                    CurrentWeapon.Fire();
                }
            }
            else
            {
                CurrentWeapon.Fire();
            }
        }

        public void AlertNonHiddenEnemiesNearPosition(Vector2 position)
        {
            HashSet<Entity> entitiesInRange = mTilemap.GetEntitiesInRange(position, 10);
            foreach (Entity entity in entitiesInRange)
            {
                if (entity.Group == "Enemy")
                {
                    MetaBehavior metaBehavior = entity.GetComponent<MetaBehavior>();
                    if (metaBehavior != null)
                    {
                        ActorStateMachine actorStateMachine = metaBehavior.GetBehavior<ActorStateMachine>();
                        if (actorStateMachine.CurrentState == "Stand" || actorStateMachine.CurrentState == "Path")
                            actorStateMachine.ActorAlert();
                    }
                }
            }
        }

        private static void PlayerReceivedDamage(object sender, DamageEventArgs e)
        {
            float factor = (e.Damage);

            sRaycastRenderSystem.FlashScreen(Color.Red, factor);

            if (e.DamageSource != null)
            {
                Vector2 targetDir = e.DamageSource.GetComponent<Transform>().Position - mPlayerTransform.Position;

                RelativeDirection dir = TwenMath.GetDirection(mPlayerTransform.Forward, targetDir);

                AnimateFaceLookingToEnemy(dir);
            }
            else
            {
                AnimatePainOnHudFace(e.Damage);
            }
            AudioManager.PlayRandomSound(new List<string>(){"PlayerPain1","PlayerPain2"});
            //mHudFaceAnimator.FinishedPlaying += new EventHandler<EventArgs>(mHudFaceAnimator_FinishedPlaying);
            
        }

        private static void PlayerDied(object sender, EventArgs e)
        {
            AudioManager.PlaySound("PlayerDies");
            ShowYouDiedStory(Respawn);
        }

        private static void Respawn(object sender, EventArgs eventArgs)
        {
            ShowLoadingScreen();

            SpawnPlayer(mTilemap.PlayerSpawn, mTilemap.PlayerViewDirection);
            PlayerHealthPoints.RestoreToFull();
            AnimateFaceIdleAnimation();
            mRaycaster.Camera.EyeHeight = 0;
            ChangeMapTo(mCurrentMapIndex);
            mZombieSpawnCounter = 0;
            
            /*
            SpawnPlayer(mTilemap.PlayerSpawn, mTilemap.PlayerViewDirection);
            PlayerHealthPoints.RestoreToFull();
            AnimateFaceIdleAnimation();
            mRaycaster.Camera.EyeHeight = 0;
             */
        }

        private void FinishedMap()
        {
            if (mCurrentMapIndex == mMapList.GetLength(0) - 1)
            {
                FinishedGame();
                return;
            }
            mCurrentMapIndex++;
            ChangeMapTo(mCurrentMapIndex);
        }

        private void FinishedGame()
        {
            mGameStateManager.Pop();
            ShowYouWonTheGameStory();
            //EntitySpawn.CreateMessage("You Won!");
        }

        void PlayerFoundSecret(object sender, PositionedEventArgs e)
        {
            EntitySpawn.CreateMessage("You found a secret area!",5,true);
        }

        static void EndInvisibility(object sender, EventArgs e)
        {
            EntitySpawn.CreateMessage("Visibility turned ON");
        }

        static void BeginInvisibility(object sender, EventArgs e)
        {
            EntitySpawn.CreateMessage("Visibility turned OFF");
        }

        private static void EndImmortality(object sender, EventArgs e)
        {
            EntitySpawn.CreateMessage("DEGREELESSNESS MODE OFF");
            AnimateFaceIdleAnimation();
        }

        private static void BeginImmortality(object sender, EventArgs e)
        {
            EntitySpawn.CreateMessage("DEGREELESSNESS MODE ON");
            AnimateFaceIdleAnimation();
        }

        private void AllWeaponsCheatActivated()
        {
            EntitySpawn.CreateMessage("impulse 9 activated..");
            GiveAllWeponsToPlayer();
        }


        public static void PlayerKilledEnemy(Entity killedEntity)
        {
            MultiKillCounter++;
            mMultiKillTimer.Reset();
        }

        protected static decimal MultiKillCounter { get; set; }

        public static void OnEntityCollidedWithHazard(object o, CollisionEventArgs args)
        {
            HealthPoints healthPoints = args.CollidingEntity.GetComponent<HealthPoints>();
            if (healthPoints != null)
                healthPoints.DealDamage(1);
        }

        #endregion

        #region Game Mechanic Actions

        private static void SpawnPlayer(Vector2 playerSpawn, Vector2 playerViewDirection)
        {
            mPlayerTransform.Position = playerSpawn;
            mPlayerTransform.Rotation = TwenMath.DirectionVectorToRotation(playerViewDirection);
            mRaycaster.SyncRaycasterCamToPlayer(mPlayerTransform);
        }

        private static CallbackTimer mMultiKillTimer;
        private static int mZombieSpawnCounter;

        private static void ResetMultiKills()
        {
            if (MultiKillCounter >= 6)
            {
                AudioManager.PlaySound("MonsterKill");
            }
            else if (MultiKillCounter == 5)
            {
                AudioManager.PlaySound("UltraKill");
            }
            else if (MultiKillCounter == 4)
            {
                AudioManager.PlaySound("MultiKill");
            }
            else if (MultiKillCounter == 3)
            {
                AudioManager.PlaySound("TripleKill");
            }
            else if (MultiKillCounter == 2)
            {
                AudioManager.PlaySound("DoubleKill");
            }
            MultiKillCounter = 0;
        }


        public static void HealPlayer(int amount)
        {
            PlayerHealthPoints.Heal(amount);
            AnimateFaceIdleAnimation();
        }

        public static void DealDamageToPlayer(int amount)
        {
            PlayerHealthPoints.DealDamage(amount);
        }

        public static void ChangePlayerWeapon(int weaponIndex)
        {
            Inventory inventory = Player.GetComponent<Inventory>();
            if (inventory.Items[weaponIndex] == null) return;
            if (inventory.SelectedItem > 0)
            {
                Entity oldWeapon = inventory.GetSelectedItem();
                oldWeapon.IsEnabled = false;
                oldWeapon.Refresh();
            }
            inventory.SelectedItem = weaponIndex;
            Entity newWeapon = inventory.GetSelectedItem();

            newWeapon.IsEnabled = true;
            newWeapon.Refresh();
            CurrentWeapon = newWeapon.GetComponent<Weapon>();
            
        }

        private static void ChangeMapTo(int mapIndex)
        {
            FinishedLoading = false;

            KillAllEntitiesOnMapExceptPlayers();
            
            ResetKeys();

            LoadMap(mMapList[mapIndex]);
            
            TicEvent += ChangeMap;  // wait one tick for entities to vanish out of the subsystems.. then change map..
        }

        private static void ResetKeys()
        {
            PlayerKeys = new Dictionary<string, bool>();
            PlayerKeys["Silver"] = false;
            PlayerKeys["Gold"] = false;
        }

        public void SpawnInZombieArea()
        {
            int x = TwenMath.Random.Next(mZombieSpawnArea.Left, mZombieSpawnArea.Right);
            int y = TwenMath.Random.Next(mZombieSpawnArea.Top, mZombieSpawnArea.Bottom);

            while (mTilemap.Entities[y,x].Count > 0)
            {
                x = TwenMath.Random.Next(mZombieSpawnArea.Left, mZombieSpawnArea.Right);
                y = TwenMath.Random.Next(mZombieSpawnArea.Top, mZombieSpawnArea.Bottom);
            }
            
            if (mZombieSpawnCounter != 10)
                EntitySpawn.CreateZombie(new Vector2(x + 0.5f, y + 0.5f), null);
            else
                EntitySpawn.CreateZombie(new Vector2(x + 0.5f, y + 0.5f), pos => EntitySpawn.CreateKeyPickup(pos, 9, "Gold"));
            mZombieSpawnCounter++;
            
            Debug.Print("Spawned Zombie #" + mZombieSpawnCounter);
        }

        #endregion

        private static void KillAllEntitiesOnMapExceptPlayers()
        {
            int killCounter = 0;
            List<Entity> allEntities = new List<Entity>(mTilemap.AllEntities);
            mTilemap.RemoveAllEntities();
            mRaycaster.TargetedWall = Point.Zero;
           
            foreach (Entity entity in allEntities)
            {
                if (entity.Group == "Player") continue;
                entity.Delete();
                killCounter++;
            }
            Debug.Print("Killed " + killCounter + " entities..(Player still alive)");
        }
        public static void KillAllEntities()
        {
            int killCounter = 0;
            Bag<Entity> allEntities = sWorld.EntityManager.ActiveEntities;
            mTilemap.RemoveAllEntities();
            mRaycaster.TargetedWall = Point.Zero;
            foreach (Entity entity in allEntities)
            {
                if (entity == null) continue;
                entity.Delete();
                sWorld.EntityManager.Remove(entity); // Do we need this here?
                killCounter++;
                /* Temporarily Removed during migration to MonoGame
                if (entity.Tag != "")
                {
                    sWorld.TagManager.Unregister(entity.Tag);  // TODO: What to do here? Unregister is now internal
                }
                */
            }
            Debug.Print("Killed " + killCounter + " entities..(Player killed)");
        }


        public static void ResetTilemap()
        {
            LoadMap(mMapList[mCurrentMapIndex]);
        }

        static void ChangeMap(object sender, EventArgs e)
        {
            
            TicEvent -= ChangeMap;
            mRaycaster.ChangeMap(mTilemap);
            mMapRenderer.ChangeMap(mTilemap);
            mMapRenderer.DrawMap = false;
            sRaycastRenderSystem.ChangeMap(mTilemap);
            mTilemapCollisionSystem.ChangeMap(mTilemap);
            mTilemapEntityCollisionSystem.ChangeMap(mTilemap);
            EntitySpawn.ChangeMap(mTilemap);
            SpawnThings();
            

            SpawnPlayer(mTilemap.PlayerSpawn, mTilemap.PlayerViewDirection);
            mTilemap.AddEntity(Player);

            DestroyLoadingScreen();
        }

        public static BaseWidget CreatePauseLabel()
        {
            LabelWidget curLabel = new LabelWidget("PAUSED!");
            float height = 90;
            float width = 200;
            curLabel.Bounds = new UniRectangle(new UniScalar(0.5f, -(width/2)), new UniScalar(0.1f, 0), width, height);
            curLabel.DrawLabelBackground = true;
            curLabel.DrawBackgroundShadow = true;
            curLabel.LabelColor = Color.DarkBlue;

            GameGui.RootWidget.AddChild(curLabel);
            return curLabel;
        }

        public static LabelWidget CreateTextWidget(string text, float fractionalWidth, float fractionalHeight)
        {
            LabelWidget textWidget = new LabelWidget(text);
            float height = sScreenHeight * fractionalHeight;
            float width = sScreenWidth * fractionalWidth;
            float xborder = (1.0f - fractionalWidth) / 2;
            float yborder = (1.0f - fractionalHeight) / 4;
            textWidget.Bounds = new UniRectangle(new UniScalar(xborder, 0), new UniScalar(yborder, 0), width, height);
            textWidget.DrawLabelBackground = true;
            textWidget.DrawBackgroundShadow = false;
            textWidget.LabelColor = new Color(22, 22, 30, 240);
            textWidget.Font = GameFont.LongTexts;
            textWidget.BorderSize = 15;
            textWidget.IsSizeDependingOnContent = true;
            GameGui.RootWidget.AddChild(textWidget);
            return textWidget;
        }

        public static LabelWidget FullScreenOverlay()
        {
            LabelWidget overlay = new LabelWidget();
            float height = sScreenHeight;
            float width = sScreenWidth;
            overlay.Bounds = new UniRectangle(0,0, width, height);
            overlay.DrawLabelBackground = true;
            overlay.DrawBackgroundShadow = false;
            overlay.LabelColor = new Color(0,0,0,0);
            GameGui.RootWidget.AddChild(overlay);
            return overlay;
        }

        protected override void Shutdown()
        {
            //throw new NotImplementedException();
            SaveConfiguration();
            AudioManager.Dispose();
        }

        #region status bar face animation

        public static void AnimateEvilGrinOnHudFace()
        {
            if (PlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = "Grin20Percent";
            else if (PlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = "Grin40Percent";
            else if (PlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = "Grin60Percent";
            else if (PlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = "Grin80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = "GrinFullHealth";

            mHudFaceAnimator.ResetAndPlay();
        }

        public static void AnimatePainOnHudFace(int damage)
        {
            string prefix = "";
            if (damage >= 20)
                prefix = "Big";

            if (PlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain20Percent";
            else if (PlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain40Percent";
            else if (PlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain60Percent";
            else if (PlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = prefix + "Pain80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = prefix + "PainFullHealth";

            mHudFaceAnimator.ResetAndPlay();


        }

        public static void AnimateFaceIdleAnimation()
        {
            if (PlayerHealthPoints.IsImmortal)
                mHudFaceAnimator.CurrentAnimation = "IdleImmortal";
            else if (PlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = "Idle20Percent";
            else if (PlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = "Idle40Percent";
            else if (PlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = "Idle60Percent";
            else if (PlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = "Idle80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = "IdleFullHealth";

            mHudFaceAnimator.ResetAndPlay();
        }

        private static void AnimateFaceLookingToEnemy(RelativeDirection dir)
        {
            if (PlayerHealthPoints.Health < 20)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "20Percent";
            else if (PlayerHealthPoints.Health < 40)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "40Percent";
            else if (PlayerHealthPoints.Health < 60)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "60Percent";
            else if (PlayerHealthPoints.Health < 80)
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "80Percent";
            else
                mHudFaceAnimator.CurrentAnimation = "Look" + dir + "FullHealth";

            mHudFaceAnimator.ResetAndPlay();
        }

        #endregion

        protected override void Update(GameTime gameTime)
        {

            //DebugDrawer.DrawString("GameState: " + mGameStateManager.ActiveState.ToString());
            if (!IsActive) return;
            if (mZombieSpawnTimer != null)
                mZombieSpawnTimer.Update(gameTime.ElapsedGameTime.TotalSeconds);
            for (int index = mTimers.Count-1; index >= 0; index--)
            {
                CallbackTimer callbackTimer = mTimers[index];
                callbackTimer.Update(gameTime.ElapsedGameTime.TotalSeconds);
                if (callbackTimer.IsDone())
                    mTimers.Remove(callbackTimer);
            }
            /*
            foreach (Entity targetedEntity in mRaycaster.TargetedEntities)
            {
                MetaBehavior metaBehavior = targetedEntity.GetComponent<MetaBehavior>();
                if (metaBehavior != null)
                {
                    ActorStateMachine actorStateMachine = metaBehavior.GetBehavior<ActorStateMachine>();
                    if (actorStateMachine != null)
                    {
                        DebugDrawer.DrawString(targetedEntity.Group + ": " + actorStateMachine.CurrentState);
                    }
                }
                SpriteAnimator spriteAnimator = targetedEntity.GetComponent<SpriteAnimator>();
                if (spriteAnimator != null)
                    DebugDrawer.DrawString(targetedEntity.Group + ": " + spriteAnimator.CurrentAnimation);
            }
            */
            mTilemap.UpdateEntities();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if ((mGameStateManager.ActiveState.GetType() == typeof(GamePlayState) || mGameStateManager.ActiveState.GetType() == typeof(StoryTellingState) || mGameStateManager.ActiveState.GetType() == typeof(EscapeMenuState)) && FinishedLoading)
            {
                sWorld.Draw();
                mSpriteBatch.Begin();
                DrawStatusbar();
                mSpriteBatch.End();
            }

            sGui.Draw(mSpriteBatch);

            DebugDrawer.Draw();

            base.Draw(gameTime);
        }
        private void DrawStatusbar()
        {
            if (PlayerHealthPoints != null)
            {
                string hpString = "HP: " + PlayerHealthPoints.Health;
                int offsetLeft = 120;
                Vector2 hpStringSize = mHudFont.MeasureString(hpString);
                Vector2 hpPos = new Vector2(offsetLeft, sScreenHeight);
                Vector2 shadowOffset = new Vector2(-1, -2);
                mSpriteBatch.DrawString(mHudFont, hpString, hpPos + shadowOffset, Color.Black, 0f,
                                        new Vector2(0, hpStringSize.Y), 1f, SpriteEffects.None, 0.5f);
                Color hpColor = PlayerHealthPoints.Health < 20 ? Color.Red : Color.White;
                mSpriteBatch.DrawString(mHudFont, hpString, hpPos, hpColor, 0f, new Vector2(0, hpStringSize.Y), 1f,
                                        SpriteEffects.None, 0.5f);
                if (CurrentWeapon != null)
                {
                    if (CurrentWeapon.NeedsAmmo)
                    {
                        string ammoString = "Ammo: " + PlayerAmmo;

                        Vector2 ammoStringSize = mHudFont.MeasureString(ammoString);
                        Vector2 ammoPos = new Vector2(offsetLeft + hpStringSize.X + 50, sScreenHeight);
                        mSpriteBatch.DrawString(mHudFont, ammoString, ammoPos + shadowOffset, Color.Black, 0f,
                                                new Vector2(0, ammoStringSize.Y), 1f, SpriteEffects.None, 0.5f);
                        Color ammoColor = PlayerAmmo == 0 ? Color.Red : Color.White;
                        mSpriteBatch.DrawString(mHudFont, ammoString, ammoPos, ammoColor, 0f, new Vector2(0, ammoStringSize.Y),
                                                1f, SpriteEffects.None, 0.5f);
                    }
                    /*
                    string weaponNameString = CurrentWeapon.Name;
                    Vector2 weaponNameStringSize = mHudFont.MeasureString(weaponNameString);
                    Vector2 weaponNamePos = new Vector2(sScreenWidth - weaponNameStringSize.X - 20, sScreenHeight);
                    mSpriteBatch.DrawString(mHudFont, weaponNameString, weaponNamePos + shadowOffset, Color.Black, 0f,
                                            new Vector2(0, weaponNameStringSize.Y), 1f, SpriteEffects.None, 0.5f);
                    Color weaponNameColor = Color.White;
                    mSpriteBatch.DrawString(mHudFont, weaponNameString, weaponNamePos, weaponNameColor, 0f,
                                            new Vector2(0, weaponNameStringSize.Y),
                                            1f, SpriteEffects.None, 0.5f);
                    */
                }
            }

            
        }


    }
}
