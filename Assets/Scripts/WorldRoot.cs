using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _DevicePosNs;
using GameplayNs;
using CommonNs;

public class WorldRoot : MonoBehaviour
{
    public List<GameObject> ObjectsToDelete;

    public void SetupWorldRootCoordinates (Vector3 newPos, Quaternion newRot)
    {
        _DevicePos.FloorPosition = newPos;
        _DevicePos.FloorRotation = newRot;

        transform.position = newPos;
        transform.rotation = newRot;
        GameController.Data.NotifyWorldRootPlaced ();
    }

    void Start ()
    {
        gameObject.AddComponent<EventSystemChecker> ();
    }
}