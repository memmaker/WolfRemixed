using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace Twengine.Components
{
    public struct Animation
    {
        public string Name { get; set; }
        public List<int> AnimationIndices { get; set; }
        public float TimePerFrame { get; set; }
        public bool Loop { get; set; }

        public bool IsDirectional { get; set; }
    }

    public struct DirectionalAnimation
    {
        public string Name { get; set; }
        public List<List<int>> AnimationIndices { get; set; }
        public float TimePerFrame { get; set; }
        public bool Loop { get; set; }
    }
    public class SpriteAnimator : IComponent
    {
        private int mAnimationIndex;
        private float mTotalElapsed;

        private Dictionary<string, Animation> mAnimations;
        public event EventHandler<EventArgs> FinishedPlaying;
        
        public bool Paused{ get; set; }

        public int CurrentFrameIndex { get { return mAnimations[mCurrentAnimation].AnimationIndices[mAnimationIndex]; } }
        private string mCurrentAnimation;
        private string mFollowUpAnimation;
        private Dictionary<string, DirectionalAnimation> mDirectionalAnimations;
        public bool EnteredFrameThisTick { get; set; }

        public string CurrentAnimation
        {
            get { return mCurrentAnimation; }
            set
            {
                if (mAnimations.ContainsKey(value) && mCurrentAnimation != value)
                {
                    mCurrentAnimation = value;
                    mAnimationIndex = 0;
                }
            }
        }

        public bool IsCurrentAnimationDirectional { get { return mAnimations[mCurrentAnimation].IsDirectional; } }

        public SpriteAnimator()
        {
            mAnimations = new Dictionary<string, Animation>();
            mDirectionalAnimations = new Dictionary<string, DirectionalAnimation>();
            mAnimationIndex = 0;
            mCurrentAnimation = "Idle";
        }

        public void ResetAndPlay()
        {
            mAnimationIndex = 0;
            mTotalElapsed = 0f;
            Paused = false;
            mFollowUpAnimation = "";
            EnteredFrameThisTick = true;
        }
        public void ResetAndPlay(string followUpAnimation)
        {
            ResetAndPlay();
            mFollowUpAnimation = followUpAnimation;
        }
        public void StartPlay(string animation)
        {
            CurrentAnimation = animation;
            ResetAndPlay();
        }
        public int GetDirectionalFrameIndex(string animation, int directionIndex, int frameIndex)
        {
            return mDirectionalAnimations[animation].AnimationIndices[frameIndex][directionIndex];
        }

        /// <summary>
        /// Use this if one animation frame is varied by direction
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fps"></param>
        /// <param name="startIndex"></param>
        /// <param name="directionCount"></param>
        /// <param name="frameCount"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public DirectionalAnimation AddDirectionalAnimationByFrames(string name, int fps, int startIndex, int directionCount, int frameCount, bool loop)
        {
            int currentIndex = startIndex;
            List<int> animationIndices = new List<int>();
            List<List<int>> directionalAnimatedSpriteFrameIndices = new List<List<int>>();
            for (int i = startIndex; i < startIndex + frameCount; i++)
            {
                animationIndices.Add(animationIndices.Count);
                List<int> frame = new List<int>();  // all directional frames for one animation frame
                for (int j = 0; j < directionCount; j++)
                {
                    frame.Add(currentIndex);
                    currentIndex++;
                }
                directionalAnimatedSpriteFrameIndices.Add(frame);
            }


            DirectionalAnimation directionalAnimation = new DirectionalAnimation
            {
                AnimationIndices = directionalAnimatedSpriteFrameIndices,
                Name = name,
                TimePerFrame = 1.0f / fps,
                Loop = loop,
            };
            mDirectionalAnimations[name] = directionalAnimation;
            
            
            Animation newAnim = new Animation
            {
                AnimationIndices = animationIndices,
                Name = name,
                TimePerFrame = 1.0f / fps,
                Loop = loop,
                IsDirectional = true
            };
            mAnimations[name] = newAnim;
            return directionalAnimation;
        }

        /// <summary>
        /// Use this if every frame is ordered by direction..
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fps"></param>
        /// <param name="startIndex"></param>
        /// <param name="directionCount"></param>
        /// <param name="frameCount"></param>
        /// <param name="offset"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public DirectionalAnimation AddDirectionalAnimationByAngle(string name, int fps, int startIndex, int directionCount, int frameCount, int offset, bool loop)
        {
            int currentindex = startIndex;
            List<List<int>> directionalAnimatedSpriteFrameIndices = new List<List<int>>();
            List<List<int>> dirList = new List<List<int>>();
            for (int i = 0; i < directionCount; i++)
            {
                
                List<int> dirFrames = new List<int>();
                for (int j = 0; j < frameCount; j++)
                {
                    dirFrames.Add(currentindex);
                    currentindex++;
                }
                dirList.Add(dirFrames);
                currentindex += offset - frameCount;
            }

            List<int> animationIndices = new List<int>();
            for (int i = 0; i < frameCount; i++)
            {
                animationIndices.Add(animationIndices.Count);
                List<int> frame = new List<int>();
                for (int j = 0; j < directionCount; j++)
                {
                    frame.Add(dirList[j][i]);
                    currentindex++;
                }
                directionalAnimatedSpriteFrameIndices.Add(frame);
            }

            DirectionalAnimation directionalAnimation = new DirectionalAnimation
            {
                AnimationIndices = directionalAnimatedSpriteFrameIndices,
                Name = name,
                TimePerFrame = 1.0f / fps,
                Loop = loop,
            };

            Animation newAnim = new Animation
            {
                AnimationIndices = animationIndices,
                Name = name,
                TimePerFrame = 1.0f / fps,
                Loop = loop,
                IsDirectional = true
            };
            mAnimations[name] = newAnim;

            mDirectionalAnimations[name] = directionalAnimation;
            return directionalAnimation;
        }

        public Animation AddAnimation(string name, List<int> animationIndices, float fps, bool loop)
        {
            Animation newAnim = new Animation
                                    {
                                        AnimationIndices = animationIndices,
                                        Name = name,
                                        TimePerFrame = 1.0f/fps,
                                        Loop = loop,
                                        IsDirectional = false
                                    };
            mAnimations[name] = newAnim;
            return newAnim;
        }

        public Animation AddAnimation(string name, List<int> animationIndices, int fps)
        {
            return AddAnimation(name, animationIndices, fps, true);
        }

        public void UpdateFrame(float elapsed)
        {
            if (Paused)
                return;
            mTotalElapsed += elapsed;
            if (mTotalElapsed > mAnimations[mCurrentAnimation].TimePerFrame)
            {
                IncreaseAnimationIndex();
                EnteredFrameThisTick = true;
            }
            else
            {
                EnteredFrameThisTick = false;
            }
        }

        

        private void IncreaseAnimationIndex()
        {
            mAnimationIndex++;
            if (mAnimationIndex > mAnimations[mCurrentAnimation].AnimationIndices.Count - 1)
            {
                if (!mAnimations[mCurrentAnimation].Loop)
                {
                    // no loop
                    StopPlaying();
                    return;
                }
                // loop
                mAnimationIndex = 0;
            }
            mTotalElapsed -= mAnimations[mCurrentAnimation].TimePerFrame;
        }

        private void StopPlaying()
        {
            mAnimationIndex = mAnimations[mCurrentAnimation].AnimationIndices.Count - 1;
            Paused = true;
            OnFinishedPlaying();

            if (mFollowUpAnimation != "")
            {
                mCurrentAnimation = mFollowUpAnimation;
                mFollowUpAnimation = "";
                ResetAndPlay();
            }
        }

        private void OnFinishedPlaying()
        {
            if (FinishedPlaying == null) return;
            FinishedPlaying(this, new EventArgs());
        }
    }
}
