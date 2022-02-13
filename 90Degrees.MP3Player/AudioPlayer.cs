using FMOD;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MP3Player
{

    public class AudioPlayer : IDisposable
    {
        private FMOD.System mSystem = null;
        private Dictionary<int, Sound> mLoadedSounds;
        private Dictionary<int, SoundEffect> mLoadedSoundEffects = new Dictionary<int, SoundEffect>();
        private Dictionary<int, Channel> mSoundsOnChannels;
        private string mContentPrefix;
        private Random mRandom;
        private CHANNEL_CALLBACK mCallback;
        private Channel mMusicChannel;
        private List<int> mMusicPlayList;
        private int mCurrentMusicIndex;
        private int mMaxChannels;
        private bool mIsShuttingDown;

        public AudioPlayer()
        {
            mContentPrefix = "Content/";
            mMaxChannels = 2;
            InitFMOD();
            mRandom = new Random(DateTime.Now.Millisecond);
            mLoadedSounds = new Dictionary<int, Sound>();
            mSoundsOnChannels = new Dictionary<int, Channel>();
            mMusicPlayList = new List<int>();
            mCurrentMusicIndex = -1;
            mCallback = MusicChannelCallback;
            mIsShuttingDown = false;
        }

        private void InitFMOD()
        {
            uint version = 0;
            RESULT result = Factory.System_Create(ref mSystem);
            ERRCHECK(result);

            result = mSystem.getVersion(ref version);
            ERRCHECK(result);
            if (version < VERSION.number)
            {
                throw new Exception("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + VERSION.number.ToString("X") + ".");
            }

            result = mSystem.init(mMaxChannels, INITFLAGS.NORMAL, IntPtr.Zero);
            ERRCHECK(result);
        }


        public void UnloadSound(int cue)
        {
            StopAndUnloadSound(mLoadedSounds[cue]);
            mLoadedSounds.Remove(cue);
        }
        public void AddEffect(SoundEffect audioStream, int cue)
        {
            mLoadedSoundEffects.Add(cue, audioStream);
        }
        public void LoadSound(string filename, int cue, bool isStream)
        {
            MODE flags;
            if (isStream)
                flags = (MODE._2D | MODE.HARDWARE | MODE.CREATESTREAM);
            else
                flags = (MODE._2D | MODE.HARDWARE | MODE.CREATESAMPLE);

            Sound sound = null;
            RESULT result = mSystem.createSound(mContentPrefix + filename, flags, ref sound);
            ERRCHECK(result);
            mLoadedSounds.Add(cue, sound);
        }
        public void PlayRandomSound(List<int> cues)
        {
            if (cues.Count <= 0) return;
            PlaySound(cues[mRandom.Next(0, cues.Count)]);
        }

        public void PlayRandomEffect(List<int> cues)
        {
            if (cues.Count <= 0) return;
            PlayEffect(cues[mRandom.Next(0, cues.Count)]);
        }

        public void StartPlaylist(List<int> playList)
        {
            if (playList.Count <= 0) return;

            mMusicPlayList = playList;
            mCurrentMusicIndex = 0;
            mMusicChannel = PlaySound(mMusicPlayList[mCurrentMusicIndex]);
            mMusicChannel.setCallback(mCallback);

        }

        public void PlayEffect(int cue)
        {
            if (mLoadedSoundEffects.ContainsKey(cue))
            {
                mLoadedSoundEffects[cue].Play();
            }
        }

        public void StopPlaylist()
        {
            mMusicPlayList = null;
            if (mMusicChannel != null)
            {
                mMusicChannel.stop();
                mMusicChannel.setCallback(null);
                mMusicChannel = null;
            }
        }

        public Channel PlaySound(int cue, bool looping = false)
        {
            Channel channel = null;
            if (mLoadedSounds.ContainsKey(cue))
            {
                mLoadedSounds[cue].setMode(looping ? MODE.LOOP_NORMAL : MODE.LOOP_OFF);

                RESULT result = mSystem.playSound(CHANNELINDEX.FREE, mLoadedSounds[cue], false, ref channel);

                ERRCHECK(result);

                if (mSoundsOnChannels.ContainsKey(cue))
                {
                    mSoundsOnChannels[cue] = channel;
                }
                else
                {
                    mSoundsOnChannels.Add(cue, channel);
                }
            }
            return channel;
        }

        private RESULT MusicChannelCallback(IntPtr channelraw, CHANNEL_CALLBACKTYPE type, IntPtr commanddata1, IntPtr commanddata2)
        {
            if (mIsShuttingDown || mMusicPlayList == null) return RESULT.ERR_NOTREADY;
            if (type == CHANNEL_CALLBACKTYPE.END)
            {
                mCurrentMusicIndex = (mCurrentMusicIndex + 1) % mMusicPlayList.Count;
                mMusicChannel = PlaySound(mMusicPlayList[mCurrentMusicIndex]);
            }

            return RESULT.OK;
        }

        public void StopSound(int cue)
        {
            if (mSoundsOnChannels.ContainsKey(cue))
            {
                mSoundsOnChannels[cue].stop();
                mSoundsOnChannels.Remove(cue);
            }
        }

        public void StopAllSounds()
        {
            for (int i = mMaxChannels - 1; i >= 0; i--)
            {
                Channel channel = null;
                mSystem.getChannel(i, ref channel);
                channel.stop();
            }
            mSoundsOnChannels.Clear();
        }

        private void StopAndUnloadSound(Sound sound)
        {
            if (sound != null)
            {
                RESULT result = sound.release();
                ERRCHECK(result);
            }
        }

        public void Reset()
        {
            if (mMusicChannel != null)
            {
                mMusicChannel.setCallback(null);
                mMusicChannel = null;
            }
            StopAllSounds();
            foreach (Sound sound in mLoadedSounds.Values)
            {
                sound.release();
            }
        }

        public void Dispose()
        {
            mIsShuttingDown = true;
            if (mMusicChannel != null)
            {
                mMusicChannel.setCallback(null);
                mMusicChannel = null;
            }
            StopAllSounds();
            foreach (Sound sound in mLoadedSounds.Values)
            {
                sound.release();
            }
            mSystem.close();
        }

        private void ERRCHECK(RESULT result)
        {
            if (result != RESULT.OK)
            {
                Debug.Print("FMOD error! " + result + " - " + Error.String(result));
                throw new ApplicationException("FMOD ERROR!");
                Environment.Exit(-1);
            }
        }

        public void Update()
        {
            RESULT result = mSystem.update();
            ERRCHECK(result);
            /*
            foreach (Channel channel in mSoundsOnChannels.Values)
            {
                bool isplaying = false;
                result = channel.isPlaying(ref isplaying);
                ERRCHECK(result);

            }
             * */
        }

    }
}
