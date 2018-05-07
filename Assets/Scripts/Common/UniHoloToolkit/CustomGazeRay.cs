// Based  on sources of Microsoft Mixed Reality Toolkit,  adopted in correspondence to  MIT License. 
// Copyright Microsoft Corporation. All rights reserved. Licensed under the MIT License.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CustomGazeRayNs
{
    public class CustomGazeRay : MonoBehaviour
    {
        #region PUBLIC_MEMBER_VARIABLES
        public List<BaseTapGazeTrigger> viewTriggers;
        #endregion // PUBLIC_MEMBER_VARIABLES


        static CustomGazeRay _instance;
        static CustomGazeRay Instance
        {
           get
            {

                if (_instance == null || _instance.gameObject == null)
                    _instance = FindObjectOfType<CustomGazeRay>();
                return _instance;
            }


        }


        public static void RegisterTrigger(BaseTapGazeTrigger trigger)
        {
            Instance.viewTriggers.Add(trigger);
        }

        public static void  UnRegisterTrigger(BaseTapGazeTrigger trigger)
        {
            if (Instance == null || Instance.viewTriggers == null || trigger == null)
                return;
            Instance.viewTriggers.Remove(trigger);
        }
        void Awake()
        {
            if (_instance != null)
            {
                Debug.LogError("Attempt to have 2 CustomGazeRay at scene, name = "+ gameObject.name);
                this.enabled = false;
                return;

            }
           
        }

        #region MONOBEHAVIOUR_METHODS
        void Update()
        {
            // Check if the Head gaze direction is intersecting any of the ViewTriggers
            RaycastHit hit;
            Ray cameraGaze = new Ray(this.transform.position, this.transform.forward);
            Physics.Raycast(cameraGaze, out hit, Mathf.Infinity);
            foreach (var trigger in viewTriggers)
            {
                trigger.Focused = hit.collider && (hit.collider.gameObject == trigger.gameObject);
            }
        }
        #endregion // MONOBEHAVIOUR_METHODS
    }


}
