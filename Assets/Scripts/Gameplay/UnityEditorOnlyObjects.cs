//#define DESTROY_ANYWAY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonNs
{
    public class UnityEditorOnlyObjects : MonoBehaviour
    {
        
        public List<GameObject> ObjectsToDelete;
        private void Awake()
        {
           DisableIfNotEditor();
        }

        void Start()
        {
            DestroyIfNotEditor();
        }

        void DisableIfNotEditor()
        {
          
#if DESTROY_ANYWAY || !UNITY_EDITOR
            for (int i=0; i< ObjectsToDelete.Count; i++  )
            if(ObjectsToDelete[i] != null)
                    ObjectsToDelete[i].SetActive(false); 
#endif
        }

        void DestroyIfNotEditor()
        {
           

#if DESTROY_ANYWAY || !UNITY_EDITOR
            for (int i = 0; i < ObjectsToDelete.Count; i++)
                if (ObjectsToDelete[i] != null)
                    Destroy(ObjectsToDelete[i]);
#endif
        }

        // Use this for initialization


        // Update is called once per frame
        void Update()
        {

        }
    }
}
