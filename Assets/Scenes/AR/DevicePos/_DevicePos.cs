using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _DevicePosNs
{

    public static class _DevicePos
    {
        public static bool IsReady { get; private set; }
        public static void SetReady() { IsReady = true; }
        public static void Stop() { IsReady = false; }

        public static Vector3 PlayerPosition;
        public static Quaternion PlayerRotation;

        public static Vector3 FloorPosition;
        public static Quaternion FloorRotation;


        /// <summary>
        /// main light intencity
        /// </summary>

        static _DevicePos()
        {
            ResetToDefault();
        }

       
        public static void ResetToDefault()
        {
            IsReady = false;
            FloorPosition = Vector3.zero;
            PlayerPosition = FloorPosition + 1.8f * Vector3.up - Vector3.forward;
            PlayerRotation = FloorRotation = Quaternion.identity;

        }

    }


}
