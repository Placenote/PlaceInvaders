using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonNs
{
    public class RotateSkybox : MonoBehaviour
    {
        public float SkyboxSpeed = 1f;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {


            GetComponent<Skybox>().material.SetFloat("_Rotation", Time.time * SkyboxSpeed);
        }

    }
}
