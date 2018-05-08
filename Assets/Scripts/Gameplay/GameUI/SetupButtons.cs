using System.Collections.Generic;
using System.Xml;
using GameplayNs;
using Placenote;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{

    public class SetupButtons : MonoBehaviour
    {
        public Text StatusText;
        public Button ServerBtn;
        public GameObject ServerPanel;

        public Button StartEnvironmentScaningBtn;
        public Button StartGameBtn;
        public GameObject PlacenoteShowPoints;
        public FeaturesVisualizer FeaturesVisualizer;

        public GameObject flag;
//        public List<GameObject> ActivateOnGameStart = new List<GameObject>();

        private bool isScanning = false;
        private bool isLocalized = false;

        private void Start()
        {
            ServerBtn.onClick.AddListener(ToggleServerUI);
            StartEnvironmentScaningBtn.onClick.AddListener(EnvironmentScanningClick);
            StartGameBtn.onClick.AddListener(OnStartGameClick);
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
        
        private void ToggleServerUI()
        {
            if (ServerPanel != null)
            {
                ServerPanel.SetActive(!ServerPanel.activeSelf);
                StartEnvironmentScaningBtn.gameObject.SetActive(!StartEnvironmentScaningBtn.gameObject.activeSelf);
                StartGameBtn.gameObject.SetActive(!StartGameBtn.gameObject.activeSelf);
            }
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
