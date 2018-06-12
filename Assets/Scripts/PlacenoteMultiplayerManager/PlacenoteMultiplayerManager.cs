using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

namespace Placenote
{
    /// <summary>
    /// The master class to handle Placenote mapping and Photon Multiplayer.
    /// </summary>
    /// <remarks>
    /// Has functionality to handle Photon connection, room setup, joining, and leaving
    /// Also can handle Placenote mapping, map saving, map loading, and localization.
    /// Contains subscribable events designed to integrate with your existing game.
    /// Don't implement PlacenoteMultiplayerManager in your classes. Instead, implent PlacenotePunMultiplayerBehaviour.
    /// </remarks>
    public class PlacenoteMultiplayerManager : Photon.PunBehaviour, PlacenoteListener
    {
        /// <summary>
        /// All subscribable events for the PlacenotePunMultiplayerBehaviour class.
        /// See PlacenotePunMultiplayerBehaviour class for details on each event.
        /// </summary>
        #region PlacenotePunMultiplayerBehaviour

        //Photon connection events
        public event Action OnConnectedToPhotonEvent = delegate { };
        public event Action OnFailedToConnectToPhotonEvent = delegate { };

        // Room events
        public event Action OnRoomListUpdateEvent = delegate { };
        public event Action OnStartJoiningRoomEvent = delegate { };
        public event Action OnJoinedRoomEvent = delegate { };
        public event Action OnJoinedMappedRoomEvent = delegate { };
        public event Action<string> OnHostRoomErrorEvent = delegate { };

        // Mapping events
        public event Action OnMappingStartEvent = delegate { };
        public event Action OnMappingFailedEvent = delegate { };
        public event Action OnMappingCompleteEvent = delegate { };
        public event Action OnMappingSufficientEvent = delegate { };

        // Mapping progress events
        public event Action<bool> OnMapSavingStatusUpdateEvent = delegate { };
        public event Action<float> OnMapSavingProgressUpdateEvent = delegate { };
        public event Action<bool> OnMapLoadingStatusUpdateEvent = delegate { };
        public event Action<float> OnMapLoadingProgressUpdateEvent = delegate { };

        // Localization events
        public event Action OnLocalizationLostEvent = delegate { };
        public event Action OnLocalizedEvent = delegate { };

        // Game events
        public event Action OnGameStartEvent = delegate { };
        public event Action OnGameQuitEvent = delegate { };

        // Player counting value events
        public event Action OnPlayerValueUpdateEvent = delegate { };

        #endregion PlacenotePunMultiplayerBehaviour

        #region Photon initialization

        [Tooltip ("Only players with same GameNetVersion and PUN version can play with each other")]
        public string mGameNetVersion = "1.00";

        // PhotonView setup to use RPC
        PhotonView mPhotonView;

        // Is this photon client the host of the room
        public bool IsHost { get; private set; }

        #region > Room Info

        // Parameters of room
        private string mRoomName;
        private RoomOptions mRoomOptions;
        private TypedLobby mLobby = TypedLobby.Default;

        /// <summary>
        /// Gets or sets a value indicating whether this has room started.
        /// Saved as Custom property of room, so all players can view it.
        /// </summary>
        public bool HasRoomStarted
        {
            get { return (bool)PhotonNetwork.room.CustomProperties["HasRoomStarted"]; }
            private set
            {
                ExitGames.Client.Photon.Hashtable roomStarted = new ExitGames.Client.Photon.Hashtable
                {
                    {"HasRoomStarted", value}
                };
                PhotonNetwork.room.SetCustomProperties (roomStarted);
            }
        }

        /// <summary>
        /// The GPS location of the where the room was created.
        /// Latitude first then longitude.
        /// Saved as Custom property of room, so other players can filter their viewable 
        /// rooms using GetListOfNearbyRooms
        /// </summary>
        private float[] RoomGPS
        {
            get { return (float[])PhotonNetwork.room.CustomProperties["GPS"]; }
            set
            {
                ExitGames.Client.Photon.Hashtable GPSValues = new ExitGames.Client.Photon.Hashtable
                {
                    {"GPS", value}
                };
                PhotonNetwork.room.SetCustomProperties (GPSValues);
            }
        }

        /// <summary>
        /// The Placenote MapId associated with the current room.
        /// The host sets this value when they finish mapping.
        /// Saved as Custom property of room, so other players can load map after joining room.
        /// </summary>
        private string LatestMapId
        {
            get { return (string)PhotonNetwork.room.CustomProperties["MapId"]; }
            set
            {
                ExitGames.Client.Photon.Hashtable mapId = new ExitGames.Client.Photon.Hashtable
                {
                    {"MapId", value}
                };
                PhotonNetwork.room.SetCustomProperties (mapId);
            }
        }

        /// <summary>
        /// The total players playing (ie. able to shoot) in a room.
        /// Saved as a custom property of the room, so that all clients have access to 
        /// the same value.
        /// </summary>
        public int TotalPlayersPlaying
        {
            get
            {
                if (PhotonNetwork.room.CustomProperties["LocalizedPlayers"] != null)
                    return (int)PhotonNetwork.room.CustomProperties["LocalizedPlayers"];
                else
                    return -1; // -1 indicates error
            }
            set
            {
                ExitGames.Client.Photon.Hashtable roomStatus = new ExitGames.Client.Photon.Hashtable
                {
                    { "LocalizedPlayers", value }
                };
                PhotonNetwork.room.SetCustomProperties (roomStatus);
            }
        }

        /// <summary>
        /// Gets the total players in room from PhotonNetwork
        /// </summary>
        public int TotalPlayersInRoom
        {
            get { return PhotonNetwork.room.PlayerCount; }
            private set { }
        }

        /// <summary>
        /// Gets the max players in room from PhotonNetwork
        /// </summary>
        public int MaxPlayersInRoom
        {
            get { return PhotonNetwork.room.MaxPlayers; }
            private set { }
        }

        #endregion > Room Info

        #endregion Photon initialization

        #region ARKit initialization 

        public UnityEvent OnARKitInitialized = new UnityEvent ();
        private UnityARSessionNativeInterface mSession;
        private bool mFrameUpdated = false;
        private UnityARImageFrameData mImage = null;
        private UnityARCamera mARCamera;
        private bool mARKitInit = false;

        #endregion ARKit initialization

        #region Local player info

        // The local players current GPS value
        private float[] mUserGPS;

        // Has the player stared the game yet?
        public bool IsPlaying { private set; get; }

        // Is the player localized on the map?
        public bool IsLocalized { private set; get; }

        #endregion Local player info

        // Variable to control debugging features
        public bool mDebug;

        #region Singleton

        private static PlacenoteMultiplayerManager sInstance = null;
        public static PlacenoteMultiplayerManager Instance
        {
            get { return sInstance; }
        }

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public static bool IsInitialized
        {
            get { return sInstance != null; }
        }

        /// <summary>
        /// Base awake method that sets the singleton's unique instance.
        /// </summary>
        protected virtual void Awake ()
        {
            if (sInstance != null)
            {
                Debug.LogError ("Trying to instantiate a second instance of PlacenoteMultiplayerManager singleton ");
            }
            else
            {
                sInstance = this;
                PhotonNetwork.autoJoinLobby = true;
                PhotonNetwork.automaticallySyncScene = false;
                mPhotonView = sInstance.GetComponent<PhotonView> ();
            }
        }

        protected virtual void OnDestroy ()
        {
            if (sInstance == this)
            {
                sInstance = null;
            }
        }

        #endregion Singleton

        #region Standard Unity Functions Except Awake (In Singleton)

        private void Start ()
        {
            mRoomOptions = new RoomOptions () { MaxPlayers = 10 };
            IsHost = false;
            IsPlaying = false;
            IsLocalized = false;

            // Allows for GPS custom property to be readable in Photon lobby
            mRoomOptions.CustomRoomPropertiesForLobby = new string[1] { "GPS" };
            // Start GPS setup
            StartCoroutine (StartLocationService ());
            // Start connection to photon
            Connect ();

            // Required for OnStatusChange and OnPose to be called
            LibPlacenote.Instance.RegisterListener (this);

            // ARKit Initialization
            mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
            UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
            StartARKit ();

            if (mDebug)
            {
                FeaturesVisualizer.EnablePointcloud ();
            }
        }

        private void Update ()
        {
            // Sends current from to Libplacenote whenever it is updated
            if (mFrameUpdated)
            {
                mFrameUpdated = false;
                if (mImage == null)
                {
                    InitARFrameBuffer ();
                }

                if (mARCamera.trackingState == ARTrackingState.ARTrackingStateNotAvailable)
                {
                    // ARKit pose is not yet initialized
                    return;
                }
                else if (!mARKitInit)
                {
                    mARKitInit = true;
                    OnARKitInitialized.Invoke ();
                }

                Matrix4x4 matrix = mSession.GetCameraPose ();

                Vector3 arkitPosition = PNUtility.MatrixOps.GetPosition (matrix);
                Quaternion arkitQuat = PNUtility.MatrixOps.GetRotation (matrix);

                LibPlacenote.Instance.SendARFrame (mImage, arkitPosition, arkitQuat, mARCamera.videoParams.screenOrientation);
            }
        }

        #endregion Standard Unity Functions Except Awake (In Singleton)

        #region GPS

        /// <summary>
        /// Starts the device's location service to get GPS coords.
        /// </summary>
        private IEnumerator StartLocationService ()
        {
            #if UNITY_EDITOR // For debug
                mUserGPS = new float[] { 43.4513604f, -80.49861820000001f };
            #endif
            if (!Input.location.isEnabledByUser)
            {
                Debug.Log ("User has not enabled GPS");
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

            mUserGPS = new float[] { Input.location.lastData.latitude, Input.location.lastData.longitude };
        }

        #endregion GPS

        #region Photon functions

        #region > Public Photon functions

        /// <summary>
        /// Host a photon room.
        /// Calls OnStartJoiningRoomEvent
        /// </summary>
        public void HostRoom (string roomName)
        {
            IsHost = true;
            mRoomName = roomName;
            if (String.IsNullOrEmpty (mRoomName))
            {
                // Could not create room because...
                OnHostRoomErrorEvent ("Empty room name");
                return;
            }
            OnStartJoiningRoomEvent ();
            PhotonNetwork.CreateRoom (mRoomName, mRoomOptions, mLobby);
        }

        // Overloaded host room function that accepts a GameObject. 
        // Used for Unity Editor button OnClick functions
        public void HostRoom (GameObject roomName)
        {
            IsHost = true;
            mRoomName = roomName.GetComponent<Text> ().text;
            if (String.IsNullOrEmpty (mRoomName))
            {
                // Could not create room because...
                OnHostRoomErrorEvent ("of an empty room name.");
                return;
            }
            OnStartJoiningRoomEvent ();
            PhotonNetwork.CreateRoom (mRoomName, mRoomOptions, mLobby);
        }

        /// <summary>
        /// Joins photon room with name specified by roomName.
        /// Calls OnStartJoiningRoomEvent
        /// </summary>
        public void JoinRoom (string roomName)
        {
            if (String.IsNullOrEmpty (roomName))
            {
                Debug.Log ("No room name set! Unable to join.");
                return;
            }
            OnStartJoiningRoomEvent ();
            PhotonNetwork.JoinRoom (roomName);
        }

        /// <summary>
        /// Gets the list of nearby rooms.
        /// </summary>
        /// <returns>The list of nearby rooms as RoomInfo object</returns>
        /// <param name="meterRange">The range in meters to look for rooms. Default 100m</param>
        public RoomInfo[] GetListOfNearbyRooms (float meterRange = 100f)
        {
            RoomInfo[] allPhotonRooms = PhotonNetwork.GetRoomList ();
            List<RoomInfo> nearbyPhotonRooms = new List<RoomInfo> ();

            // Conversion of meterRange to latitude and longitude
            float GPSThreshold = meterRange / 111111f;

            foreach (RoomInfo room in allPhotonRooms)
            {
                float[] roomGPS = (float[])room.CustomProperties["GPS"];
                if (Mathf.Abs (roomGPS[0] - mUserGPS[0]) <= GPSThreshold
                    && Mathf.Abs (roomGPS[1] - mUserGPS[1]) <= GPSThreshold)
                {
                    nearbyPhotonRooms.Add (room);
                }
            }
            return nearbyPhotonRooms.ToArray ();
        }

        #endregion > Public Photon functions

        #region > Private Photon functions

        /// <summary>
        /// Connects to Photon Network.
        /// Called in Start.
        /// </summary>
        private void Connect ()
        {
            Debug.Log ("Called Connect() at state " + PhotonNetwork.connectionState);
            if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
            {
                PhotonNetwork.ConnectUsingSettings (mGameNetVersion);
                Debug.Log ("Connecting started");
            }
        }

        /// <summary>
        /// Disconnects from Photon Network.
        /// Never called. Only used for debuging.
        /// </summary>
        private void Disconnect ()
        {
            if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            {
                PhotonNetwork.Disconnect ();
                Debug.Log ("Disconnect by user");
            }
        }

        #endregion > Private Photon functions

        /// <summary>
        /// Photon callbacks that are called automatically. Mostly for debug messages.
        /// Inherited from Punbehaviour.
        /// </summary>
        #region > Photon Callbacks

        public override void OnConnectionFail (DisconnectCause cause)
        {
            Debug.Log ("Photon OnConnectionFail:" + cause);
        }

        // Calls OnFailedToConnectToPhotonEvent
        public override void OnFailedToConnectToPhoton (DisconnectCause cause)
        {
            Debug.Log ("Photon OnFailedToConnectToPhoton:" + cause);
            OnFailedToConnectToPhotonEvent ();
        }

        // Calls OnConnectedToPhotonEvent
        public override void OnConnectedToPhoton ()
        {
            Debug.Log ("Photon OnConnectedToPhoton");
            OnConnectedToPhotonEvent ();
        }

        public override void OnJoinedLobby ()
        {
            Debug.Log ("Photon OnJoinedLobby");
        }

        /// <summary>
        /// If player is host it starts mapping. 
        /// Then calls OnMappingFailedEvent if mapping failed.
        /// If the player is not the host it updates on screen text according to what state
        /// the room is in. Then it updates the UI with PrepareGame().
        /// Then calls OnJoinedMappedRoomEvent if there is a MapId set.
        /// Calls OnJoinedRoomEvent
        /// </summary>
        override public void OnJoinedRoom ()
        {
            OnJoinedRoomEvent ();
            Debug.Log ("Photon OnJoinedRoom:" +
                       (PhotonNetwork.room == null ? "null" : PhotonNetwork.room.Name));
            if (IsHost)
            {
                if (StartMapping ())
                {
                    // The room has been successfully created, now set the GPS, so other players can join
                    RoomGPS = mUserGPS;
                    HasRoomStarted = false;
                    // No players have started playing yet
                    TotalPlayersPlaying = 0;
                }
                else
                {
                    OnMappingFailedEvent ();
                }
            }
            else
            {
                // If you are not the host and you join a room with a map id
                // then load that m
                if (LatestMapId != null)
                {
                    LoadLatestMap ();
                    OnJoinedMappedRoomEvent ();
                }
            }
            // Update player count values
            mPhotonView.RPC ("UpdatePlayerValueRPC", PhotonTargets.All);
        }

        public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer)
        {
            Debug.Log ("Photon OnPhotonPlayerConnected");
            mPhotonView.RPC ("UpdatePlayerValueRPC", PhotonTargets.All);
        }

        public override void OnPhotonPlayerDisconnected (PhotonPlayer newPlayer)
        {
            Debug.Log ("Photon OnPhotonPlayerDisconnected");
            mPhotonView.RPC ("UpdatePlayerValueRPC", PhotonTargets.All);
        }

        public override void OnDisconnectedFromPhoton ()
        {
            Debug.Log ("Photon OnDisconnectedFromPhoton");
        }

        public override void OnLeftRoom ()
        {
            Debug.Log ("Photon OnLeftRoom");
            // User is no longer host when totally disconnected from photon.
            IsHost = false;
        }

        // Calls OnRoomListUpdateEvent
        public override void OnReceivedRoomListUpdate ()
        {
            Debug.Log ("Photon OnReceivedRoomListUpdate. Rooms ->");
            foreach (RoomInfo room in PhotonNetwork.GetRoomList ())
                Debug.Log ("Room Name: " + room.Name);
            OnRoomListUpdateEvent ();
        }

        // Calls OnHostRoomErrorEvent
        public override void OnPhotonCreateRoomFailed (object[] codeAndMsg)
        {
            Debug.Log ("Photon OnPhotonCreateRoomFailed:" + codeAndMsg[0].ToString ());
            OnHostRoomErrorEvent ("that room name already exists.");
        }

        // Updates IsHost to match MasterClient
        public override void OnMasterClientSwitched (PhotonPlayer newMasterClient)
        {
            Debug.Log ("Photon OnMasterClientSwitched");
            // If this client is now the new Master client, 
            // and there is no map yet then start the mapping process
            if (PhotonNetwork.isMasterClient && string.IsNullOrEmpty (LatestMapId))
            {
                IsHost = true;
                StartMapping ();
            }
        }

        #endregion > Photon Callbacks

        #endregion Photon functions

        #region Placenote functions

        #region > Mapping session

        /// <summary>
        /// Starts a mapping session. Only the host needs to map the play area.
        /// Returns the success of initializing a mapping session.
        /// Called inside OnJoinedRoom (if IsHost)
        /// Called inside OnMasterClientSwitched (if there is no MapId for the room).
        /// Calls OnMappingStartEvent
        /// </summary>
        /// <returns><c>true</c>, if mapping was started, <c>false</c> otherwise.</returns>
        public bool StartMapping ()
        {
            if (!LibPlacenote.Instance.Initialized ())
            {
                Debug.Log ("SDK not yet initialized");
                return false;
            }

            FeaturesVisualizer.EnablePointcloud ();
            LibPlacenote.Instance.StartSession ();
            // Start checking amount of map points.
            sInstance.InvokeRepeating ("CheckMapping", 0f, 0.5f);
            OnMappingStartEvent ();
            return true;
        }

        /// <summary>
        /// Invoked by StartMapping.
        /// Check if the current map has sufficient points to be considered a 
        /// good map. Requires >50 points each with >=4 measurement count.
        /// Calls OnMappingSufficientEvent when requirement is met.
        /// </summary>
        public void CheckMapping ()
        {
            LibPlacenote.PNFeaturePointUnity[] map = LibPlacenote.Instance.GetMap ();
            int amountOfGoodPoints = 0;
            if (map != null)
            {
                for (int i = 0; i < map.Length; ++i)
                {
                    if (map[i].measCount >= 4)
                    {
                        amountOfGoodPoints++;
                    }
                }   
            }
            if (amountOfGoodPoints >= 50) 
            {
                sInstance.CancelInvoke ();
                OnMappingSufficientEvent ();
            }
        }

        /// <summary>
        /// Stops the current mapping session. Only the host needs to stop because
        /// they are the only player mapping.
        /// Calls OnMappingCompleteEvent.
        /// </summary>
        public void StopMapping ()
        {
            // Disable visable point cloud
            if (!mDebug)
            {
                FeaturesVisualizer.DisablePointcloud ();
                FeaturesVisualizer.clearPointcloud ();
            }

            // Saves map and gives feedback via EnvironmentMappingProgress() and EnvironmentMappingComplete()
            LibPlacenote.Instance.SaveMap ((mapId) =>
            {
                LatestMapId = mapId;
            }, (completed, faulted, percentage) =>
            {
                if (!completed && !faulted)
                {
                    Debug.Log ("Saving Progress..." + (percentage * 100f) + "%");
                    OnMapSavingProgressUpdateEvent (percentage);
                }
                else
                {
                    // Stop the mapping session
                    LibPlacenote.Instance.StopSession (); 
                    bool savingSuccess = !faulted;
                    Debug.Log ("Saving " + (savingSuccess ? "success!" : "Error!"));
                    OnMapSavingStatusUpdateEvent (savingSuccess);
                    if (savingSuccess)
                    {
                        // When mapping is complete all current players should begin loading the map
                        mPhotonView.RPC ("LoadMapRPC", PhotonTargets.All);
                        OnMappingCompleteEvent ();
                    }
                }
            });
        }

#endregion > Mapping session

#region > Loading Map

        [PunRPC]
        public void LoadMapRPC ()
        {
            LoadLatestMap ();
        }

        /// <summary>
        /// Loads the latest map.
        /// </summary>
        public void LoadLatestMap ()
        {
            if (string.IsNullOrEmpty (LatestMapId))
            {
                Debug.Log ("No mapId is set! Cannot load map");
                OnMapLoadingStatusUpdateEvent (false);
                return;
            }

            if (!LibPlacenote.Instance.Initialized ())
            {
                Debug.Log ("SDK not yet initialized");
                OnMapLoadingStatusUpdateEvent (false);
                return;
            }

            LibPlacenote.Instance.LoadMap (LatestMapId, (completed, faulted, percentage) =>
            {
                if (completed)
                {
                    // Starts Session for localization
                    LibPlacenote.Instance.StartSession ();
                    OnMapLoadingStatusUpdateEvent (completed);
                }
                else if (faulted)
                {
                    OnMapLoadingStatusUpdateEvent (faulted);
                }
                else
                {
                    OnMapLoadingProgressUpdateEvent (percentage);
                }
            });
        }
        #endregion > Loading Map

#region > Quiting Game

        /// <summary>
        /// Stops Placenote session, leaves photon room, and resets player info.
        /// Should be called when a player quits a game session.
        /// (Game session refers to the gameplay after the mapping and 
        /// localization is complete).
        /// Calls OnGameQuitEvent.
        /// </summary>
        public void QuitGame ()
        {
            Debug.Log ("Quiting Game...");
            if (IsPlaying)
            {
                // Player is quiting game, so decrease count value.
                TotalPlayersPlaying -= 1;
            }
            // Stop checking if map has enough points
            sInstance.CancelInvoke ();
            // Player is no longer playing or host.
            IsPlaying = false;
            IsHost = false;
            IsLocalized = false;

            // Update player count value for all players
            mPhotonView.RPC ("UpdatePlayerValueRPC", PhotonTargets.All);
            StopUsingMap ();
            OnGameQuitEvent ();
            PhotonNetwork.LeaveRoom ();
        }

        /// <summary>
        /// Stops Placenote session.
        /// </summary>
        private void StopUsingMap ()
        {
            FeaturesVisualizer.DisablePointcloud ();
            FeaturesVisualizer.clearPointcloud ();
            LibPlacenote.Instance.StopSession ();
        }

        #endregion > Quiting Game

        /// <summary>
        /// Needed for the PlacenoteListener interface requirement. Not used...
        /// </summary>
        public void OnPose (Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

        /// <summary>
        /// Callback for when the Placenote status changes. 
        /// Calls OnLocalizatedEvent.
        /// Calls OnLocalizationLostEvent.
        /// </summary>
        /// <param name="prevStatus">Previous status of the Placenote session.</param>
        /// <param name="currStatus">Current status of the Placenote session.</param>
        public void OnStatusChange (LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
        {
            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
            {
                OnLocalizedEvent ();
                IsLocalized = true;
                Debug.Log ("LOCALIZED");
            }
            else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING)
            {
                Debug.Log ("MAPPING");
            }
            else if (currStatus == LibPlacenote.MappingStatus.LOST)
            {
                OnLocalizationLostEvent ();
                IsLocalized = false;
                Debug.Log ("Searching for position lock");
            }
            else if (currStatus == LibPlacenote.MappingStatus.WAITING) {}
        }

        #endregion Placenote functions

        /// <summary>
        /// Sets local player info for starting game. Sets HasRoomStarted to true.
        /// Should be called when a player starts a game session.
        /// (Game session refers to the gameplay after the mapping and 
        /// localization is complete).
        /// Calls OnGameStartEvent.
        /// </summary>
        public void StartGame ()
        {
            Debug.Log ("Starting Game...");
            // Player is starting game, so increase count value
            TotalPlayersPlaying += 1;
            // Player is now playing game
            IsPlaying = true;
            // Update player count value for all players
            mPhotonView.RPC ("UpdatePlayerValueRPC", PhotonTargets.All);

            HasRoomStarted = true;

            // Broadcast start game event
            OnGameStartEvent ();
        }

        [PunRPC]
        public void UpdatePlayerValueRPC ()
        {
            // Update player count value
            OnPlayerValueUpdateEvent ();
        }

        #region ARKit 

        private void StartARKit ()
        {
            Application.targetFrameRate = 60;
            ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
            config.planeDetection = UnityARPlaneDetection.Horizontal;
            config.alignment = UnityARAlignment.UnityARAlignmentGravity;
            config.getPointCloudData = true;
            config.enableLightEstimation = true;
            mSession.RunWithConfig (config);
        }

        private void ARFrameUpdated (UnityARCamera camera)
        {
            mFrameUpdated = true;
            mARCamera = camera;
        }

        private void InitARFrameBuffer ()
        {
            mImage = new UnityARImageFrameData ();

            int yBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yHeight;
            mImage.y.data = Marshal.AllocHGlobal (yBufSize);
            mImage.y.width = (ulong)mARCamera.videoParams.yWidth;
            mImage.y.height = (ulong)mARCamera.videoParams.yHeight;
            mImage.y.stride = (ulong)mARCamera.videoParams.yWidth;

            // This does assume the YUV_NV21 format
            int vuBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yWidth / 2;
            mImage.vu.data = Marshal.AllocHGlobal (vuBufSize);
            mImage.vu.width = (ulong)mARCamera.videoParams.yWidth / 2;
            mImage.vu.height = (ulong)mARCamera.videoParams.yHeight / 2;
            mImage.vu.stride = (ulong)mARCamera.videoParams.yWidth;

            mSession.SetCapturePixelData (true, mImage.y.data, mImage.vu.data);
        }
#endregion ARKit
    }
}