using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PunServerNs
{
    public class SrvController : Photon.PunBehaviour
    {

        [Header("Setup connection here")]
        [Tooltip("Only players with same GameNetVersion and PUN version can play with each other")]
        public string GameNetVersion = "1.00";
        public string DefaultUserName = "Player1";

        // parametert of room
        public string RoomName = "PrototypeRoom";
        RoomOptions roomOptions ;
        TypedLobby lobby = TypedLobby.Default;

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
            NetStateChanged(newNetState, message);
            LastConnectionMessage = message;
            Debug.Log("SetNetState: "+ newNetState+",'"+(message??"null")+"'");
        }


        event Action<NetGameStateId, string> NetStateChanged = delegate { };
        


        [Header("Public for debug only")]
        public NetGameStateId _curConnectionState;
        public string LastConnectionMessage;


        #region Monobehavior  standard methods
        void Awake()
        {
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.autoJoinLobby = true;


            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = false;


        }
        void Start()
        {
            // PhotonNetwork.connectionState = new ConnectionState();
            //dbgDoConnect = false;
            roomOptions = new RoomOptions();

        }



        // Update is called once per frame
        bool DoDisconnectOnce = true;
        void Update()
        {
          
            if (DoDisconnectOnce)
            {
                DoDisconnectOnce = false;
                PhotonNetwork.Disconnect();
            }
         
        }
        #endregion  Monobehavior  standard methods


        #region Public Methods

       // public bool dbgDoConnect;

        public void Connect(string playerName)
        {
            Debug.Log("Called Connect() at state " + PhotonNetwork.connectionState);

            if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
            {
                PhotonNetwork.playerName = playerName;
                Debug.Log("userid "+PhotonNetwork.player.UserId);
               // connect as defined in Photon configuration file
                PhotonNetwork.ConnectUsingSettings(GameNetVersion);
                SetNetState(NetGameStateId.Connecting, "Connecting started");
            }
            else
            {
                PhotonNetwork.Disconnect();
                SetNetState(NetGameStateId.Failed, "Disconnect by user");
               
            }
        }



           public void Subscribe(Action<NetGameStateId, string> onNetStateChanged)
        {
            NetStateChanged += onNetStateChanged;
        }

        public void UnSubscribe(Action<NetGameStateId,string> onNetStateChanged)
        {
            NetStateChanged -= onNetStateChanged;
        }


        #endregion  Public Methods

        #region Photon callbacks

        public override void OnConnectionFail(DisconnectCause cause)
        {
            Debug.Log("------ OnConnectionFail:"+cause);
            SetNetState(NetGameStateId.Failed, "Failed connection:" + cause.ToString());
        }
        public override void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
          
            Debug.Log("--------- OnFailedToConnectToPhoton:" + cause+ "---------------");
            SetNetState(NetGameStateId.Failed,"Failed connection to Photon:"+ cause.ToString());
        }

        public override void OnConnectedToPhoton()
        {
            Debug.Log("--------- OnConnectedToPhoton:---------------------");
            SetNetState(NetGameStateId.Connecting, "Connecting to Photon");
        }


        public override void OnJoinedLobby()
        {
            Debug.Log("--------- OnJoinedLobby: ------------------------");
            PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, lobby);
            SetNetState(NetGameStateId.Connecting, "Joined Lobby");
        }


        public override void OnConnectedToMaster()
        {

            Debug.Log("---------------  OnConnectedToMaster Region:" + PhotonNetwork.networkingPeer.CloudRegion);
            SetNetState(NetGameStateId.Connecting, "Connected to master server");
        }


        override public  void OnJoinedRoom()
        {
            SetNetState(NetGameStateId.Connected, 
                (PhotonNetwork.room == null? "null" : PhotonNetwork.room.Name) );
            Debug.Log("------------------------------    OnJoinedRoom:"+
                (PhotonNetwork.room == null ? "null" : PhotonNetwork.room.Name)
                +"------------------------------------------------");
            Debug.Log("------------------------------------------------------------");
            SetNetState(NetGameStateId.Connected,
                "Joined room:"+
                (PhotonNetwork.room == null ? "null" : PhotonNetwork.room.Name) );
        }

        public override void OnDisconnectedFromPhoton()
        {
            Debug.Log("-------- OnDisconnectedFromPhoton --------------");
            SetNetState(NetGameStateId.Disconnected, "Totally disconnected");

        }
        public override void OnLeftRoom()
        {
            Debug.Log("-------- OnLeftRoom --------------");
            SetNetState(NetGameStateId.Disconnected, "Left room");
        }

        public override void OnReceivedRoomListUpdate()
        {
            foreach (RoomInfo room in PhotonNetwork.GetRoomList())
            {
                Debug.Log("0000000000  " + room.Name + " 0000000");
            }

        }

        
        #endregion Photon callbacks


    }
}
