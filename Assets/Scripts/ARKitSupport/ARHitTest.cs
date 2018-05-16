using GameplayNs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace ARKitSupportNs
{
    public class ARHitTest : MonoBehaviour
    {
        /// <summary>
        /// hardcoded by Apple layer id for generated planes
        /// </summary>
        public const int planeLayerId = 10;
        public bool PlaceNoteOrientation = true;

        public Transform m_HitTransform;
        public GameObject Visualization;
        public float maxRayDistance = 30.0f;
        public LayerMask collisionLayer = 1 << planeLayerId;  //ARKitPlane layer

        bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
            if (hitResults.Count > 0)
            {
                foreach (var hitResult in hitResults)
                {
                    Debug.Log("Got hit!");
                    m_HitTransform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                    m_HitTransform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
                    return true;
                }
            }
            return false;
        }

        HitCubeParent hitCubeParent;
        private void Start()
        {
            IgnoreTaps = PlaceNoteOrientation;
            hitCubeParent = m_HitTransform.GetComponent<HitCubeParent>();
            hitCubeParent.AssignHitTest(this);
        }

        Collider _thisCollider;
        Collider ThisCollider
        {
            get
            {
                if (_thisCollider == null)
                    _thisCollider = GetComponent<Collider>();
                return _thisCollider;
            }
        }


        private void OnEnable()
        {
            IgnoreTaps = false;
        }

        private void OnDisable()
        {
            IgnoreTaps = true;
        }
        //public  bool _IgnoreTaps = false;
        private bool _IgnoreTaps = false;
        public bool IgnoreTaps
        {
            get { return _IgnoreTaps || PlaceNoteOrientation; }
            set
            {
                _IgnoreTaps = value || PlaceNoteOrientation;
                if (Visualization != null)
                    Visualization.SetActive(!_IgnoreTaps || PlaceNoteOrientation);
                if (ThisCollider != null)
                    ThisCollider.enabled = (!_IgnoreTaps);
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (IgnoreTaps)
                return;
#if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                //we'll try to hit one of the plane collider gameobjects that were generated by the plugin
                //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
                if (Physics.Raycast(ray, out hit, maxRayDistance, collisionLayer))
                {
                    //we're going to get the position from the contact point
                    m_HitTransform.position = hit.point;
                    Debug.Log(string.Format("pos: x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

                    //and the rotation from the transform of the plane collider
                    m_HitTransform.rotation = hit.transform.rotation;
                    Vector3 euler = m_HitTransform.rotation.eulerAngles;
                    //Vector3 euler = _HitTransform.rotation.
                    Debug.Log(string.Format("rot: x:{0:0.######} y:{1:0.######} z:{2:0.######}", euler.x, euler.y, euler.z));

                    hitCubeParent.OnHitTestDone();
                }
            }
#else
			if (Input.touchCount > 0 && m_HitTransform != null)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
				{
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent,
                        // if you want to use infinite planes use this:
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        ARHitTestResultType.ARHitTestResultTypeHorizontalPlane,
                        ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    };

                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                             hitCubeParent.OnHitTestDone();
                            return;
                        }
                    }
				}
			}
#endif

        }


    }
}
