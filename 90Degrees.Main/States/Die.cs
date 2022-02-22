using Microsoft.Xna.Framework;
using raycaster.Scripts;
using System.Collections.Generic;
using MP3Player;
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
            DyingSoundCues = new List<SoundCue>();
            SpawnProbability = 1;
            LeaveCorpse = true;
        }
        public override void BeginState()
        {
            AudioPlayer.PlayRandomEffect(DyingSoundCues.ConvertAll((cue) => { return (int)cue; }));
        }

        public List<SoundCue> DyingSoundCues { get; set; }

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
