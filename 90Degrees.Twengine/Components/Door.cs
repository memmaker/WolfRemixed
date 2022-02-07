using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Microsoft.Xna.Framework;

namespace Twengine.Components
{

    public class Door : Component
    {
        private float mCloseDelayInSeconds;
        public Vector2 SpawnPos { get; set; }
        public event EventHandler<EventArgs> FinishedOpen;
        public event EventHandler<EventArgs> FinishedClose;
        public event EventHandler<EventArgs> BeginOpen;
        public event EventHandler<EventArgs> BeginClose;

        /// <summary>
        /// When this reaches zero, the door closes.
        /// </summary>
        public float CloseTimer { get; set; }

        public Vector2 OpenPosition { get; set; }

        public bool StartAnimating { get; set; }
        /// <summary>
        /// The current state of the door, this defines if the cell on the map is blocked..
        /// </summary>
        public bool IsOpen { get; set; }
        /// <summary>
        /// Is set to true when the door is animating to open.
        /// </summary>
        public bool IsOpening { get; set; }
        /// <summary>
        /// Is set to true when the door is animating to close.
        /// </summary>
        public bool IsClosing { get; set; }
        public Orientation Orientation { get; set; }
        public Door(Vector2 spawnPos, Orientation orientation, float closeDelayInSeconds)
        {
            Orientation = orientation;
            SpawnPos = spawnPos;
            IsOpen = false;
            IsOpening = false;
            IsClosing = false;
            StartAnimating = false;
            CloseTimer = -1f;
            mCloseDelayInSeconds = closeDelayInSeconds;
            SetOpenPosition();
        }

        private void SetOpenPosition()
        {
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    OpenPosition = new Vector2(SpawnPos.X - 1, SpawnPos.Y);
                    break;
                case Orientation.Vertical:
                    OpenPosition = new Vector2(SpawnPos.X, SpawnPos.Y - 1);
                    break;
            }
        }

        public void ResetCloseTimer()
        {
            CloseTimer = mCloseDelayInSeconds;
        }
        public void OnFinishedOpen()
        {
            ResetCloseTimer();

            if (FinishedOpen == null) return;
            FinishedOpen(this, new EventArgs());
        }

        public void OnFinishedClose()
        {
            if (FinishedClose == null) return;
            FinishedClose(this, new EventArgs());
        }

        public void OnBeginOpen()
        {
            ResetCloseTimer();

            if (BeginOpen == null) return;
            BeginOpen(this, new EventArgs());
        }

        public void OnBeginClose()
        {
            if (BeginClose == null) return;
            BeginClose(this, new EventArgs());
        }
        public void StartOpenDoor()
        {
            IsOpening = true;
            IsClosing = false;
            StartAnimating = true;
            OnBeginOpen();
        }
        public void StartCloseDoor()
        {
            IsOpening = false;
            IsClosing = true;
            StartAnimating = true;
            CloseTimer = -1f;
            OnBeginClose();
        }
    }
}
