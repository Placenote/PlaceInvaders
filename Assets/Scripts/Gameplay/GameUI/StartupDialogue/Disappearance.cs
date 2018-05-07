using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUiNs
{
    public class Disappearance : MonoBehaviour
    {
        public float TimeOut = 3;
        public GameObject Target;

        // Use this for initialization
        IEnumerator Start()
        {
            if (Target == null)
                Target = gameObject;
            yield return new WaitForSeconds(TimeOut);
            Target.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
