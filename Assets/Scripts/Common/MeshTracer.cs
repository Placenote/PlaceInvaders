using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonNs
{
    /// <summary>
    /// turns on/of rendering of object when it enters/exits tagged collider
    /// 
    /// </summary>
    public class MeshTracer : MonoBehaviour
    {
        public string VisibilityAreaTag = "VisibilityArea";
        [Tooltip("Controlled game object")]
        public Transform ControlledGameObject;

        [Tooltip("All objects without parent will be recognized as debug version")]
        public bool DestroyDebugInstances;

        public float TimeOut = 2;

        MeshRenderer goRenderer;
        bool IsDebugInstance { get { return ControlledGameObject != null && ControlledGameObject.parent == null; } }

        // Use this for initialization
        void Start()
        {


            if (ControlledGameObject == null)
                ControlledGameObject = gameObject.transform;


            goRenderer = ControlledGameObject.GetComponent<MeshRenderer>();
            if (goRenderer != null)
                goRenderer.enabled = false;

            if (IsDebugInstance)
                Destroy(this);
            else
                StartCoroutine(SetReady());
        }

        // Update is called once per frame
        void Update()
        {

        }

        bool Ready = false;
        IEnumerator SetReady()
        {
            yield return new WaitForSeconds(TimeOut);
            Ready = true;
        }

        void OnTriggerEnter(Collider other)
        {

            if (Ready && goRenderer != null && other.CompareTag(VisibilityAreaTag))
                goRenderer.enabled = true;
            //   if(go.parent != null && go.parent.parent != null)
            //  Debug.Log("Activated "+ go.parent.parent.parent.gameObject.name);
        }

        private void OnTriggerStay(Collider other)
        {
            if (Ready && goRenderer != null && !goRenderer.enabled)
                goRenderer.enabled = true;

        }


        void OnTriggerExit(Collider other)
        {

            if (goRenderer != null && other.CompareTag(VisibilityAreaTag))
                goRenderer.enabled = false;
            //  Debug.Log("DeActivated " + go.parent.parent.parent.gameObject.name);

        }

    }
}