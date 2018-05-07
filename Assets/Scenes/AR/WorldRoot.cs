using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _DevicePosNs;
using CommonNs;
using ARKitSupportNs;
using GameplayNs;

public class WorldRoot : MonoBehaviour
{
    public List<GameObject> ObjectsToDelete;

    

    public  void SetupWorldRootCoordinates(Vector3 newPos, Quaternion newRot)
    {
        _DevicePos.FloorPosition = newPos;
        _DevicePos.FloorRotation = newRot;

        transform.position = newPos;
        transform.rotation = newRot;
        GameController.Data.NotifyWorldRootPlaced();

    }

    // Use this for initialization

    void Start () {
        gameObject.AddComponent<EventSystemChecker>();
	}


    public void ClearObjectsToDelete()
    {
        for (int i = 0; i < ObjectsToDelete.Count; i++)
        {
            if (ObjectsToDelete[i] != null)
                Destroy(ObjectsToDelete[i]);
        }

        if(_DevicePos.IsReady)
        {
            transform.position = _DevicePos.FloorPosition;
            transform.rotation = _DevicePos.FloorRotation;
        }
    }

}

/*
// Update is called once per frame
    bool doOnce = true;
        public Transform HitFlag;
        public float FlagMovingSensitivity = 0.5f;



void Update ()
{
    if (doOnce)
    {
        doOnce = false;

        if (_DevicePos.IsReady)
        {
            transform.position = _DevicePos.FloorPosition;
            transform.rotation = _DevicePos.FloorRotation;
        }

        UnityARCameraManager ARKitManager = FindObjectOfType<UnityARCameraManager>();
        if (ARKitManager != null)
        {
            if (HitFlag == null)
            {
                var HitFlagComponent = FindObjectOfType<ARHitTest>();
                if (HitFlagComponent != null)
                    HitFlag = HitFlagComponent.m_HitTransform;
            }
            Clear();
        }

    }

    if (HitFlag != null && Vector3.Distance(HitFlag.position, transform.position) > FlagMovingSensitivity)
    {
        transform.position = HitFlag.position;
        transform.rotation = HitFlag.rotation;
    }
}
*/



