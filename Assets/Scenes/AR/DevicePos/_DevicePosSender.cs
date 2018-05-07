using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DevicePosNs
{
    public class _DevicePosSender : MonoBehaviour
    {
        [Header("Place and rotation of WorldRoot")]
        public Transform FloorCenter;
        [Header("Public just for debug,got from UnityARCameraManager")]
        public Transform Device;
        // Use this for initialization
        void Start()
        {
            
        }


     

        public void StartTracking()
        {
            
            var cameraManager = FindObjectOfType<UnityARCameraManager>();
            if (cameraManager != null && cameraManager.m_camera != null)
                Device = cameraManager.m_camera.transform;
            else
            {

                Debug.LogError("Camera  is not detected!!!");
               // Device = null;
            }

            if (FloorCenter != null)
            {
                _DevicePos.FloorPosition = FloorCenter.transform.position;
                _DevicePos.FloorRotation = FloorCenter.transform.rotation;
            }
            _DevicePos.SetReady();
     
            Debug.Log("---------   Device status ----: " + _DevicePos.IsReady);
        }

 

        // Update is called once per frame
        void Update()
        {
            if (_DevicePos.IsReady)
            {
                _DevicePos.PlayerPosition = Device.position;
                _DevicePos.PlayerRotation = Device.rotation;
            }

        }
#if false
        private void FixedUpdate()
        {
            if (_DevicePos.IsReady)
            {
                _DevicePos.PlayerPosition = Device.position;
                _DevicePos.PlayerRotation = Device.rotation;
            }
        }
#endif

    }
}
