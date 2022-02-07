using System;
using Artemis;
using Microsoft.Xna.Framework;
using Twengine.Components;
using Twengine.Datastructures;
using Twengine.Helper;
using Twengine.Managers;
using XNAHelper;

namespace raycaster.Managers
{
    public class UseManager
    {
        private Raycaster mRaycaster;
        private Transform mPlayerTransform;
        private Tilemap mTilemap;

        public event EventHandler<EventArgs> PlayerFinishedMap;

        public UseManager(Raycaster raycaster, Tilemap map, Entity player)
        {
            mRaycaster = raycaster;
            mTilemap = map;
            mPlayerTransform = player.GetComponent<Transform>();
        }

        public void PlayerIsUsing()
        {
            Point targetedWall = mRaycaster.TargetedWall;
            
            float dist = GetWallDist(targetedWall);
            
            if (dist > 2) return;

            int pushedWallId = mTilemap.GetCellDataByPosition(targetedWall);

            PlayerPushedWallWithId(pushedWallId, targetedWall);
        }

        private void PlayerPushedWallWithId(int pushedWallId, Point targetedWall)
        {
            if (pushedWallId == 9)
                OnFinishedMap();
        }

        private float GetWallDist(Point targetedWall)
        {
            Vector2 wallPos = targetedWall.ToVector2();
            wallPos.X += 0.5f;
            wallPos.Y += 0.5f;
            return Vector2.Distance(mPlayerTransform.Position, wallPos);
        }

        private void OnFinishedMap()
        {
            if (PlayerFinishedMap == null) return;
            PlayerFinishedMap(this, new EventArgs());
        }
    }
}
