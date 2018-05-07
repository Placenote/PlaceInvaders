// Based  on sources of Microsoft Mixed Reality Toolkit,  adopted in correspondence to  MIT License. 
// Copyright Microsoft Corporation. All rights reserved. Licensed under the MIT License.

using UnityEngine;

namespace UniHoloToolkit
{
    public enum PivotAxis
    {
        // Rotate about all axes.
        Free,
        // Rotate about an individual axis.
        X,
        Y
    }

    /// <summary>
    /// The Billboard class implements the behaviors needed to keep a GameObject
    /// oriented towards the user.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        public Transform _target;
        /// <summary>
        /// Player camera or his head etc
        /// </summary>
        public Transform Target
        {

            get
            {
                if (_target == null && Camera.main != null && Camera.main.gameObject != null)
                    _target = Camera.main.gameObject.transform;
                return _target;
            }
        }


        public void AssignTarget(Transform newTarget)
        {
            _target = newTarget;
        }

        /// <summary>
        /// The axis about which the object will rotate.
        /// </summary>
        [Tooltip("Specifies the axis about which the object will rotate (Free rotates about both X and Y).")]
        public PivotAxis PivotAxis = PivotAxis.Free;

        /// <summary>
        /// Overrides the cached value of the GameObject's default rotation.
        /// </summary>
        public Quaternion DefaultRotation { get; private set; }

        private void Awake()
        {
            // Cache the GameObject's default rotation.
            DefaultRotation = gameObject.transform.rotation;
        }

        /// <summary>
        /// Keeps the object facing to player.
        /// </summary>
        private void Update()
        {
            if (Target == null)
                return;
            // Get a Vector that points from the player to the target of his look.
            Vector3 forward;
            Vector3 up;

            // Adjust for the pivot axis. We need a forward and an up for use with Quaternion.LookRotation
            switch (PivotAxis)
            {
                // If we're fixing one axis, then we're projecting the Target's forward vector onto
                // the plane defined by the fixed axis and using that as the new forward.
                case PivotAxis.X:
                    Vector3 right = transform.right; // Fixed right
                    forward = Vector3.ProjectOnPlane(Target.forward, right).normalized;
                    up = Vector3.Cross(forward, right); // Compute the up vector
                    break;

                case PivotAxis.Y:
                    up = transform.up; // Fixed up
                    forward = Vector3.ProjectOnPlane(Target.forward, up).normalized;
                    break;

                // If the axes are free then we're simply aligning the forward and up vectors
                // of the object with those of the Target. 
                case PivotAxis.Free:
                default:
                    forward = Target.forward;
                    up = Target.up;
                    break;
            }


            // Calculate and apply the rotation required to reorient the object
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
}