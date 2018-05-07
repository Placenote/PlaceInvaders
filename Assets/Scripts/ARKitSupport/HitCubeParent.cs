using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ARKitSupportNs
{
    public class HitCubeParent : MonoBehaviour
    {
            ARHitTest hitTest;
        // Use this for initialization
        void Start()
        {
        }


        public void AssignHitTest(ARHitTest newHitTest)
        {
            hitTest = newHitTest;
        }

        public void OnHitTestDone()
        {
            GameController.SetupWorldRootCoordinates
                (
                    hitTest.transform.position,
                    hitTest.transform.rotation
                );
        }

        /// <summary>
        /// to be called external events listener
        /// </summary>
        public void OnGameStart()
        {
            if (hitTest != null)
            {
                hitTest.gameObject.SetActive(false);
               
            }
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
