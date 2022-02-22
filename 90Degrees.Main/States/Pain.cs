using raycaster.Scripts;
using System.Collections.Generic;
using MP3Player;

namespace raycaster.States
{
    public class Pain : ActorGameState
    {

        public override string AnimationName
        {
            get { return "Hit"; }
        }

        public override ActorState ActorStateType
        {
            get { return ActorState.Pain; }
        }
        public Pain()
        {
            PainSoundCues = new List<SoundCue>();
        }
        public override void FinishedAnimating()
        {
            NextActorState = ActorState.Chase;
        }
        public override void BeginState()
        {
            NextActorState = ActorState.Pain;
            AudioPlayer.PlayRandomEffect(PainSoundCues.ConvertAll<int>((cue) => { return (int)cue; }));
        }

        public List<SoundCue> PainSoundCues { get; set; }


    }
}
