using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonNs
{
    public class LeftRightRotator : MonoBehaviour
    {
        public List<Light> lights;
        public Transform CenterOfLook;

        public float AngleLimit = 90;
        public int direction;


        public float speed;

        public float AngleAtStart;
        // Use this for initialization
        private void Start()
        {
            transform.LookAt(CenterOfLook);
            AngleAtStart = Vector3.Angle(CenterOfLook.position - transform.position, transform.forward);
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(transform.up, direction * speed * Time.deltaTime);
            if (Mathf.Abs(AngleAtStart - Vector3.Angle(CenterOfLook.position - transform.position, transform.forward)) > AngleLimit)
            {
                direction = -direction;
            }

        }
    }
}
