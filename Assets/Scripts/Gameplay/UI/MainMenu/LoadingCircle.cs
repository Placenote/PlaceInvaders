using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    private RectTransform rectComponent;
    private float rotateSpeed = 400f;

    private void Start ()
    {
        rectComponent = GetComponent<RectTransform> ();
    }

    /// <summary>
    /// Rotate the grey part of the circle based on rotateSpeed
    /// </summary>
    private void Update ()
    {
        rectComponent.Rotate (0f, 0f, -rotateSpeed * Time.deltaTime);
    }
}