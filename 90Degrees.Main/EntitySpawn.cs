using Artemis;
using Microsoft.Xna.Framework;
using raycaster.Scripts;
using raycaster.States;
using System;
using System.Collections.Generic;
using Twengine.Components;
using Twengine.Components.Meta;
using Twengine.Datastructures;
using Twengine.Managers;
using XNAHelper;

namespace raycaster
{
    public class PickupEventArgs : EventArgs
    {
        public int AmountPickedUp { get; set; }
    }
    public class EntitySpawn
    {
        private static EntityWorld sWorld;
        private static Tilemap sTilemap;

        public static void Init(EntityWorld world, Tilemap tilemap)
        {
            sWorld = world;
            sTilemap = tilemap;
        }

        public static void ChangeMap(Tilemap tilemap)
        {
            sTilemap = tilemap;
        }


        public static void CreateGuardDifferent(int actor, Vector2 spawnPos, Direction lookingDir, bool isActorPatrolling)
        {
            switch (actor)
            {
                case 0:
                    CreateGuard("Enemies/BlakeStone/bs_guard_sheet.png", spawnPos, TwenMath.DirectionToRotation(lookingDir), isActorPatrolling);
                    break;
                case 1:
                    CreateGuard("Enemies/Wolf/mguard_sheet.png", spawnPos, TwenMath.DirectionToRotation(lookingDir), isActorPatrolling);
                    break;
                case 2:
                    CreateDoomMarine(spawnPos, TwenMath.DirectionToRotation(lookingDir), isActorPatrolling);
                    break;
            }

        }
        #region Entity Definitions

        public static Entity CreateDoor(Point position, Orientation orientation, SpriteSheet sheet, int wallIndex)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = "Door";
            Vector2 spawnPos = position.ToCellCenteredVector2();

            e.AddComponent(new Transform(spawnPos, 0f));

            Collider collider = new Collider(0.5f) { CollisionShape = Shape.Rectangle };
            e.AddComponent(collider);
            Door doorComponent = new Door(spawnPos, orientation, 5);
            doorComponent.BeginOpen += delegate
                                           {
                                               RaycastGame.AudioManager.PlayEffect((int)SoundCue.OpenDoor);
                                           };
            doorComponent.BeginClose += delegate
                                            {
                                                RaycastGame.AudioManager.PlayEffect((int)SoundCue.CloseDoor);
                                            };
            e.AddComponent(doorComponent);

            e.AddComponent(new RaycastSprite(sheet, wallIndex) { Orientation = orientation });
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateDoomMarine(Vector2 position, float rotation, bool isActorPatrolling)
        {
            SpriteSheet guard = AssetManager.Default.LoadSpriteSheet("Enemies/Doom/doommarine_sheet.png", 128, 128);
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            //Collider collider = new Collider(0.5f);
            //e.AddComponent(collider);
            e.AddComponent(new Transform(position, rotation) { CollideWithMap = true });
            e.AddComponent(new RaycastSprite(guard, 0));
            e.AddComponent(new Physics(1f) { Mass = 0.8f });
            Enemy enemy = new Enemy() { FiringRange = 5, HearingRange = 10, VisibleRange = 7, FieldOfView = 120 };

            e.AddComponent(enemy);
            HealthPoints healthPoints = new HealthPoints(20);
            e.AddComponent(healthPoints);
            /*
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true, true);
            spriteAnimator.AddAnimation("Walk", new List<int>() { 1, 2, 3, 4 }, 6, true, true);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 40, 41, 42, 43, 44, 45, 46 }, 6, false, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 55 }, 6, false, false);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 53 }, 8, false, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 54 }, 8, false, false);
            spriteAnimator.Paused = false;
            e.AddComponent(spriteAnimator);
            */
            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            ActorStateMachine actorStateMachine = new ActorStateMachine(sTilemap);
            healthPoints.Dying += actorStateMachine.ActorDying;
            metaBehavior.AddBehavior(actorStateMachine);
            if (isActorPatrolling)
            {
                actorStateMachine.AddState(new Stand());
                actorStateMachine.AddStartState(new Path());
            }
            else
            {
                actorStateMachine.AddStartState(new Stand());
                actorStateMachine.AddState(new Path());
            }

            actorStateMachine.AddState(new Chase());
            actorStateMachine.AddState(new PrepareFire());
            actorStateMachine.AddState(new Fire());
            actorStateMachine.AddState(new Die());

            e.AddComponent(metaBehavior);

            e.Refresh();// always call Refresh() when adding/removing components!
            //collider.Collided += metaBehavior.OnCollision;
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateZeldaOldMan(Vector2 position)
        {
            SpriteSheet oldman = AssetManager.Default.LoadSpriteSheet("Specials/Zelda/oldman.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            e.AddComponent(new Collider(0.3f));
            e.AddComponent(new Transform(position, 0f) { CollideWithMap = true });
            e.AddComponent(new RaycastSprite(oldman, 0));
            e.AddComponent(new Physics(1f) { Mass = 0.8f });

            HealthPoints healthPoints = new HealthPoints(10);
            e.AddComponent(healthPoints);
            healthPoints.Dying += (o, args) => e.Delete();

            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }


        public static Entity CreateAnimatedSprite(Vector2 position, SpriteSheet sheet, List<int> frames, bool isObstructing)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = isObstructing ? "Obstacle" : "Deco";
            if (isObstructing)
            {
                e.AddComponent(new Collider(0.2f));
            }
            e.AddComponent(new Transform(position, 0f));
            e.AddComponent(new RaycastSprite(sheet, frames[0]));
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("Idle", frames, 5, true);
            spriteAnimator.CurrentAnimation = "Idle";
            spriteAnimator.ResetAndPlay();
            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateFireball(Vector2 position, Vector2 direction)
        {
            SpriteSheet fireballText = AssetManager.Default.LoadSpriteSheet("Weapons/Projectiles/fireball_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Projectile";
            e.AddComponent(new Transform(position, TwenMath.DirectionVectorToRotation(direction)) { CollideWithMap = true, CollideWithEntityMap = true });
            e.AddComponent(new RaycastSprite(fireballText, 0));
            Physics physics = new Physics(15) { IntendedMovementDirection = direction };
            e.AddComponent(physics);

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("Idle", new List<int>() { 0, 1 }, 5, true);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 2, 3, 4 }, 5, false);
            spriteAnimator.CurrentAnimation = "Idle";
            spriteAnimator.ResetAndPlay();
            spriteAnimator.FinishedPlaying += (sender, args) => e.Delete();
            Collider collider = new Collider(0.2f) { IsTrigger = true };
            e.AddComponent(collider);
            EventHandler<CollisionEventArgs> entityCollisionHandler = null;
            entityCollisionHandler = delegate (object sender, CollisionEventArgs e1)
                                         {
                                             physics.IntendedMovementDirection = Vector2.Zero;
                                             spriteAnimator.StartPlay("Hit");
                                             collider.CollidedWithEntity -= entityCollisionHandler;
                                             HealthPoints healthPoints = e1.CollidingEntity.GetComponent<HealthPoints>();
                                             if (healthPoints != null)
                                                 healthPoints.DealDamage(5);
                                         };
            collider.CollidedWithEntity += entityCollisionHandler;
            EventHandler<EventArgs> wallCollisionHandler = null;
            wallCollisionHandler = delegate
                                                          {
                                                              physics.IntendedMovementDirection = Vector2.Zero;
                                                              spriteAnimator.StartPlay("Hit");
                                                              collider.CollidedWithWall -= wallCollisionHandler;
                                                          };
            collider.CollidedWithWall += wallCollisionHandler;

            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateBulletHitAnimation(Vector2 position, bool isEnemyHit)
        {
            SpriteSheet bulletSheet = AssetManager.Default.LoadSpriteSheet("Weapons/Projectiles/bulletHits.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Deco";
            e.AddComponent(new Transform(position, 0) { CollideWithMap = false, CollideWithEntityMap = false });
            e.AddComponent(new RaycastSprite(bulletSheet, 0));

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("HitEnemy", new List<int>() { 0, 1, 2 }, 10, false);
            spriteAnimator.AddAnimation("HitWall", new List<int>() { 3, 4, 5, 6 }, 10, false);
            spriteAnimator.CurrentAnimation = isEnemyHit ? "HitEnemy" : "HitWall";
            spriteAnimator.ResetAndPlay();
            spriteAnimator.FinishedPlaying += (sender, args) => e.Delete();

            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateTwoStateAnimatedSprite(Vector2 position, SpriteSheet sheet, List<int> frames, List<int> alternateStateFrames, bool isObstructing)
        {

            Entity e = CreateAnimatedSprite(position, sheet, frames, isObstructing);

            Collider collider = new Collider(0.3f);
            e.AddComponent(collider);

            if (alternateStateFrames.Count > 0)
            {
                SpriteAnimator spriteAnimator = e.GetComponent<SpriteAnimator>();
                spriteAnimator.AddAnimation("AlternateState", alternateStateFrames, 6, true);
            }
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateAnimatedHazard(Vector2 position, SpriteSheet sheet, List<int> frames, List<int> alternateStateFrames, bool isObstructing)
        {

            Entity e = CreateAnimatedSprite(position, sheet, frames, isObstructing);
            e.Group = "Hazard";
            Collider collider = new Collider(0.5f);
            e.AddComponent(collider);
            collider.CollidedWithEntity += RaycastGame.OnEntityCollidedWithHazard;
            if (alternateStateFrames.Count > 0)
            {
                SpriteAnimator spriteAnimator = e.GetComponent<SpriteAnimator>();
                spriteAnimator.AddAnimation("AlternateState", alternateStateFrames, 6, true);
            }
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }


        public static Entity CreateScientist(Vector2 position, float rotation, bool isActorPatrolling)
        {
            SpriteSheet guard = AssetManager.Default.LoadSpriteSheet("Enemies/BlakeStone/scientist_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            Collider collider = new Collider(0.2f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(position, rotation) { CollideWithMap = true, CollideWithEntityMap = true });
            e.AddComponent(new RaycastSprite(guard, 0));
            e.AddComponent(new Physics(1f) { Mass = 0.8f });
            Enemy enemy = new Enemy() { FiringRange = 5, HearingRange = 10, VisibleRange = 7, FieldOfView = 120 };

            e.AddComponent(enemy);

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddDirectionalAnimationByFrames("Idle", 6, 0, 8, 1, true);
            spriteAnimator.AddDirectionalAnimationByFrames("Walk", 6, 8, 8, 4, true);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 40, 41, 42, 43, 44 }, 6, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 45 }, 6, false);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 46, 47 }, 8, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 48, 47, 46 }, 8, false);
            spriteAnimator.Paused = false;
            e.AddComponent(spriteAnimator);

            HealthPoints healthPoints = new HealthPoints(20);

            e.AddComponent(healthPoints);

            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            ActorStateMachine actorStateMachine = new ActorStateMachine(sTilemap);
            healthPoints.Dying += actorStateMachine.ActorDying;
            metaBehavior.AddBehavior(actorStateMachine);
            Stand stand = new Stand() { AttackOnSight = false };
            Path path = new Path() { AttackOnSight = false };
            if (isActorPatrolling)
            {

                actorStateMachine.AddState(stand);

                actorStateMachine.AddStartState(path);
            }
            else
            {
                actorStateMachine.AddStartState(stand);
                actorStateMachine.AddState(path);
            }

            actorStateMachine.AddState(new Chase());
            actorStateMachine.AddState(new PrepareFire());
            actorStateMachine.AddState(new Fire());
            actorStateMachine.AddState(new Die());
            actorStateMachine.AddState(new Pain());
            e.AddComponent(metaBehavior);

            healthPoints.ReceivedDamage += actorStateMachine.ActorHit;

            e.Refresh();// always call Refresh() when adding/removing components!
            //collider.Collided += metaBehavior.OnCollision;
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateTableGuy(Vector2 spawnPos)
        {
            SpriteSheet texture = AssetManager.Default.LoadSpriteSheet("Enemies/BlakeStone/tableguy_sheet.png", 64, 64);
            Entity e = CreateHiddenMonster(spawnPos, texture, true);

            SpriteAnimator spriteAnimator = e.GetComponent<SpriteAnimator>();

            spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 6, true);
            spriteAnimator.AddAnimation("BreakFree", new List<int>() { 1, 2, 3 }, 3, false);
            spriteAnimator.AddAnimation("Walk", new List<int>() { 5, 6, 7, 8 }, 6, true);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 9, 10 }, 8, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 11 }, 8, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 12 }, 6, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 13, 14, 15, 16, 17 }, 6, false);

            spriteAnimator.Paused = false;

            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateGlassGuy(Vector2 spawnPos)
        {
            SpriteSheet texture = AssetManager.Default.LoadSpriteSheet("Enemies/BlakeStone/glassguy_sheet.png", 64, 64);
            Entity e = CreateHiddenMonster(spawnPos, texture, true);

            SpriteAnimator spriteAnimator = e.GetComponent<SpriteAnimator>();

            spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 6, true);
            spriteAnimator.AddAnimation("BreakFree", new List<int>() { 1, 2, 3 }, 3, false);
            spriteAnimator.AddAnimation("Walk", new List<int>() { 5, 6, 7, 8 }, 6, true);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 9 }, 8, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 10, 11 }, 8, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 15 }, 6, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 16, 17, 18, 19, 20 }, 6, false);

            spriteAnimator.Paused = false;

            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateStatueGuy(Vector2 spawnPos)
        {
            SpriteSheet texture = AssetManager.Default.LoadSpriteSheet("Enemies/BlakeStone/statueguy_sheet.png", 64, 64);
            Entity e = CreateHiddenMonster(spawnPos, texture, false);

            SpriteAnimator spriteAnimator = e.GetComponent<SpriteAnimator>();

            spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 6, true);
            spriteAnimator.AddAnimation("BreakFree", new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 }, 3, false);
            spriteAnimator.AddAnimation("Walk", new List<int>() { 9, 10, 11, 12 }, 6, true);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 13, 14 }, 8, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 15 }, 8, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 19 }, 6, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 20, 21, 22, 23, 24 }, 6, false);

            spriteAnimator.Paused = false;

            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateEggGuy(Vector2 spawnPos)
        {
            SpriteSheet texture = AssetManager.Default.LoadSpriteSheet("Enemies/BlakeStone/eggguy_sheet.png", 64, 64);
            Entity e = CreateHiddenMonster(spawnPos, texture, false);

            SpriteAnimator spriteAnimator = e.GetComponent<SpriteAnimator>();

            spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 6, true);
            spriteAnimator.AddAnimation("BreakFree", new List<int>() { 1, 2, 3 }, 3, false);
            spriteAnimator.AddAnimation("Walk", new List<int>() { 4, 5, 6, 7 }, 6, true);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 8 }, 8, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 9, 10 }, 8, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 14 }, 6, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 15, 16, 17 }, 6, false);

            spriteAnimator.Paused = false;

            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }


        private static Entity CreateHiddenMonster(Vector2 spawnPos, SpriteSheet sheet, bool leaveRemains)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            Collider collider = new Collider(0.2f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(spawnPos, 0f) { CollideWithMap = true, CollideWithEntityMap = true });
            e.AddComponent(new RaycastSprite(sheet, 0));
            e.AddComponent(new Physics(1f) { Mass = 0.8f });
            Enemy enemy = new Enemy() { FiringRange = 1, HearingRange = 10, VisibleRange = 7, FieldOfView = 360 };

            e.AddComponent(enemy);
            e.AddComponent(new SpriteAnimator());

            HealthPoints healthPoints = new HealthPoints(70);
            e.AddComponent(healthPoints);


            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            ActorStateMachine actorStateMachine = new ActorStateMachine(sTilemap);
            healthPoints.Dying += actorStateMachine.ActorDying;
            metaBehavior.AddBehavior(actorStateMachine);

            actorStateMachine.AddStartState(new StandHidden());
            actorStateMachine.AddState(new BreakFree() { LeaveRemains = leaveRemains });
            actorStateMachine.AddState(new Chase());
            actorStateMachine.AddState(new PrepareFire());
            actorStateMachine.AddState(new Fire());
            actorStateMachine.AddState(new Die());
            actorStateMachine.AddState(new Pain());
            e.AddComponent(metaBehavior);
            healthPoints.ReceivedDamage += actorStateMachine.ActorHit;
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateCabinetGuy(Vector2 spawnPos)
        {
            SpriteSheet guard = AssetManager.Default.LoadSpriteSheet("Enemies/BlakeStone/cabinetguy_sheet.png", 64, 64);
            Entity e = CreateHiddenMonster(spawnPos, guard, true);

            SpriteAnimator spriteAnimator = e.GetComponent<SpriteAnimator>();

            spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 6, true);
            spriteAnimator.AddAnimation("BreakFree", new List<int>() { 1, 2, 3 }, 3, false);
            spriteAnimator.AddAnimation("Walk", new List<int>() { 5, 6, 7, 8 }, 6, true);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 9 }, 8, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 10, 11 }, 8, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 15 }, 6, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 16, 17, 18, 19, 20 }, 6, false);

            spriteAnimator.Paused = false;

            e.Refresh();// always call Refresh() when adding/removing components!
            //collider.Collided += metaBehavior.OnCollision;
            return e;
        }
        public static Entity CreateGuard(string spriteSheet, Vector2 position, float rotation, bool isActorPatrolling)
        {

            SpriteSheet guard = AssetManager.Default.LoadSpriteSheet(spriteSheet, 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            Collider collider = new Collider(0.2f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(position, rotation) { CollideWithMap = true, CollideWithEntityMap = true });
            e.AddComponent(new RaycastSprite(guard, 0));
            e.AddComponent(new Physics(1f) { Mass = 0.8f });
            Enemy enemy = new Enemy() { FiringRange = 5, HearingRange = 10, VisibleRange = 7, FieldOfView = 120 };

            e.AddComponent(enemy);

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            //spriteAnimator.AddDirectionalAnimationByFrames("Idle", 6, 0, 8, 1, true);
            //spriteAnimator.AddDirectionalAnimationByFrames("Walk", 6, 1, 8, 4, true);
            spriteAnimator.AddDirectionalAnimationByAngle("Idle", 5, 0, 8, 1, 5, true);
            spriteAnimator.AddDirectionalAnimationByAngle("Walk", 6, 1, 8, 4, 5, true);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 44, 40, 41, 42, 43 }, 6, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 44, 45 }, 6, false);
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 46, 47 }, 8, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 48, 47, 46 }, 8, false);
            spriteAnimator.Paused = false;
            e.AddComponent(spriteAnimator);

            HealthPoints healthPoints = new HealthPoints(25);
            e.AddComponent(healthPoints);



            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            ActorStateMachine actorStateMachine = new ActorStateMachine(sTilemap);
            healthPoints.Dying += actorStateMachine.ActorDying;
            metaBehavior.AddBehavior(actorStateMachine);
            Stand standState = new Stand() { DetectPlayerSoundCues = new List<SoundCue>() { SoundCue.Achtung, SoundCue.Alarm, SoundCue.WerDa } };
            Path pathState = new Path();
            if (isActorPatrolling)
            {
                actorStateMachine.AddState(standState);
                actorStateMachine.AddStartState(pathState);
            }
            else
            {
                actorStateMachine.AddStartState(standState);
                actorStateMachine.AddState(pathState);
            }

            actorStateMachine.AddState(new Chase());
            actorStateMachine.AddState(new PrepareFire());
            actorStateMachine.AddState(new Fire() { FireSoundCues = new List<SoundCue>() { SoundCue.Gunshot01, SoundCue.Gunshot02, SoundCue.Gunshot03 } });
            actorStateMachine.AddState(new Die() { DyingSoundCues = new List<SoundCue>() { SoundCue.MeinLeben }, SpawnAfterDeathFunction = pos => CreateAmmoPickup(pos) });
            actorStateMachine.AddState(new Pain() { PainSoundCues = new List<SoundCue>() { SoundCue.EnemyPain } });
            e.AddComponent(metaBehavior);
            healthPoints.ReceivedDamage += actorStateMachine.ActorHit;
            e.Refresh();// always call Refresh() when adding/removing components!
            //collider.Collided += metaBehavior.OnCollision;
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateBat(Vector2 position)
        {
            SpriteSheet bat = AssetManager.Default.LoadSpriteSheet("Enemies/Catacomb/bat_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            Collider collider = new Collider(0.2f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(position, 0f) { CollideWithMap = true, CollideWithEntityMap = true, MaxSpeed = 14 });
            e.AddComponent(new RaycastSprite(bat, 0));
            e.AddComponent(new Physics(2) { Mass = 0.8f });
            Enemy enemy = new Enemy() { FiringRange = 1.5f, HearingRange = 10, VisibleRange = 7, FieldOfView = 360 };

            e.AddComponent(enemy);

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("Walk", new List<int>() { 0, 1, 2, 3 }, 3, true);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 3 }, 4, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 3 }, 4, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 4, 5 }, 4, false);
            spriteAnimator.CurrentAnimation = "Walk";
            spriteAnimator.ResetAndPlay();

            e.AddComponent(spriteAnimator);

            HealthPoints healthPoints = new HealthPoints(10);
            e.AddComponent(healthPoints);



            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            ActorStateMachine actorStateMachine = new ActorStateMachine(sTilemap);
            healthPoints.Dying += actorStateMachine.ActorDying;
            metaBehavior.AddBehavior(actorStateMachine);
            actorStateMachine.AddStartState(new Stand());
            actorStateMachine.AddState(new Chase());
            actorStateMachine.AddState(new PrepareFire());
            actorStateMachine.AddState(new Fire());
            actorStateMachine.AddState(new Die() { LeaveCorpse = false });
            actorStateMachine.AddState(new Pain());
            e.AddComponent(metaBehavior);
            healthPoints.ReceivedDamage += actorStateMachine.ActorHit;
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateBlueMonster(Vector2 position)
        {
            SpriteSheet guard = AssetManager.Default.LoadSpriteSheet("Enemies/Catacomb/bluemonster.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            Collider collider = new Collider(0.2f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(position, 0f) { CollideWithMap = true, CollideWithEntityMap = true, MaxSpeed = 8 });
            e.AddComponent(new RaycastSprite(guard, 0));
            e.AddComponent(new Physics(2) { Mass = 0.8f });
            Enemy enemy = new Enemy() { FiringRange = 1.5f, HearingRange = 10, VisibleRange = 7, FieldOfView = 360 };

            e.AddComponent(enemy);

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("Walk", new List<int>() { 0, 1, 2, 3 }, 3, true);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 4, 5, 6 }, 4, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 7 }, 4, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 8, 9, 10 }, 4, false);
            spriteAnimator.CurrentAnimation = "Walk";
            spriteAnimator.ResetAndPlay();

            e.AddComponent(spriteAnimator);

            HealthPoints healthPoints = new HealthPoints(10);
            e.AddComponent(healthPoints);



            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            ActorStateMachine actorStateMachine = new ActorStateMachine(sTilemap);
            healthPoints.Dying += actorStateMachine.ActorDying;
            metaBehavior.AddBehavior(actorStateMachine);
            actorStateMachine.AddStartState(new Stand());
            actorStateMachine.AddState(new Chase());
            actorStateMachine.AddState(new PrepareFire());
            actorStateMachine.AddState(new Fire());
            actorStateMachine.AddState(new Die());
            actorStateMachine.AddState(new Pain());
            e.AddComponent(metaBehavior);
            healthPoints.ReceivedDamage += actorStateMachine.ActorHit;
            e.Refresh();// always call Refresh() when adding/removing components!

            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateZombie(Vector2 position, SpawnFunction dropFunction)
        {
            SpriteSheet guard = AssetManager.Default.LoadSpriteSheet("Enemies/Catacomb/zombie_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Enemy";
            Collider collider = new Collider(0.2f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(position, 0f) { CollideWithMap = true, CollideWithEntityMap = true, MaxSpeed = 4 });
            e.AddComponent(new RaycastSprite(guard, 0));
            e.AddComponent(new Physics(0.5f) { Mass = 0.8f });
            Enemy enemy = new Enemy() { FiringRange = 1.5f, HearingRange = 10, VisibleRange = 7, FieldOfView = 120 };

            e.AddComponent(enemy);

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            //spriteAnimator.AddDirectionalAnimationByFrames("Idle", 6, 0, 8, 1, true);
            //spriteAnimator.AddDirectionalAnimationByFrames("Walk", 6, 1, 8, 4, true);
            spriteAnimator.AddAnimation("BreakFree", new List<int>() { 0, 1, 2, 3 }, 3, false);
            spriteAnimator.AddAnimation("Walk", new List<int>() { 4, 5, 6, 7 }, 3, true);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 8 }, 4, false);
            spriteAnimator.AddAnimation("Hit", new List<int>() { 9 }, 4, false);
            spriteAnimator.AddAnimation("Dying", new List<int>() { 10, 11 }, 4, false);
            spriteAnimator.CurrentAnimation = "BreakFree";
            spriteAnimator.ResetAndPlay("Walk");

            e.AddComponent(spriteAnimator);

            HealthPoints healthPoints = new HealthPoints(60);
            e.AddComponent(healthPoints);



            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            ActorStateMachine actorStateMachine = new ActorStateMachine(sTilemap);
            healthPoints.Dying += actorStateMachine.ActorDying;
            metaBehavior.AddBehavior(actorStateMachine);
            actorStateMachine.AddStartState(new BreakFree() { LeaveRemains = false });
            actorStateMachine.AddState(new Chase());
            actorStateMachine.AddState(new PrepareFire());
            actorStateMachine.AddState(new Fire());
            actorStateMachine.AddState(new Die() { SpawnAfterDeathFunction = dropFunction });
            actorStateMachine.AddState(new Pain());
            e.AddComponent(metaBehavior);
            healthPoints.ReceivedDamage += actorStateMachine.ActorHit;
            e.Refresh();// always call Refresh() when adding/removing components!
            //collider.Collided += metaBehavior.OnCollision;
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateDecoSprite(SpriteSheet spriteTextures, Vector2 spawnPos, int tileIndex, bool isObstructing)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = isObstructing ? "Obstacle" : "Deco";
            e.AddComponent(new Transform(spawnPos, 0f));
            e.AddComponent(new RaycastSprite(spriteTextures, tileIndex));
            if (isObstructing)
            {
                e.AddComponent(new Collider(0.2f));
            }
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }



        public static Entity CreateDestroyableObstacle(SpriteSheet spriteTextures, Vector2 spawnPos, int tileIndex, int deathFrames, int destroyedFrames)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = "Obstacle";
            e.AddComponent(new Transform(spawnPos, 0f));
            e.AddComponent(new RaycastSprite(spriteTextures, tileIndex));
            e.AddComponent(new Collider(0.5f));
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("Idle", new List<int>() { tileIndex }, 6, false);
            List<int> deathAnimationIndices = new List<int>();
            for (int i = tileIndex + 1; i <= tileIndex + deathFrames; i++)
            {
                deathAnimationIndices.Add(i);
            }
            List<int> destroyedAnimationIndices = new List<int>();
            for (int i = tileIndex + deathFrames + 1; i <= tileIndex + deathFrames + destroyedFrames; i++)
            {
                destroyedAnimationIndices.Add(i);
            }
            spriteAnimator.AddAnimation("Dying", deathAnimationIndices, 6, false);
            spriteAnimator.AddAnimation("Destroyed", destroyedAnimationIndices, 6, true);
            spriteAnimator.StartPlay("Idle");
            e.AddComponent(spriteAnimator);
            HealthPoints healthPoints = new HealthPoints(20);
            healthPoints.Dying += delegate
                                      {
                                          spriteAnimator.CurrentAnimation = "Dying";
                                          spriteAnimator.ResetAndPlay("Destroyed");
                                      };
            e.AddComponent(healthPoints);


            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }



        public static Entity CreateCorpse(Entity enemy, Vector2 position)
        {
            RaycastSprite raycastSprite = enemy.GetComponent<RaycastSprite>();
            Entity e = sWorld.CreateEntity();
            e.Group = "Deco";
            e.AddComponent(new Transform(position, 0f));
            e.AddComponent(new RaycastSprite(raycastSprite.SpriteSheet, raycastSprite.FrameIndex));
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateHuntBody(int memberIndex, Vector2 position)
        {
            SpriteSheet guard = AssetManager.Default.LoadSpriteSheet("Specials/Zelda/hunt_killed.png", 128, 128);
            Entity e = sWorld.CreateEntity();
            e.Group = "Deco";
            e.AddComponent(new Transform(position, 0f));
            e.AddComponent(new RaycastSprite(guard, memberIndex));
            e.Refresh();// always call Refresh() when adding/removing components!
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateDukeShotgun(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet shotgun = AssetManager.Default.LoadSpriteSheet("Weapons/Duke/dukeshotgun.png", 128, 128);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 240.0f;
            int frameHeight = 128;


            int xpos = (int)(screenDimension.X * 0.7);
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(shotgun, 0) { Scale = scale, Origin = new Vector2(64, 64) });
            e.AddComponent(new Weapon("Shotgun of the Duke", 5, 15, 8, 20) { PenetrationCount = 3, Range = 10, Accuracy = 45, ShotCount = 7 });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            animation.Loop = true;
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("Fire", new List<int>() { 1, 2, 3, 4, 5, 6, 5, 4, 3 }, 10, false);
            spriteAnimator.CurrentAnimation = "Idle";
            e.AddComponent(spriteAnimator);
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(7, new Vector2(xpos, ypos), 1);
            e.AddComponent(fpsWeaponAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateDoomShotgun(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet shotgun = AssetManager.Default.LoadSpriteSheet("Weapons/Doom/doomshotgun_sheet.png", 128, 128);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 320f;
            int frameHeight = 128;
            int xpos = (int)(screenDimension.X * 0.5);
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(shotgun, 0) { Scale = scale, Origin = new Vector2(64, 64) });
            e.AddComponent(new Weapon("Shotgun of Doom", 5, 15, 8, 20) { PenetrationCount = 4, Range = 10, Accuracy = 50, ShotCount = 7 });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            animation.Loop = true;
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("Fire", new List<int>() { 1, 2, 3, 4, 3, 2 }, 7, false);
            spriteAnimator.CurrentAnimation = "Idle";
            e.AddComponent(spriteAnimator);
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(5, new Vector2(xpos, ypos), 1);
            e.AddComponent(fpsWeaponAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateDoomPunch(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet punch = AssetManager.Default.LoadSpriteSheet("Weapons/Doom/punch_sheet.png", 128, 128);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 320f;
            int frameHeight = 128;
            int xpos = (int)(screenDimension.X * 0.3);
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale));
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(punch, 0) { Scale = scale, Origin = new Vector2(64, 64), Depth = 0.5f });
            e.AddComponent(new Weapon("Punch of the marine", 2, 20, -1, -1) { PenetrationCount = 1, Range = 1.5f, IsRanged = false, NeedsAmmo = false, Accuracy = 0 });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            animation.Loop = true;
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("Fire", new List<int>() { 1, 2, 3, 2, 1 }, 7, false);
            spriteAnimator.CurrentAnimation = "Idle";
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(4, new Vector2(xpos, ypos), 3);
            e.AddComponent(fpsWeaponAnimator);
            fpsWeaponAnimator.AddFramePosition(0, new Vector2((int)(screenDimension.X * 0.8), ypos));
            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateMagicHand(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet hand = AssetManager.Default.LoadSpriteSheet("Weapons/Catacomb/hand.png", 88, 72);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 320f;
            int xpos = (int)(screenDimension.X * 0.5f + (scale * 4f));
            int ypos = (int)(screenDimension.Y - ((hand.FrameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(hand, 0) { Scale = scale, Depth = 0.5f, Origin = new Vector2(hand.FrameWidth / 2f, hand.FrameHeight / 2f) });
            e.AddComponent(new Weapon("Power of magic", 1, 4, -1, -1) { PenetrationCount = 1, Range = 20f, IsRanged = true, NeedsAmmo = false, ProjectileCreationFunction = CreateFireball });

            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("Fire", new List<int>() { 0 }, 9, false);
            spriteAnimator.CurrentAnimation = "Idle";
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(1, new Vector2(xpos, ypos), 0);
            e.AddComponent(fpsWeaponAnimator);
            e.AddComponent(spriteAnimator);

            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateBlakeStoneAutoChargePistol(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet pistol = AssetManager.Default.LoadSpriteSheet("Weapons/BlakeStone/smallgun_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 160f;
            int frameHeight = 64;
            int xpos = (int)(screenDimension.X * 0.5 - (scale * 3f));
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(pistol, 0) { Scale = scale, Origin = new Vector2(32, 32) });
            e.AddComponent(new Weapon("Blakes Auto Charge Pistol", 3, 12, 10, 20) { PenetrationCount = 1, Range = 40, NeedsAmmo = false, Accuracy = 4, IsSilent = true, IsAutomatic = false });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 1, 2, 3, 4, 0 }, 9, false);
            spriteAnimator.CurrentAnimation = "Idle";
            spriteAnimator.Paused = false;

            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(5, new Vector2(xpos, ypos), 2);
            e.AddComponent(fpsWeaponAnimator);
            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateWolfRifle(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet pistol = AssetManager.Default.LoadSpriteSheet("Weapons/Wolf/assault_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 160f;
            int frameHeight = 64;
            int xpos = (int)(screenDimension.X * 0.5);
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(pistol, 0) { Scale = scale, Origin = new Vector2(32, 32) });
            e.AddComponent(new Weapon("German Assault Rifle", 5, 15, 30, 99) { PenetrationCount = 1, Range = 40, IsAutomatic = true, FireSoundCue = SoundCue.MachineGun, Accuracy = 12 });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            animation.Loop = true;
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 1 }, 5, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 2, 3 }, 7, true);
            spriteAnimator.AddAnimation("EndFire", new List<int>() { 4 }, 6, false);
            spriteAnimator.CurrentAnimation = "Idle";
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(5, new Vector2(xpos, ypos), 2);
            e.AddComponent(fpsWeaponAnimator);
            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateWolfGatling(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet pistol = AssetManager.Default.LoadSpriteSheet("Weapons/Wolf/gatling_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 160f;
            int frameHeight = 64;
            int xpos = (int)(screenDimension.X * 0.5);
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(pistol, 0) { Scale = scale, Origin = new Vector2(32, 32) });
            e.AddComponent(new Weapon("GATLING!", 5, 15, 30, 99) { PenetrationCount = 2, Range = 40, IsAutomatic = true, FireSoundCue = SoundCue.GatlingGun, Accuracy = 20 });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            animation.Loop = true;
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("PrepareFire", new List<int>() { 1 }, 5, false);
            spriteAnimator.AddAnimation("Fire", new List<int>() { 2, 3 }, 14, true);
            spriteAnimator.AddAnimation("EndFire", new List<int>() { 4 }, 6, false);
            spriteAnimator.CurrentAnimation = "Idle";
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(5, new Vector2(xpos, ypos), 2);
            e.AddComponent(fpsWeaponAnimator);
            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateWolfKnife(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet pistol = AssetManager.Default.LoadSpriteSheet("Weapons/Wolf/knife_sheet.png", 64, 64);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 100f;
            int frameHeight = 64;
            int frameWidth = 64;
            int xpos = (int)(screenDimension.X * 0.5);
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));

            e.AddComponent(new Sprite(pistol, 0) { Scale = scale, Origin = new Vector2(frameWidth / 2, frameHeight / 2) });
            e.AddComponent(new Weapon("S.S. Knife", 4, 24, -1, -1) { PenetrationCount = 1, Range = 1.5f, NeedsAmmo = false, FireSoundCue = SoundCue.Knife, Accuracy = 0, IsSilent = true });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            animation.Loop = true;
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("Fire", new List<int>() { 1, 2, 3, 4, 0 }, 9, false);
            spriteAnimator.CurrentAnimation = "Idle";
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(5, new Vector2(xpos, ypos), 3);
            e.AddComponent(fpsWeaponAnimator);
            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateWeaponPickup(SpriteSheet spriteTextures, Vector2 spawnPos, int tileIndex, string pickupMessage, int inventorySlotIndex, Func<Point, int, Entity> creationFunction)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = "Pickup";
            Collider collider = new Collider(0.4f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(spawnPos, 0f));
            e.AddComponent(new RaycastSprite(spriteTextures, tileIndex));

            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            metaBehavior.AddBehavior(new RemoveOnCollision());
            e.AddComponent(metaBehavior);

            e.Refresh();// always call Refresh() when adding/removing components!
            collider.CollidedWithEntity += delegate (object o, CollisionEventArgs args)
            {
                metaBehavior.OnCollision(o, args);
                CreateMessage(pickupMessage);
                RaycastGame.GiveWeaponToPlayer(inventorySlotIndex, creationFunction);
            };
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateRottPistol(Point screenDimension, int statusbarHeight)
        {
            SpriteSheet pistol = AssetManager.Default.LoadSpriteSheet("Weapons/Rott/pistol_sheet.png", 128, 128);
            Entity e = sWorld.CreateEntity();
            e.Group = "Hud";
            float scale = screenDimension.X / 320f;
            int frameHeight = 128;
            int xpos = (int)(screenDimension.X * 0.5 + 8 * scale);
            int ypos = (int)(screenDimension.Y - ((frameHeight / 2f) * scale) - statusbarHeight);
            e.AddComponent(new Transform(xpos, ypos, 0f));
            e.AddComponent(new Sprite(pistol, 0) { Scale = scale, Origin = new Vector2(64, 64) });
            e.AddComponent(new Weapon("H.U.N.T. Pistol", 5, 15, 30, 60) { PenetrationCount = 1, Range = 50, NeedsAmmo = true, FireSoundCue = SoundCue.Pistol1, Accuracy = 5 });
            SpriteAnimator spriteAnimator = new SpriteAnimator();
            Animation animation = spriteAnimator.AddAnimation("Idle", new List<int>() { 0 }, 1, true);
            animation.Loop = true;
            spriteAnimator.Paused = false;
            spriteAnimator.AddAnimation("Fire", new List<int>() { 1, 2, 0 }, 9, false);
            spriteAnimator.CurrentAnimation = "Idle";
            FpsWeaponAnimator fpsWeaponAnimator = new FpsWeaponAnimator(3, new Vector2(xpos, ypos), 1);
            e.AddComponent(fpsWeaponAnimator);
            e.AddComponent(spriteAnimator);
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        public static Entity CreateHudFace(Point screenDimension, EventHandler<EventArgs> finishedAnimateCallback)
        {
            SpriteSheet face = AssetManager.Default.LoadSpriteSheet("Hud/Doom/marinerfaces.png", 32, 32);
            Entity hudFace = sWorld.CreateEntity();
            hudFace.Group = "Hud";
            float scale = screenDimension.X / 320.0f;
            hudFace.AddComponent(new Transform((int)(10 + 16 * scale), (int)((screenDimension.Y) - ((32 / 2f) * scale)), 0f));
            hudFace.AddComponent(new Sprite(face, 0) { Scale = scale, Depth = 0.2f, Origin = new Vector2(16, 16) });

            SpriteAnimator hudFaceAnimator = new SpriteAnimator();
            hudFaceAnimator.AddAnimation("IdleFullHealth", new List<int>() { 1, 0, 2 }, 0.2f, true);
            hudFaceAnimator.AddAnimation("Idle80Percent", new List<int>() { 9, 8, 10 }, 0.3f, true);
            hudFaceAnimator.AddAnimation("Idle60Percent", new List<int>() { 17, 16, 18 }, 0.5f, true);
            hudFaceAnimator.AddAnimation("Idle40Percent", new List<int>() { 25, 24, 26 }, 0.6f, true);
            hudFaceAnimator.AddAnimation("Idle20Percent", new List<int>() { 33, 32, 34 }, 1, true);


            hudFaceAnimator.AddAnimation("GrinFullHealth", new List<int>() { 3 }, 0.4f, false);
            hudFaceAnimator.AddAnimation("Grin80Percent", new List<int>() { 11 }, 0.3f, false);
            hudFaceAnimator.AddAnimation("Grin60Percent", new List<int>() { 19 }, 0.3f, false);
            hudFaceAnimator.AddAnimation("Grin40Percent", new List<int>() { 17 }, 0.2f, false);
            hudFaceAnimator.AddAnimation("Grin20Percent", new List<int>() { 25 }, 0.1f, false);

            hudFaceAnimator.AddAnimation("PainFullHealth", new List<int>() { 6 }, 0.8f, false);
            hudFaceAnimator.AddAnimation("Pain80Percent", new List<int>() { 14 }, 0.8f, false);
            hudFaceAnimator.AddAnimation("Pain60Percent", new List<int>() { 22 }, 0.7f, false);
            hudFaceAnimator.AddAnimation("Pain40Percent", new List<int>() { 30 }, 0.6f, false);
            hudFaceAnimator.AddAnimation("Pain20Percent", new List<int>() { 38 }, 0.5f, false);

            hudFaceAnimator.AddAnimation("BigPainFullHealth", new List<int>() { 7 }, 0.8f, false);
            hudFaceAnimator.AddAnimation("BigPain80Percent", new List<int>() { 15 }, 0.7f, false);
            hudFaceAnimator.AddAnimation("BigPain60Percent", new List<int>() { 23 }, 0.6f, false);
            hudFaceAnimator.AddAnimation("BigPain40Percent", new List<int>() { 31 }, 0.5f, false);
            hudFaceAnimator.AddAnimation("BigPain20Percent", new List<int>() { 39 }, 0.5f, false);

            hudFaceAnimator.AddAnimation("LookLeftFullHealth", new List<int>() { 4 }, 1, false);
            hudFaceAnimator.AddAnimation("LookRightFullHealth", new List<int>() { 5 }, 1, false);

            hudFaceAnimator.AddAnimation("LookLeft80Percent", new List<int>() { 12 }, 1, false);
            hudFaceAnimator.AddAnimation("LookRight80Percent", new List<int>() { 13 }, 1, false);

            hudFaceAnimator.AddAnimation("LookLeft60Percent", new List<int>() { 20 }, 1, false);
            hudFaceAnimator.AddAnimation("LookRight60Percent", new List<int>() { 21 }, 1, false);

            hudFaceAnimator.AddAnimation("LookLeft40Percent", new List<int>() { 29 }, 1, false);
            hudFaceAnimator.AddAnimation("LookRight40Percent", new List<int>() { 28 }, 1, false);

            hudFaceAnimator.AddAnimation("LookLeft20Percent", new List<int>() { 37 }, 1, false);
            hudFaceAnimator.AddAnimation("LookRight20Percent", new List<int>() { 36 }, 1, false);

            hudFaceAnimator.AddAnimation("IdleImmortal", new List<int>() { 41 }, 0.1f, true);

            hudFaceAnimator.AddAnimation("Dead", new List<int>() { 40 }, 0.1f, true);

            hudFaceAnimator.CurrentAnimation = "IdleFullHealth";
            hudFaceAnimator.Paused = false;

            hudFaceAnimator.FinishedPlaying += finishedAnimateCallback;

            hudFace.AddComponent(hudFaceAnimator);
            hudFace.Refresh();// always call Refresh() when adding/removing components!
            return hudFace;
        }

        public static Entity CreateKeyPickup(Vector2 spawnPos, int tileIndex, string keyName)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = "Pickup";
            Collider collider = new Collider(0.3f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(spawnPos, 0f));
            e.AddComponent(new RaycastSprite(sTilemap.SpriteTextures, tileIndex));

            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            metaBehavior.AddBehavior(new RemoveOnCollision());
            e.AddComponent(metaBehavior);

            e.Refresh();// always call Refresh() when adding/removing components!
            collider.CollidedWithEntity += delegate (object o, CollisionEventArgs args)
            {
                if (args.CollidingEntity.Group != "Player") return;
                metaBehavior.OnCollision(o, args);
                CreateMessage("Picked up a " + keyName + " Key.");
                RaycastGame.PlayerKeys[keyName] = true;
            };
            sTilemap.AddEntity(e);
            return e;
        }

        public static Entity CreateFoodPickup(Vector2 spawnPos, SpriteSheet sheet, int tileIndex, int hpHealAmount, string messageText)
        {
            EventHandler<PickupEventArgs> pickupCallback = (o, args) => RaycastGame.HealPlayer(args.AmountPickedUp);
            Entity e = sWorld.CreateEntity();
            e.Group = "Pickup";
            Collider collider = new Collider(0.3f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(spawnPos, 0f));
            e.AddComponent(new RaycastSprite(sheet, tileIndex));

            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            metaBehavior.AddBehavior(new RemoveOnCollision());
            e.AddComponent(metaBehavior);

            e.Refresh();// always call Refresh() when adding/removing components!
            collider.CollidedWithEntity += delegate (object o, CollisionEventArgs args)
            {
                if (args.CollidingEntity.Group != "Player") return;
                if (RaycastGame.PlayerHealthPoints.Health != RaycastGame.PlayerHealthPoints.MaxHealth)
                {
                    metaBehavior.OnCollision(o, args);
                    CreateMessage(messageText);
                    pickupCallback(null, new PickupEventArgs() { AmountPickedUp = hpHealAmount });
                }
            };
            sTilemap.AddEntity(e);
            return e;
        }


        public static Entity CreateAmmoPickup(Vector2 position)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = "Pickup";
            Collider collider = new Collider(1f);
            e.AddComponent(collider);
            e.AddComponent(new Transform(position, 0f));
            e.AddComponent(new RaycastSprite(sTilemap.SpriteTextures, 4));

            MetaBehavior metaBehavior = new MetaBehavior(sWorld, e);
            metaBehavior.AddBehavior(new RemoveOnCollision());
            e.AddComponent(metaBehavior);

            e.Refresh();// always call Refresh() when adding/removing components!
            collider.CollidedWithEntity += delegate (object o, CollisionEventArgs args)
            {
                if (RaycastGame.PlayerAmmo != 99)
                {
                    metaBehavior.OnCollision(o, args);
                    CreateMessage("You got some ammo.");
                    RaycastGame.GiveAmmoToPlayer(12);
                }
            };
            sTilemap.AddEntity(e);
            return e;
        }


        public static Entity CreateMessage(string messageText, float lifeTimeInSeconds = 3, bool center = false)
        {
            Entity e = sWorld.CreateEntity();
            e.Group = "Other";
            e.AddComponent(new Message(messageText) { IsCentered = center });
            e.AddComponent(new Expires(lifeTimeInSeconds));
            e.Refresh();// always call Refresh() when adding/removing components!
            return e;
        }

        #endregion

    }
}
