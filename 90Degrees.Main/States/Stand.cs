using raycaster.Scripts;
using System.Collections.Generic;

namespace raycaster.States
{
    public class Stand : ActorGameState
    {
        public override string AnimationName
        {
            get { return "Idle"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Stand; }
        }
        public List<SoundCue> DetectPlayerSoundCues { get; set; }
        public override void Update()
        {
            base.Update();
            if ((AttackOnSight && CanDetectPlayer()) || EnemyWasWounded())
            {
                if (DetectPlayerSoundCues != null)
                    RaycastGame.AudioManager.PlayRandomEffect(DetectPlayerSoundCues.ConvertAll((cue) =>
                    {
                        return (int)cue;
                    }));

                if (PlayerIsInRange(mTransform.Position, mEnemy.FiringRange))
                {
                    NextActorState = ActorState.PrepareShoot;
                }
                else
                {
                    NextActorState = ActorState.Chase;
                }
            }
            else
            {
                NextActorState = ActorState.Stand;
            }

        }

    }
}
