using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARKitSupportNs
{
    public class ARSceneStarter : MonoBehaviour
    {
        public  bool DoOnce = true;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (DoOnce)
            {
                DoOnce = false;
                Debug.Log("Call GameController.WorldRootObject.ClearObjectsToDelete()");
                GameController.WorldRootObject.ClearObjectsToDelete();
            }

        }
    }
}
