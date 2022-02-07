using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using raycaster.StoryTelling;
using Twengine.Helper;
using XNAGameGui.Gui.Widgets;

namespace raycaster.GameStates
{
    class StoryTellingState : GameState, Engine.GameStates.IUpdateable
    {
        private Queue<StoryTellingEvent> mEventQueue;
        private List<StoryTellingEvent> mDeferredEndEvents;
        private StoryTellingEvent mCurrentEvent;
        private GameStateManager mStateManager;
        private KeyboardState mLastKeyboardState;
        private List<StoryTellingEvent> mSourceEvents;
        private MouseState mLastMouseState;

        public bool Enabled => true;

        public StoryTellingState(GameStateManager stateManager, List<StoryTellingEvent> events)
        {

            mStateManager = stateManager;
            mLastKeyboardState = Keyboard.GetState();
            mLastMouseState = Mouse.GetState();
            mSourceEvents = events;
            mEventQueue = new Queue<StoryTellingEvent>();
            mDeferredEndEvents = new List<StoryTellingEvent>();
            
        }

        protected override void OnEntered()
        {
            foreach (StoryTellingEvent storyTellingEvent in mSourceEvents)
            {
                storyTellingEvent.Reset();
                mEventQueue.Enqueue(storyTellingEvent);
            }
            
            Debug.Assert(mEventQueue.Count > 0);
            mCurrentEvent = mEventQueue.Dequeue();
            mCurrentEvent.Begin();
        }

        protected override void OnLeaving()
        {
            mCurrentEvent.End();
            foreach (StoryTellingEvent storyTellingEvent in mDeferredEndEvents)
            {
                storyTellingEvent.End();
            }
            mEventQueue.Clear();
        }
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            
            if (keyboardState.IsKeyDown(Keys.Escape) && mLastKeyboardState.IsKeyUp(Keys.Escape))
                mStateManager.Push(new EscapeMenuState(mStateManager));

            if (mouseState.RightButton == ButtonState.Pressed && mLastMouseState.RightButton == ButtonState.Released)  // cancel this event on space..
            {
                if (mEventQueue.Count == 0)
                {
                    mStateManager.Pop();    // finished all events.. exit..
                    return;
                }
                NextEvent();
            }
            foreach (StoryTellingEvent storyTellingEvent in mDeferredEndEvents)
            {
                storyTellingEvent.Update(gameTime);
            }
            mCurrentEvent.Update(gameTime);

            if (mCurrentEvent.IsFinished() || mCurrentEvent.IsNonBlocking)
            {
                CheckForEmptyQueue();
            }
            mLastKeyboardState = keyboardState;
            mLastMouseState = mouseState;
        }

        private void CheckForEmptyQueue()
        {
            if (mEventQueue.Count == 0)
            {
                mStateManager.Pop();    // finished all events.. exit..
            }
            else
            {
                NextEvent();
            }
        }

        private void NextEvent()
        {
            if (!mCurrentEvent.DeferredEnd)
                mCurrentEvent.End();
            else
                mDeferredEndEvents.Add(mCurrentEvent);
            
            Debug.Assert(mEventQueue.Count != 0);
            
            mCurrentEvent = mEventQueue.Dequeue();
            mCurrentEvent.Begin();
        }

    }
}
