using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.iOS;
using _DevicePosNs;

namespace ARKitSupportNs
{
    public class TapsTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
      //  ARHitTest hitTestCube;
        public _DevicePosSender devicePosSender;
        void Start()
        {
         //   hitTestCube = FindObjectOfType<ARHitTest>();
            devicePosSender = FindObjectOfType<_DevicePosSender>();
            //        hitTestCube.IgnoreTaps = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
           // hitTestCube.IgnoreTaps = false;
            if (devicePosSender != null)
                devicePosSender.StartTracking();


            Debug.Log("OnPointerDown");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
         //   hitTestCube.IgnoreTaps = true;
            if (devicePosSender != null)
                devicePosSender.StartTracking();
            Debug.Log("OnPointerUp");
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}
