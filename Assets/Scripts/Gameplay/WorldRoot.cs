using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _DevicePosNs;
using GameplayNs;
using CommonNs;

public class WorldRoot : MonoBehaviour
{
    public void SetupWorldRootCoordinates (Vector3 newPos, Quaternion newRot)
    {
        _DevicePos.FloorPosition = newPos;
        _DevicePos.FloorRotation = newRot;

        transform.position = newPos;
        transform.rotation = newRot;
    }

    void Start ()
    {
        gameObject.AddComponent<EventSystemChecker> ();
    }
}