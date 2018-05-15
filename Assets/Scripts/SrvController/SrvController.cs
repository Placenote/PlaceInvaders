using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameUiNs;

namespace PunServerNs
{
    public class SrvController : Photon.PunBehaviour
    {
		public SetupUI SetupUIInstance;

		[Header ("Setup connection here")]
        [Tooltip ("Only players with same GameNetVersion and PUN version can play with each other")]
        public string GameNetVersion = "1.00";
        public string DefaultUserName = "Player1";

        // parametert of room
        public string RoomName = "PrototypeRoom";
        RoomOptions roomOptions;
        TypedLobby lobby = TypedLobby.Default;

		// Hosting
		public bool IsHost { get; private set; } 
		public bool IsLeavingHostedRoom { private get; set; }
		public bool IsQuitingToMainMenu { get; private set; }



        /// <summary>
        ///  Status of connection from ready to network game point of view,
        ///  i. e. "connected" means "joined to room and ready to start the game" etc
        /// </summary>
        public NetGameStateId NetState
        {
            get {return _curConnectionState; }
        }
        void SetNetState(NetGameStateId newNetState, string message)
        {
            _curConnectionState = newNetState;
            NetStateChanged (newNetState, message);
            LastConnectionMessage = message;
            Debug.Log("SetNetState: "+ newNetState+",'"+ (message??"null") +"'");
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
            PhotonNetwork.automaticallySyncScene = false; //TODO The tutorial says this should be true. Not sure...


        }
        void Start ()
        {
            // PhotonNetwork.connectionState = new ConnectionState();
            //dbgDoConnect = false;
			roomOptions = new RoomOptions () { MaxPlayers = 10};

			if (SetupUIInstance == null)
				SetupUIInstance = FindObjectOfType<SetupUI> ();
			PhotonNetwork.Disconnect ();


			IsHost = false;
			IsLeavingHostedRoom = false;
			IsQuitingToMainMenu = false;
        }
			
        #endregion  Monobehavior  standard methods


        #region Public Methods

		public void Connect (string playerName)
        {
            Debug.Log ("Called Connect() at state " + PhotonNetwork.connectionState);
            if (PhotonNetwork.connectionState == ConnectionState.Disconnected) {
                PhotonNetwork.playerName = playerName;
                Debug.Log ("userid "+PhotonNetwork.player.UserId);
                // connect as defined in Photon configuration file
                PhotonNetwork.ConnectUsingSettings (GameNetVersion);
                SetNetState (NetGameStateId.Connecting, "Connecting started");
            }
        }

		public void Disconnect ()
		{
			if (PhotonNetwork.connectionState != ConnectionState.Disconnected) {
				PhotonNetwork.Disconnect ();
				SetNetState (NetGameStateId.Failed, "Disconnect by user");
			}
		}

		public void HostRoom (string roomName)
		{
			RoomName = roomName;
			IsHost = true;
		}

		public void JoinRoom (string roomName)
		{
			PhotonNetwork.JoinRoom (roomName);
		}
			
		public void LeaveRoom ()
		{
			// Kick all other players if host leaves
			if (PhotonNetwork.isMasterClient) {
				GetComponent<PhotonView> ().RPC ("KickPlayer", PhotonTargets.Others);
			}
			PhotonNetwork.LeaveRoom ();
		}

		public void QuitToMainMenu ()
		{
			IsQuitingToMainMenu = true;
			Disconnect (); // TODO Test what happens when hosts quits but other players are still there
			SetupUIInstance.Initialize();
		}

		/// <summary>
		/// Kicks the player via RPC from LeaveRoom().
		/// </summary>
		[PunRPC]
		private void KickPlayer ()
		{
			Debug.Log ("GOT KICKED");
			SetupUIInstance.GoBack ();
		}

		public RoomInfo[] GetRooms ()
		{
			return PhotonNetwork.GetRoomList ();
		}

		public void PrepareGameRPC ()
		{
			GetComponent<PhotonView> ().RPC ("PrepareGame", PhotonTargets.Others);
		}

		[PunRPC]
		private void PrepareGame ()
		{
			SetupUIInstance.PrepareGame ();
		}

		private void UpdatePlayerAmounts ()
		{
			SetupUIInstance.UpdatePlayerAmounts (PhotonNetwork.room.PlayerCount.ToString(), PhotonNetwork.room.MaxPlayers.ToString(), true);
		}

        public void Subscribe (Action<NetGameStateId, string> onNetStateChanged)
        {
            NetStateChanged += onNetStateChanged;
        }

        public void UnSubscribe (Action<NetGameStateId,string> onNetStateChanged)
        {
            NetStateChanged -= onNetStateChanged;
        }


        #endregion  Public Methods

        #region Photon callbacks

        public override void OnConnectionFail (DisconnectCause cause)
        {
            Debug.Log ("------ OnConnectionFail:"+cause);
            SetNetState(NetGameStateId.Failed, "Failed connection:" + cause.ToString ());
        }
        public override void OnFailedToConnectToPhoton (DisconnectCause cause)
        {
          
            Debug.Log ("--------- OnFailedToConnectToPhoton:" + cause+ "---------------");
            SetNetState (NetGameStateId.Failed,"Failed connection to Photon:"+ cause.ToString ());
        }

        public override void OnConnectedToPhoton ()
        {
            Debug.Log ("--------- OnConnectedToPhoton:---------------------");
            SetNetState (NetGameStateId.Connecting, "Connecting to Photon");
        }


        public override void OnJoinedLobby ()
        {
            Debug.Log ("--------- OnJoinedLobby: ------------------------");
			// Auto create a room if user is hosting
			if (IsHost)
				PhotonNetwork.CreateRoom (RoomName, roomOptions, lobby);
			// If the user is leaving their own hosted room then disconnect them from photon
			// (This has to be in OnJoinedLobby because AutoJoinLobby is on).
			if (IsLeavingHostedRoom) {
				Disconnect ();
				IsLeavingHostedRoom = false;
			}

            SetNetState (NetGameStateId.Connecting, "Joined Lobby");
        }


        public override void OnConnectedToMaster ()
        {

            Debug.Log ("---------------  OnConnectedToMaster Region:" + PhotonNetwork.networkingPeer.CloudRegion);
            SetNetState (NetGameStateId.Connecting, "Connected to master server");
        }


        override public  void OnJoinedRoom ()
        {
            SetNetState (NetGameStateId.ConnectedInRoom, 
                (PhotonNetwork.room == null? "null" : PhotonNetwork.room.Name) );
            Debug.Log ("------------------------------    OnJoinedRoom:"+
                (PhotonNetwork.room == null ? "null" : PhotonNetwork.room.Name)
                +"------------------------------------------------");
            Debug.Log ("------------------------------------------------------------");
            SetNetState (NetGameStateId.ConnectedInRoom,
                "Joined room:"+
                (PhotonNetwork.room == null ? "null" : PhotonNetwork.room.Name) );
			UpdatePlayerAmounts ();
			if (IsHost)
				SetupUIInstance.ActivateStartBtnUI ();
        }

		public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer)
		{
			Debug.Log ("-------- OnPhotonPlayerConnected --------------");
			Debug.Log ("Another player has joined!");
			// Update player amounts
			UpdatePlayerAmounts ();

		}

		public override void OnPhotonPlayerDisconnected (PhotonPlayer newPlayer)
		{
			Debug.Log ("-------- OnPhotonPlayerDisconnected --------------");
			Debug.Log ("Another player has Left!");
			// Update player amounts
			UpdatePlayerAmounts ();

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
				SetupUIInstance.GenerateViewRooms ();
        }

		public override void OnPhotonCreateRoomFailed (object[] codeAndMsg)
		{
			Debug.Log ("-------- OnPhotonCreateRoomFailed:" + codeAndMsg[0].ToString ());
			SetNetState(NetGameStateId.Failed, "Failed connection:" + codeAndMsg[0].ToString ());
			Disconnect ();
			SetupUIInstance.FailToCreateRoom ();
		}

        
        #endregion Photon callbacks


    }
}
