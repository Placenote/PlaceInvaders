using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DevicePosNs
{

    public class _PlayerPosReader : MonoBehaviour
    {
        public bool DirectTrackARKitCamera;

        [Header("Track this object when device is not ready")]
        public Transform TrackedObject;
        [Header("Set position/rotation of this object to same as device")]
        public Transform ObjectToUpdate;
        
        // Use this for initialization
        void Start()
        {
            if (ObjectToUpdate == null)
                ObjectToUpdate = gameObject.transform;
            if (DirectTrackARKitCamera)
            {
                _DevicePos.Stop();
                var cam = GetARKitCamera();
                TrackedObject = (cam == null ? null : cam.transform);


            }

        }

        
        
        Camera GetARKitCamera()
        {
            Camera ret = null;
            UnityARCameraManager ARKitManager = FindObjectOfType<UnityARCameraManager>();
            if (ARKitManager != null)
                ret = ARKitManager.m_camera;
            return ret;
        }

        // Update is called once per frame
        virtual protected void Update()
        {
            if (_DevicePos.IsReady)
            {
                ObjectToUpdate.transform.position = _DevicePos.PlayerPosition;
                ObjectToUpdate.transform.rotation = _DevicePos.PlayerRotation;
            }
            else if (TrackedObject != null)
            {

                ObjectToUpdate.transform.position = TrackedObject.position;
                ObjectToUpdate.transform.rotation = TrackedObject.rotation;
            }
            else
            {

                Camera cam = GetARKitCamera();
                if (cam == null)
                    cam = Camera.main;
 
                if (cam != null)
                    TrackedObject = cam.transform;
            }


        }

    }
}