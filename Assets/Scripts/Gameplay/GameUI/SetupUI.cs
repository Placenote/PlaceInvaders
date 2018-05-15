using System.Collections.Generic;
using System.Collections;
using System.Xml;
using GameplayNs;
using Placenote;
using UnityEngine;
using UnityEngine.UI;
using PunServerNs;
using TargetNs;

namespace GameUiNs
{

    public class SetupUI : MonoBehaviour
    {
		public SrvController Srv;
		public Text StatusText; // TODO figure out what to do with this

		#region Main UI Elements

		// Stack to keep track of current UI layer
		private Stack UIStack = new Stack();

		// MainMenu.
		public GameObject MainMenuPanel;
		public Button JoinGameBtn;
		public Button HostGameBtn;
		public Button SinglePlayerBtn;
		// TODO ADD SINGLEPLAYER

		// Back Button.
		public Button BackBtn;

		// Loading Circle
		public GameObject LoadingCircle;

		// Host a room panel.
		public GameObject CreateHostRoomPanel;
		public InputField HostingRoomNameInput;
		public Text HostingRoomName;
		public Button HostRoomBtn;

		// Inside hosted room
		public GameObject HostingRoomPanel;
		public Button StartGameBtn;
		public Text HostingRoomPlayers;
		public GameObject LeavingHostingRoomConfirmationPanel;
		public Button LeaveBtn;
		public Button StayBtn;
		public GameObject FailToCreateRoomPanel;
		public Button ConfirmBtn;

		// View all available rooms to join.
		public GameObject ViewRoomsPanel;
		public GameObject RoomButtonsParent;		// Need parent to instaniate room buttons in correct location
		[SerializeField] GameObject roomButtonPrefab;
		private RoomInfo[] roomsArray;

		// Inside a joined room.
		public GameObject JoinedRoomPanel;
		public Text JoinedRoomName;
		public Text JoinedRoomPlayers;

		#endregion Main UI Elements

		// Placenote things
        public GameObject PlacenoteShowPoints;
        public FeaturesVisualizer FeaturesVisualizer;
        public GameObject flag;

        private bool isScanning = false;
        private bool isLocalized = false;

        private void Start ()
        {
			if (Srv == null)
				Srv = FindObjectOfType<SrvController> ();
			
			BackBtn.onClick.AddListener (GoBack);

			// Hosting path
			HostGameBtn.onClick.AddListener (ActivateHostRoomUI);
			HostRoomBtn.onClick.AddListener (ActivateHostingRoomUI);
			StartGameBtn.onClick.AddListener (EnvironmentScanningClick);
			LeaveBtn.onClick.AddListener (LeavingHostingRoomConfirmation);
			StayBtn.onClick.AddListener (LeavingHostingRoomCancel);
			ConfirmBtn.onClick.AddListener (FailToCreateRoomConfirm);

			// Joining path
			JoinGameBtn.onClick.AddListener (ActivateViewRoomsUI);
		

			// SinglePlayer path
			SinglePlayerBtn.onClick.AddListener (EnvironmentScanningClick);

			// Adding MainMenu to UIStack
			UIStack.Push (MainMenuPanel);

			// Subscribe to connection server status change event
			Srv.Subscribe( OnConnectionStateChaged);

			Initialize ();
			//TODO DELETE THIS
			Screen.SetResolution (750, 1334, false);
        }

		#region Debug

		void Update ()
		{
			//Debug.Log (PhotonNetwork.playerList.Length);

		}

		#endregion Debug

		public void Initialize ()
		{
			MainMenuPanel.SetActive (true);
			BackBtn.gameObject.SetActive (false);
			LoadingCircle.SetActive (false);
			CreateHostRoomPanel.SetActive (false);
			HostingRoomPanel.SetActive (false);
			ViewRoomsPanel.SetActive (false);
			JoinedRoomPanel.SetActive (false);
			Srv.IsLeavingHostedRoom = false;
		}

        #region > Buttons On Click Events
        
        #region >> Start Game
        
        public void OnStartGameClick () // TODO UPDATE
        {
            if (string.IsNullOrEmpty(EnvironmentScannerController.Instance.LatestMapId)) {
                StatusText.text = "Error! Can't find Environment Scan!";
                return;
            }

            var res = EnvironmentScannerController.Instance.LoadLatestMap(LatestMapLoaded, MapLoadingFail, MapLoadingPercentage);

            if (res) {
                StartGameBtn.gameObject.SetActive (false);
            }
        }

        private void LatestMapLoaded(string mapId) // TODO UPDATE
        {
            StatusText.text = "Map Loaded Succesfully!";
            EnvironmentScannerController.Instance.OnPlacenoteStatusChange.AddListener (PlacenoteStatusChange);
        }
        
        private void MapLoadingFail (string mapId) // TODO UPDATE
        {
            StatusText.text = "Map Loading Fail!";
            StartGameBtn.gameObject.SetActive (true);
        }

        private void MapLoadingPercentage (float percentage) //TODO UPDATE
        {
            StatusText.text = "Loading Map..." + (percentage * 100f) + "%";
        }
        
        #endregion
        
		private void ActivateHostRoomUI ()
		{
			if (CreateHostRoomPanel != null)
				ActivateUI (CreateHostRoomPanel);

		}

		private void ActivateHostingRoomUI ()
		{
			if (HostingRoomPanel != null) {
				if (string.IsNullOrEmpty(HostingRoomNameInput.text))
					HostingRoomName.text = "Untitled";
				else
					HostingRoomName.text = HostingRoomNameInput.text;
				Connect ();
				Srv.HostRoom (HostingRoomName.text);
				UpdatePlayerAmounts (null, null, false);
				StartGameBtn.gameObject.SetActive (false);
				ActivateUI (HostingRoomPanel);
			}
		}

		private void ActivateViewRoomsUI ()
		{
			Connect ();
			LoadingCircle.SetActive (true);
			DeleteViewRooms ();
			if (ViewRoomsPanel != null)
				ActivateUI (ViewRoomsPanel);
		}
			

		private void ActivateJoinedRoomUI ()
		{
			if (JoinedRoomPanel != null) {
				UpdatePlayerAmounts (null, null, false);
				ActivateUI (JoinedRoomPanel);
			}
		}

		public void GoBack ()
		{
			GameObject currentUI = (GameObject) UIStack.Pop ();
			currentUI.SetActive (false);

			GameObject prevUI = (GameObject) UIStack.Peek ();
			prevUI.SetActive (true);

			LoadingCircle.SetActive (false);

			// Hide back button if at menu
			if (UIStack.Peek () == MainMenuPanel)
				BackBtn.gameObject.SetActive (false);

			// If going into view rooms panel from a joined room delete all room buttons
			if (currentUI == JoinedRoomPanel ) {
				LeaveRoom ();
				LoadingCircle.SetActive (true);
				DeleteViewRooms ();
			}
			// LeaveRoom and Disconnect if leaving a hosted room
			// TODO add confirmation from leaving host room
			if (currentUI == HostingRoomPanel) {
				prevUI.SetActive (false);
				BackBtn.gameObject.SetActive (false);
				LeavingHostingRoomConfirmationPanel.SetActive (true);
			}

			// Discommect if leaving the view rooms panel.
			if (currentUI == ViewRoomsPanel) {
				Disconnect ();
			}
		}

		private void ActivateUI (GameObject UIToActivate)
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
        
        private void EnvironmentScanningClick () //TODO Update
        {
			// TODO Update this hiding code
			GameObject currentUI = (GameObject) UIStack.Peek ();
			currentUI.SetActive (false);
			Srv.PrepareGameRPC ();
			PrepareGame ();
			//TargetSpawner.Instance.InstantiateAfterDelay (); // TODO check if this is a good place for this instead of below
            if (isScanning) {
                EnvironmentScannerController.Instance.FinishScanning (EnvironmentScanningFinish, EnvironmentScanningProgress);
                isScanning = false;
            } else {
                
                var startScan = EnvironmentScannerController.Instance.StartScanning ();
                if (startScan) {
                    FeaturesVisualizer.EnablePointcloud ();
                    //TargetSpawner.Instance.InstantiateAfterDelay();
                    isScanning = startScan;
                }
            }
        }

        private void EnvironmentScanningProgress (float progress) // TODO Update
        {
            StatusText.text = "Saving Progress..." + (progress * 100f) + "%";
        }
        
        private void EnvironmentScanningFinish (bool scanningRes) // TODO Update
        {
            StatusText.text = "Saving " + (scanningRes ? "success!" : "Error!");
            StartGameBtn.gameObject.SetActive (true);
            
            if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                ActivateEnvironmentScanningBtnRPC ();
            else
                GetComponent<PhotonView> ().RPC ("ActivateEnvironmentScanningBtnRPC", PhotonTargets.All); // Pretty sure this is not need
            
            FeaturesVisualizer.DisablePointcloud ();
			FeaturesVisualizer.clearPointcloud ();
            //StartEnvironmentScaningBtn.gameObject.SetActive(true);
        }

        [PunRPC]
        private void ActivateEnvironmentScanningBtnRPC () // TODO Update
        {
           // StartEnvironmentScaningBtn.gameObject.SetActive(true);
        }
        
        #endregion
        
        #endregion

		#region Dynamic UI generation

		public void GenerateViewRooms ()
		{
			// Clear the rooms first.
			DeleteViewRooms ();
			LoadingCircle.SetActive (true);
			int counter = 0;
			roomsArray = Srv.GetRooms ();
			if(roomsArray.Length > 0)
				LoadingCircle.SetActive (false);
			foreach (RoomInfo game in roomsArray) {
				GameObject room = (GameObject)Instantiate (roomButtonPrefab);
				int roomIndex = counter;
				room.GetComponent<Button> ().onClick.AddListener (
					() => {JoinRoom (roomIndex);}
				);
				room.GetComponent<Button> ().onClick.AddListener (ActivateJoinedRoomUI);
				room.transform.GetChild (0).GetComponent<Text> ().text = game.name;
				room.transform.GetChild (1).GetComponent<Text> ().text = game.PlayerCount + "/"+ game.MaxPlayers;
				room.transform.SetParent (RoomButtonsParent.transform);
				counter++;
			}
		}

		void DeleteViewRooms ()
		{
			foreach (Transform child in RoomButtonsParent.transform) {
				GameObject.Destroy (child.gameObject);
			}
		}

		public void UpdatePlayerAmounts (string players, string maxPlayers, bool realValues)
		{

			if (realValues) {
				HostingRoomPlayers.text = "Players: " + players + "/" + maxPlayers;
				JoinedRoomPlayers.text = "Players: " + players + "/" + maxPlayers;
			} else {
				HostingRoomPlayers.text = "Players: " + "Updating...";
				JoinedRoomPlayers.text = "Players: " + "Updating...";
			}
		}

		public void ActivateStartBtnUI ()
		{
			StartGameBtn.gameObject.SetActive (true);
		}

		public void LeavingHostingRoomConfirmation ()
		{
			LeavingHostingRoomConfirmationPanel.SetActive (false);
			// Make sure previous UI shows
			GameObject prevUI = (GameObject) UIStack.Peek ();
			prevUI.SetActive (true);
			BackBtn.gameObject.SetActive (true);
			LeaveRoom ();
		}

		public void LeavingHostingRoomCancel ()
		{
			LeavingHostingRoomConfirmationPanel.SetActive (false);
			// Reactivate UI and re-add to stack
			HostingRoomPanel.SetActive (true);
			UIStack.Push (HostingRoomPanel);
			BackBtn.gameObject.SetActive (true);
		}

		public void FailToCreateRoom ()
		{
			GameObject currentUI = (GameObject) UIStack.Pop ();
			currentUI.SetActive (false);
			BackBtn.gameObject.SetActive (false);
			FailToCreateRoomPanel.SetActive (true);
		}

		public void FailToCreateRoomConfirm ()
		{
			FailToCreateRoomPanel.SetActive (false);
			GameObject prevUI = (GameObject) UIStack.Peek ();
			prevUI.SetActive (true);
			BackBtn.gameObject.SetActive (true);
		}

		public void PrepareGame ()
		{
			GameObject currentUI = (GameObject) UIStack.Peek ();
			currentUI.SetActive (false);
			BackBtn.gameObject.SetActive (false);
			// Delete and reset Stack
			UIStack = new Stack();
			UIStack.Push (MainMenuPanel);
			GameController.PrepareGame ();
		
		}
		#endregion
        
		#region Networking

		void Connect ()
		{
			float pseudoUID = Time.realtimeSinceStartup;
			pseudoUID = pseudoUID - Mathf.FloorToInt (pseudoUID)*100000;
			string playerName = "Player" + Mathf.FloorToInt (pseudoUID);
			StatusText.text = playerName;
			Srv.Connect (playerName);
		}

		void Disconnect ()
		{
			Srv.Disconnect ();
		}

		void JoinRoom (int roomIndex)
		{
			JoinedRoomName.text = roomsArray[roomIndex].name;
			Srv.JoinRoom (roomsArray[roomIndex].name);
		}

		void LeaveRoom ()
		{
			Srv.LeaveRoom ();
		}

		/// <summary>
		/// Disconnects user after leaving room (Only if they are hosting).
		/// </summary>
		/// <param name="newId">Current NetGameState ID.</param>
		public void DisconnectAfterLeaveRoom (NetGameStateId newId)
		{
			switch (newId)
			{
			case NetGameStateId.ConnectedOutOfRoom:
				if (Srv.IsHost && !(Srv.IsQuitingToMainMenu)) {
					Srv.IsLeavingHostedRoom = true;
				}
				break;
			}
		}

		void OnConnectionStateChaged (NetGameStateId id, string val)
		{
			DisconnectAfterLeaveRoom (id);
		}

		#endregion

        public void PlacenoteStatusChange (LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
        {
            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST) {
                StatusText.text = "Localized! Game Starts Now!";
                TargetSpawner.Instance.InstantiateFlag ();

                
                GameController.StartGame ();
                PlacenoteShowPoints.SetActive (false);
                EnvironmentScannerController.Instance.OnPlacenoteStatusChange.RemoveListener (PlacenoteStatusChange);
            } 
            else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING) {
                StatusText.text = "Mapping";
            } 
            else if (currStatus == LibPlacenote.MappingStatus.LOST) {
                StatusText.text = "Searching for position lock";
            } 
            else if (currStatus == LibPlacenote.MappingStatus.WAITING) {
            }
        }
    }
}
