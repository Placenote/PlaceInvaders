using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DevicePosNs
{
    public class _DevicePosFloorFollower : MonoBehaviour
    {

        // Update is called once per frame
        void Update()
        {
            transform.position = _DevicePos.FloorPosition;
            transform.rotation = _DevicePos.FloorRotation;

        }
    }
}