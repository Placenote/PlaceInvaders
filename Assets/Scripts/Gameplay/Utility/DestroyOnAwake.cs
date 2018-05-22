//#define DESTROY_ANYWAY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonNs
{
    /// <summary>
    /// Some objects are useful at development stage - like prototypes for prefabs,
    /// but should be never shown when application is running.
    /// Sometime the programmer can forget to disable them or remove from scene..
    /// This script disables them and destroys at all to be at the safe side
    /// </summary>
    public class DestroyOnAwake : MonoBehaviour
    {
        
        public List<GameObject> ObjectsToDelete;
        private void Awake()
        {
            // first disable them, because the Destroy will be really executed during next frame
            DisableObjects();
            DestroyObjects();
        }
 

        void DisableObjects()
        {
          

            for (int i=0; i< ObjectsToDelete.Count; i++  )
            if(ObjectsToDelete[i] != null)
                    ObjectsToDelete[i].SetActive(false); 

        }


        void DestroyObjects()
        {
           

            for (int i = 0; i < ObjectsToDelete.Count; i++)
                if (ObjectsToDelete[i] != null)
                    Destroy(ObjectsToDelete[i]);

        }

    }
}
