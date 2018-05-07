using UnityEngine;
using System.Collections;

public class ShowcaseMovement : MonoBehaviour {
    
    

	void Update ()
    {
        transform.position += Vector3.forward * Input.GetAxis("Horizontal") * Time.deltaTime;
	}
}
