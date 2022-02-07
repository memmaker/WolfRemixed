using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Artemis;
using Microsoft.Xna.Framework;
using Twengine.Components;
using Twengine.Helper;
using Twengine.Managers;
using XNAHelper;

namespace Twengine.SubSystems.Raycast
{
    public delegate void WeaponEventHandler(WeaponEventArgs args);
    public delegate void HitLocationEventHandler(HitLocationEventArgs args);
    public delegate void DamageDealtEventHandler(DamageDealtEventArgs args);

    public class DamageDealtEventArgs : EventArgs
    {
        public Dictionary<Entity, int> DamageDealt { get; set; }
    }

    public class WeaponEventArgs : EventArgs
    {
        public Entity WeaponEntity { get; set; }
        public Weapon WeaponComponent { get; set; }
    }

    public class HitLocationEventArgs : EventArgs
    {
        public Vector2 HitLocation { get; set; }
        public bool IsEnemyHit { get; set; }
    }

    public class FpsWeaponSystem : EntityProcessingSystem
    {
        public event WeaponEventHandler PlayerUsedAmmo;
        public event WeaponEventHandler PlayerWeaponFired;
        public event HitLocationEventHandler BulletHit;
        public event DamageDealtEventHandler DamageDealt;
        private ComponentMapper<Weapon> mWeaponMapper;
        private ComponentMapper<SpriteAnimator> mSpriteMapper;
        private ComponentMapper<FpsWeaponAnimator> mFpsWeaponMapper;
        private Raycaster mRaycaster;
        

        public FpsWeaponSystem(Raycaster raycaster)
            : base(typeof(Weapon), typeof(SpriteAnimator), typeof(FpsWeaponAnimator))
        {
            mRaycaster = raycaster;
        }
        protected override void Begin()
        {
            base.Begin();
            
        }
        public override void Initialize()
        {
            mWeaponMapper = new ComponentMapper<Weapon>(world);
            mSpriteMapper = new ComponentMapper<SpriteAnimator>(world);
            mFpsWeaponMapper = new ComponentMapper<FpsWeaponAnimator>(world);
            
        }

        public override void Process(Entity e)
        {
            Weapon weapon = mWeaponMapper.Get(e);
            SpriteAnimator spriteAnimator = mSpriteMapper.Get(e);
            FpsWeaponAnimator fpsWeaponAnimator = mFpsWeaponMapper.Get(e);
            

            if (weapon.IsAutomatic)
            {
                if (spriteAnimator.CurrentAnimation == "Idle" && weapon.IsFiring)
                    BeginFireAnimationAutomatic(spriteAnimator);
            }
            else if (spriteAnimator.CurrentAnimation != "Fire" && weapon.IsFiring)
            {
                BeginFireAnimation(spriteAnimator);
            }

            if (spriteAnimator.CurrentAnimation == "Fire")
            {
                if (fpsWeaponAnimator.HitOnFrame == spriteAnimator.CurrentFrameIndex && spriteAnimator.EnteredFrameThisTick)
                {
                    OnPlayerFiredWeapon(e, weapon);
                    if (weapon.NeedsAmmo)
                    {
                        OnPlayerUsedAmmo(e, weapon);
                    }
                    if (weapon.IsProjectileWeapon)
                        SpawnProjectile(weapon);
                    else
                        CalculateHits(weapon);
                }
                if (!weapon.IsFiring && weapon.IsAutomatic)
                {
                    EndAutomaticFireAnimation(spriteAnimator);
                }
            }
            weapon.IsFiring = false;
        }

        private void OnPlayerFiredWeapon(Entity weapon, Weapon weaponComponent)
        {
            if (PlayerWeaponFired == null) return;
            PlayerWeaponFired(new WeaponEventArgs() { WeaponEntity = weapon, WeaponComponent = weaponComponent });
        }

        private void OnPlayerUsedAmmo(Entity weapon, Weapon weaponComponent)
        {
            if (PlayerUsedAmmo == null) return;
            PlayerUsedAmmo(new WeaponEventArgs(){WeaponEntity =  weapon, WeaponComponent = weaponComponent});
        }

        private void SpawnProjectile(Weapon weapon)
        {
            weapon.ProjectileCreationFunction(mRaycaster.Camera.Position + (0.3f * mRaycaster.Camera.Direction), mRaycaster.Camera.Direction);
        }

        private void CalculateHits(Weapon weapon)
        {
            Dictionary<Entity, int> damageDealt = new Dictionary<Entity, int>();
            for (int i = 0; i < weapon.ShotCount; i++)
            {
                bool hittableEntityHit = false;
                int weaponAccuracy = weapon.Accuracy;
                int xCoordinate = (mRaycaster.ScreenWidth / 2) + TwenMath.Random.Next(-weaponAccuracy, weaponAccuracy + 1);
                //Debug.Print("xCoordinate " + xCoordinate);
                if (mRaycaster.TargetedEntities[xCoordinate].Count > 0)
                {
                    List<Entity> targetedEntities = GetFirstEntitiesAlive(mRaycaster.TargetedEntities[xCoordinate], weapon.PenetrationCount);
                    foreach (Entity targetedEntity in targetedEntities)
                    {
                        Transform enemyTransform = targetedEntity.GetComponent<Transform>();
                        if (Vector2.Distance(enemyTransform.Position, mRaycaster.Camera.Position) < weapon.Range)
                        {
                            if (!damageDealt.ContainsKey(targetedEntity))
                            {
                                damageDealt.Add(targetedEntity,0);
                            }
                            int damage = TwenMath.Random.Next(weapon.MinDamagePerHit, weapon.MaxDamagePerHit + 1);
                            DealDamage(targetedEntity, damage);
                            
                            damageDealt[targetedEntity] += damage;

                            hittableEntityHit = true;
                            Vector2 dirToPlayer = mRaycaster.Position - enemyTransform.Position;
                            dirToPlayer.Normalize();
                            dirToPlayer *= 0.1f;
                            OnHitAtLocation(enemyTransform.Position + dirToPlayer, true);
                        }
                    }
                }
                if (!hittableEntityHit) // paint bullet hit on wall
                {
                    CalculateWallHit(weapon, xCoordinate);
                }
            }
            if (damageDealt.Count > 0)
            {
                OnDamageDealt(damageDealt);
            }
        }

        private void CalculateWallHit(Weapon weapon, int xCoordinate)
        {
            Vector2 targetLocation = Vector2.Zero;
            BasicRaycastHitInfo aimedAt = mRaycaster.LastHits[xCoordinate];
            float offset = 0.05f;
            if (aimedAt.VisibleWallSide == WallSide.West)
            {
                targetLocation.X = aimedAt.MapX - offset;
                targetLocation.Y = (float)(aimedAt.MapY + aimedAt.WallXOffset);
            }
            else if (aimedAt.VisibleWallSide == WallSide.East)
            {
                targetLocation.X = aimedAt.MapX + 1 + offset;
                targetLocation.Y = (float)(aimedAt.MapY + aimedAt.WallXOffset);
            }
            else if (aimedAt.VisibleWallSide == WallSide.South)
            {
                targetLocation.X = (float)(aimedAt.MapX + aimedAt.WallXOffset);
                targetLocation.Y = aimedAt.MapY + 1 + offset;
            }
            else if (aimedAt.VisibleWallSide == WallSide.North)
            {
                targetLocation.X = (float)(aimedAt.MapX + aimedAt.WallXOffset);
                targetLocation.Y = (aimedAt.MapY - offset);
            }
            if (Vector2.Distance(targetLocation, mRaycaster.Camera.Position) < weapon.Range)
                OnHitAtLocation(targetLocation, false);
        }

        private void OnDamageDealt(Dictionary<Entity, int> damageDealt)
        {
            if (DamageDealt != null)
            {
                DamageDealt(new DamageDealtEventArgs() {DamageDealt = damageDealt});
            }
        }

        private void OnHitAtLocation(Vector2 targetLocation, bool enemyHit)
        {
            if (BulletHit == null) return;
            BulletHit(new HitLocationEventArgs() { HitLocation = targetLocation, IsEnemyHit = enemyHit });
        }

        private void BeginFireAnimationAutomatic(SpriteAnimator spriteAnimator)
        {
            spriteAnimator.CurrentAnimation = "PrepareFire";
            spriteAnimator.ResetAndPlay("Fire");
        }

        private void EndAutomaticFireAnimation(SpriteAnimator spriteAnimator)
        {
            spriteAnimator.CurrentAnimation = "EndFire";
            spriteAnimator.ResetAndPlay("Idle");
        }

        private void BeginFireAnimation(SpriteAnimator spriteAnimator)
        {
            spriteAnimator.StartPlay("Fire");
            spriteAnimator.FinishedPlaying += WeaponFireAnimationFinished;
        }

        private static void DealDamage(Entity targetedEntity, int damagePerHit)
        {
            HealthPoints healthPoints = targetedEntity.GetComponent<HealthPoints>();
            if (healthPoints == null) return;
            healthPoints.DealDamage(damagePerHit);
        }

        private static List<Entity> GetFirstEntitiesAlive(List<Entity> targetedEntities, int count)
        {
            List<Entity> hits = new List<Entity>(count);
            for (int index = targetedEntities.Count - 1; index >= 0; index--)
            {
                Entity targetedEntity = targetedEntities[index];
                if (!targetedEntity.HasComponent<HealthPoints>()) continue;
                HealthPoints healthPoints = targetedEntity.GetComponent<HealthPoints>();
                if (!healthPoints.IsAlive) continue;
                hits.Add(targetedEntity);
                if (hits.Count == count)
                    return hits;
            }
            return hits;
        }

        void WeaponFireAnimationFinished(object sender, EventArgs e)
        {
            SpriteAnimator source = (SpriteAnimator)sender;
            source.FinishedPlaying -= WeaponFireAnimationFinished;
            source.CurrentAnimation = "Idle";
            source.ResetAndPlay();
        }
    }
}
