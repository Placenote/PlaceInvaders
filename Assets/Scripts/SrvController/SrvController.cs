using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameUiNs;
using Placenote;
using GameplayNs;

namespace PunServerNs
{
    public class SrvController : Photon.PunBehaviour
    {

        // Room status is a custom property of the room that all clients can see
        public enum RoomStatus
        {
            Mapping,
            Localizing,
            Playing,
        }

        public RoomStatus CurrRoomStatus
        {
            get { return (RoomStatus) PhotonNetwork.room.CustomProperties ["Status"]; }
            set
            {
                ExitGames.Client.Photon.Hashtable roomStatus = new ExitGames.Client.Photon.Hashtable
                {
                    { "Status", value }
                };
                PhotonNetwork.room.SetCustomProperties (roomStatus);
            }

        }

        public MainMenuUIController MainMenuUI;
        public GameUIController GameUI;
        public GameSetupController GameSetup;


        [Header ("Setup connection here")]
        [Tooltip ("Only players with same GameNetVersion and PUN version can play with each other")]
        public string GameNetVersion = "1.00";

        // parameters of room
        public string RoomName = "Untitled";
        RoomOptions roomOptions;
        TypedLobby lobby = TypedLobby.Default;

        public int TotalLocalizedPlayers
        {
            get { return (int) PhotonNetwork.room.CustomProperties ["LocalizedPlayers"]; }
            set
            {
                ExitGames.Client.Photon.Hashtable roomStatus = new ExitGames.Client.Photon.Hashtable
                {
                    { "LocalizedPlayers", value }
                };
                PhotonNetwork.room.SetCustomProperties (roomStatus);
            }
        }


        // Player values

        public int MaxPlayersInRoom
        {
            get { return PhotonNetwork.room.MaxPlayers; }
            private set { }
        }

        public int TotalPlayersInRoom
        {
            get { return PhotonNetwork.room.PlayerCount; }
            private set { }
        }

        // Host vs regular player bools
        public bool IsHost
        {
            get; private set;
        }
        public bool IsQuitingToMainMenu
        {
            get; private set;
        }

        // GPS
        public float latitude = 0;
        public float longitude = 0;
        public float GPSThreshold { get; private set; }

        /// <summary>
        ///  Status of connection from ready to network game point of view,
        ///  i. e. "connected" means "joined to room and ready to start the game" etc
        /// </summary>
        public NetGameStateId NetState
        {
            get { return _curConnectionState; }
        }
        void SetNetState (NetGameStateId newNetState, string message)
        {
            _curConnectionState = newNetState;
            NetStateChanged (newNetState, message);
            LastConnectionMessage = message;
            Debug.Log ("SetNetState: " + newNetState + ",'" + (message ?? "null") + "'");
        }


        event Action<NetGameStateId, string> NetStateChanged = delegate { };



        [Header ("Public for debug only")]
        public NetGameStateId _curConnectionState;
        public string LastConnectionMessage;


        #region Monobehavior  standard methods
        void Awake ()
        {
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.autoJoinLobby = true;

            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = false;     //TODO The tutorial says this should be true. Not sure
        }
        void Start ()
        {
            // PhotonNetwork.connectionState = new ConnectionState();
            //dbgDoConnect = false;
            roomOptions = new RoomOptions ()
            {
                MaxPlayers = 10
            };

            roomOptions.CustomRoomPropertiesForLobby = new string [1] { "GPS" };

            if (MainMenuUI == null)
                MainMenuUI = FindObjectOfType<MainMenuUIController> ();
            if (GameUI == null)
                GameUI = FindObjectOfType<GameUIController> ();
            if (GameSetup == null)
                GameSetup = FindObjectOfType<GameSetupController> ();

            PhotonNetwork.Disconnect ();

            IsHost = false;
            IsQuitingToMainMenu = false;
            GPSThreshold = 0.01f;
            StartCoroutine (StartLocationService ());
        }

        #endregion  Monobehavior  standard methods

        // TODO Move this
        private IEnumerator StartLocationService ()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.Log ("User has not enabled G" +
                    "PS");
                yield break;
            }

            Input.location.Start ();
            int maxWait = 10;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds (1);
                maxWait--;
            }

            if (maxWait <= 0)
            {
                Debug.Log ("TimedOut");
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log ("Unable to determine location");
                yield break;
            }

            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.latitude;
        }


        #region Public Methods

        public void Connect ()
        {
            Debug.Log ("Called Connect() at state " + PhotonNetwork.connectionState);
            if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
            {
                // Assigns random number as playerName
                float pseudoUID = UnityEngine.Random.Range (0.0f, 99.9f);
                pseudoUID = pseudoUID - Mathf.FloorToInt (pseudoUID) * 100000;
                string playerName = "Player" + Mathf.FloorToInt (pseudoUID);
                PhotonNetwork.playerName = playerName;
                Debug.Log ("userid " + PhotonNetwork.player.UserId);
                PhotonNetwork.ConnectUsingSettings (GameNetVersion);
                SetNetState (NetGameStateId.Connecting, "Connecting started");
            }
        }

        public void Disconnect ()
        {
            if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            {
                PhotonNetwork.Disconnect ();
                SetNetState (NetGameStateId.Failed, "Disconnect by user");
            }
        }

        /// <summary>
        /// Sets roomName and playerName to then connect to Photon with.
        /// </summary>
        /// <param name="roomName">Name of the hosted room.</param>
        public void HostRoom (string roomName)
        {
            RoomName = roomName;
            IsHost = true;
            Connect ();
        }

        public void JoinRoom (string roomName)
        {
            PhotonNetwork.JoinRoom (roomName);
        }

        public void LeaveRoom ()
        {
            PhotonNetwork.LeaveRoom ();
        }

        public void QuitToMainMenu ()
        {
            IsQuitingToMainMenu = true;
            Disconnect ();
            MainMenuUI.Initialize ();
        }

        public RoomInfo [] GetRooms ()
        {
            return PhotonNetwork.GetRoomList ();
        }

        public void Subscribe (Action<NetGameStateId, string> onNetStateChanged)
        {
            NetStateChanged += onNetStateChanged;
        }

        public void UnSubscribe (Action<NetGameStateId, string> onNetStateChanged)
        {
            NetStateChanged -= onNetStateChanged;
        }

        #endregion  Public Methods

        #region Photon callbacks

        public override void OnConnectionFail (DisconnectCause cause)
        {
            Debug.Log ("------ OnConnectionFail:" + cause);
            SetNetState (NetGameStateId.Failed, "Failed connection:" + cause.ToString ());
        }
        public override void OnFailedToConnectToPhoton (DisconnectCause cause)
        {

            Debug.Log ("--------- OnFailedToConnectToPhoton:" + cause + "---------------");
            SetNetState (NetGameStateId.Failed, "Failed connection to Photon:" + cause.ToString ());
        }

        public override void OnConnectedToPhoton ()
        {
            Debug.Log ("--------- OnConnectedToPhoton:---------------------");
            SetNetState (NetGameStateId.Connecting, "Connecting to Photon");
        }


        /// <summary>
        /// Callback when player joins lobby (Joining lobby is automatic after connecting to photon).
        /// If the player is host it creates a room.
        /// </summary>
        public override void OnJoinedLobby ()
        {
            Debug.Log ("--------- OnJoinedLobby: ------------------------");
            // Auto create a room if user is hosting
            if (IsHost)
            {
                // Add GPS as custom property
                float [] roomGPS = { latitude, longitude };
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable (1) { { "GPS", roomGPS } }; // add this line
                PhotonNetwork.CreateRoom (RoomName, roomOptions, lobby);
            }

            SetNetState (NetGameStateId.Connecting, "Joined Lobby");
        }


        public override void OnConnectedToMaster ()
        {

            Debug.Log ("---------------  OnConnectedToMaster Region:" + PhotonNetwork.networkingPeer.CloudRegion);
            SetNetState (NetGameStateId.Connecting, "Connected to master server");
        }


        override public void OnJoinedRoom ()
        {
            SetNetState (NetGameStateId.ConnectedInRoom,
                         (PhotonNetwork.room == null ? "null" : PhotonNetwork.room.Name));
            Debug.Log ("------------------------------    OnJoinedRoom:" +
                       (PhotonNetwork.room == null ? "null" : PhotonNetwork.room.Name)
                       + "------------------------------------------------");
            if (IsHost)
            {
                // Change room status to mapping
                CurrRoomStatus = RoomStatus.Mapping;
                Debug.Log (((float []) PhotonNetwork.room.CustomProperties ["GPS"]));
                MainMenuUI.LoadingCircle.SetActive (false); // TODO turn this into a callback in MainMenuUIController
                GameSetup.EnvironmentMappingStart ();
                TotalLocalizedPlayers = 0;

            }
            else
            {
                if (PhotonNetwork.room.CustomProperties ["MapId"] != null)
                {
                    EnvironmentScannerController.Instance.SetLatestMapId (PhotonNetwork.room.CustomProperties ["MapId"].ToString ());
                }
                if (CurrRoomStatus == RoomStatus.Mapping)
                {
                    GameUI.HelperText.text = "Wait while host maps the area!";
                }
                else if (CurrRoomStatus == RoomStatus.Localizing)
                {
                    GameUI.HelperText.text = "Move and look to where the map was created to localize.";
                    // Load the map
                    GameSetup.StartLoadingMap ();
                }
                else if (CurrRoomStatus == RoomStatus.Playing)
                {
                    GameUI.HelperText.text = "The game has started. You need to localize before you can play!";
                    // Load the map
                    GameSetup.StartLoadingMap ();
                }
                GameController.PrepareGame ();
            }


        }

        public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer)
        {
            Debug.Log ("-------- OnPhotonPlayerConnected --------------");
            Debug.Log ("Another player has joined!");
            // Update player amounts
            GameController.Data.UpdatePlayerAmounts ();

        }

        public override void OnPhotonPlayerDisconnected (PhotonPlayer newPlayer)
        {
            Debug.Log ("-------- OnPhotonPlayerDisconnected --------------");
            Debug.Log ("Another player has Left!");
            // Update player amounts
            GameController.Data.UpdatePlayerAmounts ();

        }

        public override void OnDisconnectedFromPhoton ()
        {
            Debug.Log ("-------- OnDisconnectedFromPhoton --------------");
            SetNetState (NetGameStateId.Disconnected, "Totally disconnected");
            // User is no longer host when totally disconnected from photon.
            IsHost = false;
            IsQuitingToMainMenu = false;
        }

        public override void OnLeftRoom ()
        {
            Debug.Log ("-------- OnLeftRoom --------------");
            SetNetState (NetGameStateId.ConnectedOutOfRoom, "Left room");
        }

        public override void OnReceivedRoomListUpdate ()
        {
            foreach (RoomInfo room in PhotonNetwork.GetRoomList ())
            {
                Debug.Log ("0000000000  " + room.Name + " 0000000");
            }
            // Generate buttons for viewRoomsPanel
            if (!IsHost)
                MainMenuUI.GenerateViewRooms ();
        }

        public override void OnPhotonCreateRoomFailed (object [] codeAndMsg)
        {
            Debug.Log ("-------- OnPhotonCreateRoomFailed:" + codeAndMsg [0].ToString ());
            SetNetState (NetGameStateId.Failed, "Failed connection:" + codeAndMsg [0].ToString ());
            Disconnect ();
            MainMenuUI.FailToCreateRoom ();
        }


        #endregion Photon callbacks


    }
}
