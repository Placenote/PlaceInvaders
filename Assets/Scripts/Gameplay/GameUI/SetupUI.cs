using System.Collections.Generic;
using System.Collections;
using System.Xml;
using GameplayNs;
using Placenote;
using UnityEngine;
using UnityEngine.UI;
using PunServerNs;

namespace GameUiNs
{

    public class SetupUI : MonoBehaviour
    {
		public SrvController Srv;
		public Text StatusText; // TODO figure out what to do with this

		// Don't need.
        public Button ServerBtn;
		public Button StartEnvironmentScaningBtn;
		public GameObject ServerPanel;
		// End

		#region Main UI elements

		// Stack to keep track of current UI layer
		private Stack UIStack = new Stack();

		// MainMenu.
		public GameObject MainMenuPanel;
		public Button JoinGameBtn;
		public Button HostGameBtn;
		// TODO ADD SINGLEPLAYER

		//Back Button.
		public Button BackBtn;

		// Host a room panel.
		public GameObject CreateHostRoomPanel;
		public InputField HostingRoomNameInput;
		public Text HostingRoomName;
		public Button HostRoomBtn;

		// Inside hosted room
		public GameObject HostingRoomPanel;
		public Button StartGameBtn;
		public Text HostingRoomPlayers;

		// View all available rooms to join.
		public GameObject ViewRoomsPanel;
		public GameObject RoomButtonsParent;		// Need parent to instaniate room buttons in correct location
		[SerializeField] GameObject roomButtonPrefab;
		private RoomInfo[] roomsArray;

		// Inside a joined room.
		public GameObject JoinedRoomPanel;
		public Text JoinedRoomName;

		#endregion

		// Placenote things
        public GameObject PlacenoteShowPoints;
        public FeaturesVisualizer FeaturesVisualizer;
        public GameObject flag;

        private bool isScanning = false;
        private bool isLocalized = false;

        private void Start()
        {
			if (Srv == null)
				Srv = FindObjectOfType<SrvController>();
			
			BackBtn.onClick.AddListener (GoBack);

			// Hosting path
			HostGameBtn.onClick.AddListener (ActivateHostRoomUI);
			HostRoomBtn.onClick.AddListener (ActivateHostingRoomUI);
			StartGameBtn.onClick.AddListener(OnStartGameClick);

			// Joining path
			JoinGameBtn.onClick.AddListener (ActivateViewRoomsUI);
		
			// Adding MainMenu to UIStack
			UIStack.Push (MainMenuPanel);
        }

        #region > Buttons On Click Events
        
        #region >> Start Game
        
        public void OnStartGameClick() // TODO UPDATE
        {
            if (string.IsNullOrEmpty(EnvironmentScannerController.Instance.LatestMapId)) {
                StatusText.text = "Error! Can't find Environment Scan!";
                return;
            }

            var res = EnvironmentScannerController.Instance.LoadLatestMap(LatestMapLoaded, MapLoadingFail, MapLoadingPercentage);

            if (res) {
                StartGameBtn.gameObject.SetActive(false);
                ServerBtn.gameObject.SetActive(false);
                StartEnvironmentScaningBtn.gameObject.SetActive(false);
            }
        }

        private void LatestMapLoaded(string mapId) // TODO UPDATE
        {
            StatusText.text = "Map Loaded Succesfully!";
            EnvironmentScannerController.Instance.OnPlacenoteStatusChange.AddListener(PlacenoteStatusChange);
        }
        
        private void MapLoadingFail(string mapId) // TODO UPDATE
        {
            StatusText.text = "Map Loading Fail!";
            StartGameBtn.gameObject.SetActive(true);
            ServerBtn.gameObject.SetActive(true);
            StartEnvironmentScaningBtn.gameObject.SetActive(true);
        }

        private void MapLoadingPercentage(float percentage) //TODO UPDATE
        {
            StatusText.text = "Loading Map..." + (percentage * 100f) + "%";
        }
        
        #endregion
        
		private void ActivateHostRoomUI()
		{
			if (CreateHostRoomPanel != null)
				ActivateUI(CreateHostRoomPanel);

		}

		private void ActivateHostingRoomUI()
		{
			if (HostingRoomPanel != null) {
				if (string.IsNullOrEmpty(HostingRoomNameInput.text))
					HostingRoomName.text = "Untitled";
				else
					HostingRoomName.text = HostingRoomNameInput.text;
				DoConnect();
				Srv.HostRoom(HostingRoomName.text);
				ActivateUI (HostingRoomPanel);
			}
		}

		private void ActivateViewRoomsUI()
		{
			Srv.Connect ("Player-2343");
			if (ViewRoomsPanel != null)
				ActivateUI (ViewRoomsPanel);
		}
			

		private void ActivateJoinedRoomUI()
		{
			if (JoinedRoomPanel != null)
				ActivateUI (JoinedRoomPanel);
			
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

			// If in view rooms panel delete all room buttons
			if (currentUI == ViewRoomsPanel) {
				DeleteViewRooms ();
				Srv.Disconnect ();
			}
			// Disconnect from photon if...
			// TODO add confirmation from leaving host room
			if (currentUI == HostingRoomPanel) {
				Debug.Log ("PUT CONFIRMATION MESSAGE HERE");
				Srv.Disconnect ();
			}

		}

		private void ActivateUI(GameObject UIToActivate)
		{
			// Show back button if last state was menu
			if (UIStack.Peek () == MainMenuPanel)
				BackBtn.gameObject.SetActive (true);
			// Hides currentUI
			GameObject currentUI = (GameObject) UIStack.Peek ();
			currentUI.SetActive (false);
			UIStack.Push (UIToActivate);
			UIToActivate.SetActive (!UIToActivate.activeSelf);

		}

        #region >> Environment Scanning
        
        private void EnvironmentScanningClick() //TODO Update
        {
            if (isScanning) {
                EnvironmentScannerController.Instance.FinishScanning(EnvironmentScanningFinish, EnvironmentScanningProgress);
                StartEnvironmentScaningBtn.gameObject.SetActive(false);
                isScanning = false;
            } else {
                
                var startScan = EnvironmentScannerController.Instance.StartScanning();
                if (startScan) {
                    FeaturesVisualizer.EnablePointcloud();
                    FlagController.Instance.InstantiateAfterDelay();
                    ServerBtn.gameObject.SetActive(false);
                    StartGameBtn.gameObject.SetActive(false);
                    isScanning = startScan;
                }
            }
        }

        private void EnvironmentScanningProgress(float progress) // TODO Update
        {
            StatusText.text = "Saving Progress..." + (progress * 100f) + "%";
        }
        
        private void EnvironmentScanningFinish(bool scanningRes) // TODO Update
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
        private void ActivateEnvironmentScanningBtnRPC() // TODO Update
        {
            StartEnvironmentScaningBtn.gameObject.SetActive(true);
        }
        
        #endregion
        
        #endregion

		#region Dynamic UI generation

		public void GenerateViewRooms()
		{
			int counter = 0;
			roomsArray = Srv.GetRooms ();
			foreach (RoomInfo game in roomsArray) {
				GameObject room = (GameObject)Instantiate (roomButtonPrefab);
				int roomIndex = counter;
				room.GetComponent<Button> ().onClick.AddListener (
					() => {JoinRoom (roomIndex);}
				);
				room.GetComponent<Button> ().onClick.AddListener (ActivateJoinedRoomUI);

				room.transform.GetChild(0).GetComponent<Text> ().text = game.name;
				room.transform.GetChild(1).GetComponent<Text> ().text = game.PlayerCount + "/"+ game.MaxPlayers;
				room.transform.parent = RoomButtonsParent.transform;
				counter++;
			}
		}

		void DeleteViewRooms()
		{
			foreach (Transform child in RoomButtonsParent.transform) {
				GameObject.Destroy (child.gameObject);
			}
		}

		public void UpdatePlayerAmounts(string players, string maxPlayers)
		{
			HostingRoomPlayers.text = players + "/" + maxPlayers;
		}
		#endregion
        
		#region Networking

		void DoConnect()
		{
			float pseudoUID = Time.realtimeSinceStartup;
			pseudoUID = pseudoUID - Mathf.FloorToInt(pseudoUID)*1000;
			string playerName = "Player" + Mathf.FloorToInt(pseudoUID);
			Srv.Connect(playerName);
		}

		void JoinRoom(int roomIndex)
		{
			JoinedRoomName.text = roomsArray[roomIndex].name;
			Srv.JoinRoom (roomsArray[roomIndex].name);
		}

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
