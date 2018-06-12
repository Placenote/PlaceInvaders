using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Placenote;
using UnityEngine.UI;

namespace GameUiNs
{
    /// <summary>
    /// Photon and SDK initialization screens.
    /// Ensures app cannot be started until Photon is connected and SDK is initialized.
    /// </summary>
    public class InitializationScreens : PlacenotePunMultiplayerBehaviour
    {
        // Main Menu
        public GameObject MainMenuScreens;

        // Child object references
        public Text PhotonInitialization;
        public Text SDKInitialization;
        public GameObject ErrorMessage;
        public GameObject LoadingCircle;

        private bool PhotonReady;
        private bool SDKReady;

        protected override void Start ()
        {
            base.Start ();
            PhotonInitialization.text = "Online Status: Connecting";
            SDKInitialization.text = "SDK: Initializing";
            ErrorMessage.SetActive (false);
            LoadingCircle.SetActive (true);
            PhotonReady = false;
            SDKReady = false;
            StartCoroutine (CheckSDKFail ());
        }

        protected override void OnConnectedToPhoton ()
        {
            PhotonReady = true;
            PhotonInitialization.text = "Online Status: Connected";
        }

        protected override void OnFailedToConnectToPhoton ()
        {
            PhotonReady = false;
            PhotonInitialization.text = "Online Status: Failed";
            ErrorMessage.SetActive (true);
            LoadingCircle.SetActive (false);
        }

        private void Update ()
        {
            if (!SDKReady && LibPlacenote.Instance.Initialized ())
            {
                SDKReady = true;
                SDKInitialization.text = "SDK: Initialized";
            }
            if (PhotonReady && SDKReady)
            {
                MainMenuScreens.SetActive (true);
                gameObject.SetActive (false);
            }
                
        }

        // Check if SDK is initialized after 3 seconds
        IEnumerator CheckSDKFail ()
        {
            yield return new WaitForSeconds (3f);
            if (!LibPlacenote.Instance.Initialized ())
            {
                SDKReady = false;
                SDKInitialization.text = "SDK: Failed";
                ErrorMessage.SetActive (true);
                LoadingCircle.SetActive (false);
            }
        }
    }
}