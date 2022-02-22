using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Media;

namespace MP3Player
{

    public static class AudioPlayer
    {
        private static readonly Dictionary<int, Song> mLoadedSounds = new Dictionary<int, Song>();
        private static readonly Dictionary<int, SoundEffectInstance> mLoadedSoundEffects = new Dictionary<int, SoundEffectInstance>();
        private static Random mRandom;
        private static List<int> mMusicPlayList;
        private static int mCurrentMusicIndex;
        
        static AudioPlayer()
        {
            mRandom = new Random(DateTime.Now.Millisecond);
            mMusicPlayList = new List<int>();
            mCurrentMusicIndex = -1;
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
        }

       
        public static void AddEffect(SoundEffect audioStream, int cue)
        {
            mLoadedSoundEffects.Add(cue, audioStream.CreateInstance());
        }
        public static void LoadSong(Song song, int cue)
        {
            mLoadedSounds.Add(cue, song);
        }

        public static void PlayRandomEffect(List<int> cues)
        {
            if (cues.Count <= 0) return;
            PlayEffect(cues[mRandom.Next(0, cues.Count)]);
        }

        public static void StartPlaylist(List<int> playList)
        {
            if (playList.Count <= 0) return;

            mMusicPlayList = playList;
            mCurrentMusicIndex = 0;
            PlaySong(mMusicPlayList[mCurrentMusicIndex]);
        }

        private static void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Stopped && mMusicPlayList != null && mMusicPlayList.Count > 0)
            {
                mCurrentMusicIndex = (mCurrentMusicIndex + 1) % mMusicPlayList.Count;
                PlaySong(mMusicPlayList[mCurrentMusicIndex]);
            }
        }


        public static void PlayEffect(int cue)
        {
            if (mLoadedSoundEffects.ContainsKey(cue))
            {
                mLoadedSoundEffects[cue].Play();
            }
        }

        public static void StopPlaylist()
        {
            mMusicPlayList = null;
            MediaPlayer.Stop();
        }

        public static void PlaySong(int cue, bool looping = false)
        {
            if (mLoadedSounds.ContainsKey(cue))
            {
                MediaPlayer.Play(mLoadedSounds[cue]);
                MediaPlayer.IsRepeating = looping;
            }
        }
        public static void StopSong()
        {
            MediaPlayer.Stop();
        }

        public static void StopAllSounds()
        {
            foreach (var sound in mLoadedSoundEffects.Values)
            {
                sound.Stop();
            }

            MediaPlayer.Stop();
        }
    }
}
