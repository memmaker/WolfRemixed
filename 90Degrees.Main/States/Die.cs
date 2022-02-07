using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using raycaster.Scripts;
using Twengine;
using Twengine.Helper;
using XNAHelper;

namespace raycaster.States
{
    public delegate void SpawnFunction(Vector2 spawnPos);

    public class Die : ActorGameState
    {
        public float SpawnProbability { get; set; }

        public override string AnimationName
        {
            get { return "Dying"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Die; }
        }
        public SpawnFunction SpawnAfterDeathFunction { get; set; }
        public Die()
        {
            DyingSoundCues = new List<string>();
            SpawnProbability = 1;
            LeaveCorpse = true;
        }
        public override void BeginState()
        {
            ComponentTwengine.AudioManager.PlayRandomSound(DyingSoundCues);
        }

        public List<string> DyingSoundCues { get; set; }

        public bool LeaveCorpse { get; set; }

        public override void FinishedAnimating()
        {
            if (LeaveCorpse)
                EntitySpawn.CreateCorpse(mSelf, mTransform.Position);
            if (SpawnAfterDeathFunction != null)
                SpawnAmmo();
            RaycastGame.PlayerKilledEnemy(mSelf);
            mSelf.Delete();
        }

        private void SpawnAmmo()
        {
            if (TwenMath.Random.NextDouble() < SpawnProbability)
                SpawnAfterDeathFunction(mTransform.Position + (mTransform.Forward * 0.2f));
        }
    }
}
