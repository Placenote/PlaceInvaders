using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplashScreen
{
    public class Autorotating : MonoBehaviour
    {
        public enum DirectionId
        {
            up,
            forward
        }
        public float speed = 10;

        public DirectionId AroundAxe = DirectionId.up;
        // Use this for initialization
        void Start()
        {


        }

        // Update is called once per frame
        void Update()
        {
            switch (AroundAxe)
            {
                case DirectionId.up:
                    transform.Rotate(transform.up, speed * Time.deltaTime);
                    break;
                case DirectionId.forward:
                    transform.Rotate(transform.forward, speed * Time.deltaTime);
                    break;
            }

        }


    }
}