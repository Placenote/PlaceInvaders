using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DevicePosNs
{
    public class _DevicePosFollower : MonoBehaviour
    {

        // Update is called once per frame
        void Update()
        {
            transform.position = _DevicePos.PlayerPosition;
            transform.rotation = _DevicePos.PlayerRotation;

        }
    }
}