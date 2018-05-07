using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARKitSupportNs
{
    public class DebugFloor : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

#if UNITY_IOS && !UNITY_EDITOR
        Destroy(gameObject); 
#endif
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
