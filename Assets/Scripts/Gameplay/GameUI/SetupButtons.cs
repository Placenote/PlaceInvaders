using System.Collections.Generic;
using System.Collections;
using System.Xml;
using GameplayNs;
using Placenote;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{

    public class SetupButtons : MonoBehaviour
    {
        //TODO fix order of these variables
		public Text StatusText;

		// Don't need
        public Button ServerBtn;
		public Button StartEnvironmentScaningBtn;
		public GameObject ServerPanel;
		// End

		// Main UI elements
		public GameObject MainMenuPanel;
		public Button BackBtn;

		public Button JoinGameBtn;
		public GameObject ViewRoomsPanel;

		public Button JoinRoomBtn;
		public GameObject JoinedRoomPanel;

		public Button HostGameBtn;
		public GameObject CreateHostRoomPanel;

		public Button HostRoomBtn;
		public GameObject HostingRoomPanel;

		// Secondary UI elements
		public InputField HostingRoomName;
		public Text HostingRoomText;
		public Button StartGameBtn;


        public GameObject PlacenoteShowPoints;
        public FeaturesVisualizer FeaturesVisualizer;
        public GameObject flag;

        private bool isScanning = false;
        private bool isLocalized = false;
		private Stack UIStack = new Stack();

        private void Start()
        {
			BackBtn.onClick.AddListener (GoBack);
			JoinGameBtn.onClick.AddListener (ToggleJoinGameUI);
			HostGameBtn.onClick.AddListener (ToggleHostRoomUI);
			JoinRoomBtn.onClick.AddListener (ToggleJoinedRoomUI);
			HostRoomBtn.onClick.AddListener (ToggleHostingRoomUI);

			StartGameBtn.onClick.AddListener(OnStartGameClick);
			// Adding MainMenu to UIStack
			UIStack.Push (MainMenuPanel);
        }

        #region > Buttons On Click Events
        
        #region >> Start Game
        
        public void OnStartGameClick()
        {
            if (string.IsNullOrEmpty(EnvironmentScannerController.Instance.LatestMapId))
            {
                StatusText.text = "Error! Can't find Environment Scan!";
                return;
            }

            var res = EnvironmentScannerController.Instance.LoadLatestMap(LatestMapLoaded, MapLoadingFail, MapLoadingPercentage);

            if (res)
            {
                StartGameBtn.gameObject.SetActive(false);
                ServerBtn.gameObject.SetActive(false);
                StartEnvironmentScaningBtn.gameObject.SetActive(false);
            }
        }

        private void LatestMapLoaded(string mapId)
        {
            StatusText.text = "Map Loaded Succesfully!";
            EnvironmentScannerController.Instance.OnPlacenoteStatusChange.AddListener(PlacenoteStatusChange);
        }
        
        private void MapLoadingFail(string mapId)
        {
            StatusText.text = "Map Loading Fail!";
            StartGameBtn.gameObject.SetActive(true);
            ServerBtn.gameObject.SetActive(true);
            StartEnvironmentScaningBtn.gameObject.SetActive(true);
        }

        private void MapLoadingPercentage(float percentage)
        {
            StatusText.text = "Loading Map..." + (percentage * 100f) + "%";
        }
        
        #endregion
        
		private void ToggleJoinGameUI()
		{
			Debug.Log ("Room info start...");
			Debug.Log (PhotonNetwork.GetRoomList ());
			foreach (RoomInfo game in PhotonNetwork.GetRoomList()) {
				Debug.Log (game.name);
				Debug.Log (game.PlayerCount);
				Debug.Log (game.MaxPlayers);
			}
			Debug.Log ("Room info end...");
			if (ViewRoomsPanel != null)
			{
				ToggleUI (ViewRoomsPanel);
			}
		}

		private void ToggleHostRoomUI()
		{
			if (CreateHostRoomPanel != null)
			{
				ToggleUI (CreateHostRoomPanel);
			}
		}

		private void ToggleHostingRoomUI()
		{
			if (HostingRoomPanel != null)
			{
				if (string.IsNullOrEmpty(HostingRoomName.text))
					HostingRoomText.text = "Untitled";
				else
					HostingRoomText.text = HostingRoomName.text;
				ToggleUI (HostingRoomPanel);
			}
		}

		private void ToggleJoinedRoomUI()
		{
			if (JoinedRoomPanel != null)
			{
				ToggleUI (JoinedRoomPanel);
			}
		}

		private void GoBack()
		{
			GameObject currentUI = (GameObject) UIStack.Pop ();
			currentUI.SetActive (false);

			GameObject prevUI = (GameObject) UIStack.Peek ();
			prevUI.SetActive (true);

			// Hide back button if at menu
			if (UIStack.Peek () == MainMenuPanel)
				BackBtn.gameObject.SetActive (false);
		}

		private void ToggleUI(GameObject UIToToggle)
		{
			// Show back button if last state was menu
			if (UIStack.Peek () == MainMenuPanel)
				BackBtn.gameObject.SetActive (true);
			// Hides currentUI
			GameObject currentUI = (GameObject) UIStack.Peek ();
			currentUI.SetActive (false);
			UIStack.Push (UIToToggle);
			UIToToggle.SetActive (!UIToToggle.activeSelf);
			
		}

        #region >> Environment Scanning
        
        private void EnvironmentScanningClick()
        {
            if (isScanning)
            {
                EnvironmentScannerController.Instance.FinishScanning(EnvironmentScanningFinish, EnvironmentScanningProgress);
                StartEnvironmentScaningBtn.gameObject.SetActive(false);
                isScanning = false;
            }
            else
            {
                
                var startScan = EnvironmentScannerController.Instance.StartScanning();
                if (startScan)
                {
                    FeaturesVisualizer.EnablePointcloud();
                    FlagController.Instance.InstantiateAfterDelay();
                    ServerBtn.gameObject.SetActive(false);
                    StartGameBtn.gameObject.SetActive(false);
                    isScanning = startScan;
                }
            }
        }

        private void EnvironmentScanningProgress(float progress)
        {
            StatusText.text = "Saving Progress..." + (progress * 100f) + "%";
        }
        
        private void EnvironmentScanningFinish(bool scanningRes)
        {
            StatusText.text = "Saving " + (scanningRes ? "success!" : "Error!");
            ServerBtn.gameObject.SetActive(true);
            StartGameBtn.gameObject.SetActive(true);
            
            if(PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                ActivateEnvironmentScanningBtnRPC();
            else
                GetComponent<PhotonView>().RPC("ActivateEnvironmentScanningBtnRPC", PhotonTargets.All);
            
            FeaturesVisualizer.DisablePointcloud();
			FeaturesVisualizer.clearPointcloud();
            //StartEnvironmentScaningBtn.gameObject.SetActive(true);
        }

        [PunRPC]
        private void ActivateEnvironmentScanningBtnRPC()
        {
            StartEnvironmentScaningBtn.gameObject.SetActive(true);
        }
        
        #endregion
        
        #endregion

        
        public void PlacenoteStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
        {
            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST) 
            {
                StatusText.text = "Localized! Game Starts Now!";
                FlagController.Instance.InstantiateFlag();

                
                GameController.StartGame();
                PlacenoteShowPoints.SetActive(false);
                EnvironmentScannerController.Instance.OnPlacenoteStatusChange.RemoveListener(PlacenoteStatusChange);
            } 
            else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING) 
            {
                StatusText.text = "Mapping";
            } 
            else if (currStatus == LibPlacenote.MappingStatus.LOST)
            {
                StatusText.text = "Searching for position lock";
            } 
            else if (currStatus == LibPlacenote.MappingStatus.WAITING) 
            {
            }
        }
    }
}
