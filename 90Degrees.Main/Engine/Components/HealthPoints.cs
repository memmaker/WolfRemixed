using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Artemis.Interface;

namespace Twengine.Components
{

    public class KilledEventArgs : EventArgs
    {
        public Entity DamageSource { get; set; }
    }
    public class DamageEventArgs : KilledEventArgs
    {
        public int Damage { get; set; }
    }
    public class HealthPoints : IComponent
    {
        public event EventHandler<KilledEventArgs> Dying;
        public event EventHandler<EventArgs> BeginImmortality;
        public event EventHandler<EventArgs> EndImmortality;
        public event EventHandler<DamageEventArgs> ReceivedDamage;
        public bool IsAlive { get { return mHealth > 0; } }
        private int mMaxHealth;
        public int MaxHealth
        {
            get { return mMaxHealth; }
            set {  mMaxHealth = value; mHealth = mHealth > mMaxHealth ? mMaxHealth : mHealth; }
        }

        private int mHealth;
        private bool mIsImmortal;
        public bool IsImmortal
        {
            get { return mIsImmortal; }
            set
            {
                mIsImmortal = value;
                if (mIsImmortal)
                    OnBeginImmortality();
                else
                    OnEndImmortality();
            }
        }

        private void OnEndImmortality()
        {
            if (EndImmortality == null) return;
            EndImmortality(this, new EventArgs());
        }

        private void OnBeginImmortality()
        {
            if (BeginImmortality == null) return;
            BeginImmortality(this, new EventArgs());
        }

        public int Health
        {
            get { return mHealth; }
            set { mHealth = value > mMaxHealth ? mMaxHealth : value; }
        }
        public HealthPoints(int maxHealth)
        {
            mMaxHealth = maxHealth;
            mHealth = maxHealth;
            IsImmortal = false;
        }
        public void DealDamage(int damage, Entity source)
        {
            if (!IsAlive || IsImmortal) return;
            Health -= damage;
            OnReceivedDamage(damage, source);
            if (!IsAlive)
            {
                OnDying(source);
            }
        }

        public void DealDamage(int damage)
        {
            DealDamage(damage,null);
        }

        private void OnReceivedDamage(int damage, Entity source)
        {
            if (ReceivedDamage == null) return;
            ReceivedDamage(this, new DamageEventArgs() {Damage = damage, DamageSource = source});
        }

        private void OnDying(Entity killer)
        {
            if (Dying == null) return;
            Dying(this, new KilledEventArgs() { DamageSource = killer });
        }

        public void Heal(int hitpoints)
        {
            Health += hitpoints;
        }

        public void RestoreToFull()
        {
            mHealth = mMaxHealth;
        }
    }
}
