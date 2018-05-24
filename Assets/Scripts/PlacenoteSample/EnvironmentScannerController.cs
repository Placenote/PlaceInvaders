using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using GameUiNs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

namespace Placenote
{
    [Serializable]
    public class PlacenoteStatusChangeEvent : UnityEvent<LibPlacenote.MappingStatus, LibPlacenote.MappingStatus> { }

    public class EnvironmentScannerController : MonoBehaviour, PlacenoteListener
    {
        // External class references.
        public GameUIController GameUI;

        public Text PlacenoteStatusText;
        public UnityEvent OnARKitInitialized = new UnityEvent ();
        public PlacenoteStatusChangeEvent OnPlacenoteStatusChange = new PlacenoteStatusChangeEvent ();

        public string LatestMapId;

        private UnityARSessionNativeInterface mSession;
        private bool mFrameUpdated = false;
        private UnityARImageFrameData mImage = null;
        private UnityARCamera mARCamera;
        private bool mARKitInit = false;

        #region Singleton

        public static EnvironmentScannerController Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<EnvironmentScannerController> ();

                if (instance == null)
                    instance = new GameObject ("EnvironmentScannerController").AddComponent<EnvironmentScannerController> ();

                return instance;
            }
        }
        private static EnvironmentScannerController instance = null;

        #endregion Singleton


        private void Start ()
        {
            if (GameUI == null)
                GameUI = FindObjectOfType<GameUIController> ();
            mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
            UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
            StartARKit ();
            FeaturesVisualizer.EnablePointcloud ();
            LibPlacenote.Instance.RegisterListener (this);
        }


        #region Mapping Start

        /// <summary>
        /// Starts environment scanning, return false if SDK not yet initialized,
        /// if scanning started return true
        /// </summary>
        /// <returns></returns>
        public bool StartMapping ()
        {
            if (!LibPlacenote.Instance.Initialized ())
            {
                Debug.Log ("SDK not yet initialized");
                return false;
            }

            if (PhotonNetwork.connected)
            {
                GetComponent<PhotonView> ().RPC ("MappingStarted", PhotonTargets.Others);
            }

            LibPlacenote.Instance.StartSession ();
            return true;
        }

        [PunRPC]
        private void MappingStarted ()
        {
            GameUI.HelperText.text = "Wait while host maps the area!";
        }

        /// <summary>
        /// Complete the scan of the environment, save the scanned map, and synchronize this map with all clients
        /// </summary>
        /// <param name="onSavingFinish"></param>
        /// <param name="onSavingProgress"></param>
        public void FinishMapping (Action<bool> onSavingFinish, Action<float> onSavingProgress)
        {
            LibPlacenote.Instance.SaveMap ((mapId) =>
            {
                PlacenoteStatusText.text = "Set Latest Map ID - " + mapId;

                if (PhotonNetwork.connected)
                {
                    ExitGames.Client.Photon.Hashtable mapIdValue = new ExitGames.Client.Photon.Hashtable ();
                    mapIdValue.Add ("MapId", mapId);
                    PhotonNetwork.room.SetCustomProperties (mapIdValue);
                    GetComponent<PhotonView> ().RPC ("SetLatestMapId", PhotonTargets.All, mapId);
                }
                else
                {
                    LatestMapId = mapId;
                }

            }, (completed, faulted, percentage) =>
             {
                 if (!completed && !faulted)
                 {
                     onSavingProgress.Invoke (percentage);
                 }
                 else
                 {
                     onSavingFinish.Invoke (!faulted);
                 }
             });
        }

        [PunRPC]
        public void SetLatestMapId (string id)
        {
            PlacenoteStatusText.text = "RPC or Cus. Prop. Set Map ID - " + id;
            LatestMapId = id;
        }

        #endregion Mapping start


        #region Mapping end

        /// <summary>
        /// Stop Placenote session
        /// </summary>
        public void StopUsingMap ()
        {
            FeaturesVisualizer.DisablePointcloud ();
            FeaturesVisualizer.clearPointcloud ();
            LibPlacenote.Instance.StopSession ();
        }

        #endregion Mapping end


        #region Mapping utility

        /// <summary>
        /// Load lasted scanned map, return false if SDK not yet initialized or map is null
        /// </summary>
        /// <param name="mapLoaded"></param>
        /// <param name="mapLoadingFail"></param>
        /// <param name="onLoadingPercentage"></param>
        /// <returns></returns>
        public bool LoadLatestMap (Action<string> mapLoaded, Action<string> mapLoadingFail, Action<float> onLoadingPercentage)
        {
            if (!LibPlacenote.Instance.Initialized ())
            {
                Debug.Log ("SDK not yet initialized");
                return false;
            }

            if (string.IsNullOrEmpty (LatestMapId))
            {
                Debug.Log ("Can't find Map!");
                return false;
            }

            LibPlacenote.Instance.LoadMap (LatestMapId, (completed, faulted, percentage) =>
             {
                 if (completed)
                 {
                     //LibPlacenote.Instance.StopSession ();
                     LibPlacenote.Instance.StartSession ();
                     mapLoaded.Invoke (LatestMapId);
                 }
                 else if (faulted)
                 {
                     mapLoadingFail.Invoke (LatestMapId);
                 }
                 else
                 {
                     onLoadingPercentage.Invoke (percentage);
                 }
             });

            return true;
        }

        /// <summary>
        /// Start deleting all scanned placenote maps and if maps.Length == 0 return true,
        /// if SDK not yet initialized return false
        /// </summary>
        /// <param name="onDeleteFinish"></param>
        /// <param name="onDeletingMap"></param>
        /// <returns></returns>
        public bool DeleteAllMaps (Action<bool> onDeleteFinish, Action<string> onDeletingMap)
        {
            if (!LibPlacenote.Instance.Initialized ())
            {
                Debug.Log ("SDK not yet initialized");
                return false;
            }

            LibPlacenote.Instance.ListMaps ((maps) =>
            {
                if (maps.Length == 0)
                {
                    onDeleteFinish.Invoke (true);
                    return;
                }

                var mapsCount = maps.Length;
                var deletedCount = 0f;
                var failToDelete = new List<LibPlacenote.MapInfo> ();

                foreach (var map in maps)
                {
                    onDeletingMap.Invoke (map.placeId);
                    LibPlacenote.Instance.DeleteMap (map.placeId, (deleted, errMsg) =>
                    {
                        if (deleted)
                        {
                            deletedCount++;
                        }
                        else
                        {
                            failToDelete.Add (map);
                        }

                        if (deletedCount == mapsCount)
                            onDeleteFinish.Invoke (true);
                        else if (deletedCount + failToDelete.Count == mapsCount)
                            onDeleteFinish.Invoke (false);
                    });
                }
            });
            return true;
        }

        /// <summary>
        ///  return false if SDK not yet initialized, else
        ///     return true and set loaded maps list into ibPlacenote.Instance.ListMaps;
        /// </summary>
        /// <param name="mapsLoaded"></param>
        /// <returns></returns>
        public bool GetMapsCount (Action<LibPlacenote.MapInfo[]> mapsLoaded)
        {
            if (!LibPlacenote.Instance.Initialized ())
            {
                Debug.Log ("SDK not yet initialized");
                return false;
            }
            LibPlacenote.Instance.ListMaps (mapsLoaded);
            return true;
        }

        #endregion Mapping utility


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

        void Update ()
        {
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

        public void OnPose (Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

        public void OnStatusChange (LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
        {
            OnPlacenoteStatusChange.Invoke (prevStatus, currStatus);

            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
            {
                Debug.Log ("RUNNING");
            }
            else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING)
            {
                Debug.Log ("Mapping");
            }
            else if (currStatus == LibPlacenote.MappingStatus.LOST)
            {
                Debug.Log ("Searching for position lock");
            }
            else if (currStatus == LibPlacenote.MappingStatus.WAITING)
            {
            }
        }
    }
}