using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EnemiesNs
{

    class DemoMover
    {
        public Vector3 InitialPos;
        public Vector3 Offset;

        Vector3 CurPos;

        Vector3 CurTargetPos;
        public float CurTime;
        public float StartTime;

        Vector3 Velocity;
        public float DumpRate = 5;
        int MovingState = 1;

        public void Move(float delta)
        {
            CurPos = Vector3.SmoothDamp(CurPos, CurTargetPos, ref Velocity, DumpRate);
            if (CurTime < 0)
                CurTime = StartTime;

        }

        void Reverce()
        {
            MovingState = -MovingState;
            CurPos = (MovingState > 0 ? InitialPos + Offset : InitialPos);
        }
        public void Reset()
        {
            CurTime = StartTime;
        }
    }
}