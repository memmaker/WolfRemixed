using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace Twengine.Components
{
    public delegate Entity ProjectileCreationFunctionType(Vector2 spawnPos, Vector2 moveDirection);

    public class Weapon : IComponent
    {

        public Weapon(string weaponName, int minDamage, int maxDamage, int currentAmmo, int maxAmmo)
        {
            Name = weaponName;
            IsRanged = true;
            Range = int.MaxValue;
            IsAutomatic = false;
            MinDamagePerHit = minDamage;
            MaxDamagePerHit = maxDamage;
            mMaxAmmo = maxAmmo;
            CurrentAmmo = currentAmmo;
            NeedsAmmo = true;
            Accuracy = 0;
            ShotCount = 1;
            IsSilent = false;
        }

        public string Name { get; set; }

        public bool NeedsAmmo { get; set; }
        public bool IsRanged { get; set; }
        public float Range { get; set; }
        public bool HasAmmo { get { return mCurrentAmmo > 0; } }
        public bool IsAutomatic { get; set; }
        public int PenetrationCount { get; set; }
        public int MinDamagePerHit { get; set; }
        public int MaxDamagePerHit { get; set; }

        private bool mIsFiring;
        public bool IsFiring
        {
            get { return mIsFiring; }
            set { mIsFiring = value; }
        }

        private int mCurrentAmmo;
        public int CurrentAmmo { get { return mCurrentAmmo; } set { mCurrentAmmo = value > mMaxAmmo ? mMaxAmmo : value; } }

        private int mMaxAmmo;


        public int MaxAmmo { get { return mMaxAmmo; } set { mMaxAmmo = value; mCurrentAmmo = mCurrentAmmo > mMaxAmmo ? mMaxAmmo : mCurrentAmmo; } }

        public bool IsProjectileWeapon
        {
            get { return ProjectileCreationFunction != null; }
        }

        public ProjectileCreationFunctionType ProjectileCreationFunction { get; set; }

        public string FireSoundCue { get; set; }

        /// <summary>
        /// In Pixels. This is the X Offset coordinate relative to the middle of the screen where hits can occur. 0 means perfect accuracy.
        /// </summary>
        public int Accuracy { get; set; }

        public int ShotCount { get; set; }

        public bool IsSilent { get; set; }

        public void Fire()
        {
            mIsFiring = true;
        }
    }
}
