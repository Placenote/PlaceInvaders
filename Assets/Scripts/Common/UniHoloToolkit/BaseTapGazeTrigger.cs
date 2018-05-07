// Based  on sources of Microsoft Mixed Reality Toolkit,  adopted in correspondence to  MIT License. 
// Copyright Microsoft Corporation. All rights reserved. Licensed under the MIT License.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace CustomGazeRayNs
{
    public class BaseTapGazeTrigger : MonoBehaviour, IPointerClickHandler
    {

        #region PUBLIC_MEMBER_VARIABLES

        public float activationTime = 1.5f;
        public float transitionDuration = 3;
        public bool Focused { get; set; }
        #endregion // PUBLIC_MEMBER_VARIABLES


        #region PRIVATE_MEMBER_VARIABLES
        private float mFocusedTime = 0;
        private bool mTriggered = false;
        bool DoRegister = true;

        #endregion // PRIVATE_MEMBER_VARIABLES



        #region MONOBEHAVIOUR_METHODS
        void Start()
        {

            mTriggered = false;
            mFocusedTime = 0;
            Focused = false;

        }

        void OnEnable()
        {
            DoRegister = true;
        }

        void OnDisable()
        {
            CustomGazeRay.UnRegisterTrigger(this);
        }

        void Update()
        {
            if (DoRegister)
            {
                DoRegister = false;
                CustomGazeRay.RegisterTrigger(this);
            }

            if (mTriggered)
                return;

            //  UpdateOnFocused(Focused);

            bool startAction = false;
            if (Input.GetMouseButtonUp(0))
            {
                startAction = true;
            }

            if (Focused)
            {
                UpdateOnFocused(true);
                // Update the "focused state" time
                mFocusedTime += Time.deltaTime;
                if ((mFocusedTime > activationTime) || startAction)
                {
                    mTriggered = true;
                    mFocusedTime = 0;
                    OnClick();


                    StartCoroutine(ResetAfter(transitionDuration));
                }
            }
            else
            {
                // Reset the "focused state" time
                mFocusedTime = 0;
            }
        }
        #endregion // MONOBEHAVIOUR_METHODS


        public virtual void UpdateOnFocused(bool focused)
        {
            Debug.Log("On focused " + focused);
        }



        private IEnumerator ResetAfter(float seconds)
        {
            Debug.Log("Resetting View trigger after: " + seconds);

            yield return new WaitForSeconds(seconds);

            Debug.Log("Resetting View trigger: " + this.gameObject.name);

            // Reset variables
            mTriggered = false;
            mFocusedTime = 0;
            Focused = false;
            UpdateOnFocused(false);
        }


        public virtual void OnClick()
        {
            Debug.Log("Pointer click////");
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnClick();
        }
    }

}