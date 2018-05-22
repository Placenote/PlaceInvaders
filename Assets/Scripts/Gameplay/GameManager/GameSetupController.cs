using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUiNs;
using Placenote;
using PunServerNs;

namespace GameplayNs
{
    public class GameSetupController : MonoBehaviour
    {
        // External class references.
        public SrvController Srv;
        public GameUIController GameUI;

        // Placenote things
        public GameObject PlacenoteShowPoints;
        public FeaturesVisualizer FeaturesVisualizer;
        public GameObject flag; // TODO Update flag for use

        public bool isLocalized { get; private set; }

        private void Start()
        {
            if (Srv == null)
                Srv = FindObjectOfType<SrvController>();
            if (GameUI == null)
                GameUI = FindObjectOfType<GameUIController>();

            isLocalized = false;
        }


        #region Environment Mapping

        public void EnvironmentMappingStart()
        {
            PlacenoteShowPoints.SetActive(true);
            GameController.PrepareGame();
            // Start mapping
            var startScan = EnvironmentScannerController.Instance.StartMapping();
            GameUI.HelperText.text = "Mapping Started! Move your camera around to create a map.";
            if (startScan)
                FeaturesVisualizer.EnablePointcloud();
        }

        public void EnvironmentMappingStop()
        {
            if (PhotonNetwork.connected)
            {
                if (Srv.IsHost)
                {
                    // Room changes its status to localizing
                    Srv.CurrRoomStatus = SrvController.RoomStatus.Localizing;
                }
            }
            EnvironmentScannerController.Instance.FinishMapping(EnvironmentMappingComplete, EnvironmentMappingProgress);
        }

        private void EnvironmentMappingProgress(float progress)
        {
            GameUI.HelperText.text = "Saving Progress..." + (progress * 100f) + "%";
        }

        private void EnvironmentMappingComplete(bool scanningRes)
        {
            GameUI.HelperText.text = "Saving " + (scanningRes ? "success!" : "Error!");

            if (PhotonNetwork.connected)
                GetComponent<PhotonView>().RPC("EnvironmentMappingCompleteRPC", PhotonTargets.Others);
            FeaturesVisualizer.DisablePointcloud();
            FeaturesVisualizer.clearPointcloud();
            StartLoadingMap();
        }

        [PunRPC]
        private void EnvironmentMappingCompleteRPC()
        {
            StartLoadingMap();
        }

        #endregion Environment Mapping


        #region Loading Map

        public void StartLoadingMap()
        {
            if (string.IsNullOrEmpty(EnvironmentScannerController.Instance.LatestMapId))
            {
                GameUI.HelperText.text = "Error! Can't find Environment Scan!";
                return;
            }
            var res = EnvironmentScannerController.Instance.LoadLatestMap(LatestMapLoaded, MapLoadingFail, MapLoadingPercentage);
        }

        private void LatestMapLoaded(string mapId)
        {
            GameUI.HelperText.text = "Map Loaded Succesfully! Move to spot to localize";
            EnvironmentScannerController.Instance.OnPlacenoteStatusChange.AddListener(PlacenoteStatusChange);
        }

        private void MapLoadingFail(string mapId)
        {
            GameUI.HelperText.text = "Map loading fail! Quit to main menu and try again.";
        }

        private void MapLoadingPercentage(float percentage)
        {
            GameUI.HelperText.text = "Loading Map..." + (percentage * 100f) + "%";
        }

        #endregion Loading Map


        #region Placenote events

        public void PlacenoteStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
        {
            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
            {
                isLocalized = true;
                if (PhotonNetwork.connected)
                {
                    Srv.TotalLocalizedPlayers = Srv.TotalLocalizedPlayers + 1;
                    GameController.Data.UpdatePlayerAmounts();
                    // If user is the host they can start the game once localized.
                    if (PhotonNetwork.isMasterClient)
                    {
                        GameUI.HelperText.text = "Localized! Press the button to start the game";
                        GameUI.StartGameBtn.gameObject.SetActive(true);
                    }
                    // If the user is not the host the game will start automatically after localization
                    // or it will start once the host starts
                    else
                    {
                        if (Srv.CurrRoomStatus != SrvController.RoomStatus.Playing)
                            GameUI.HelperText.text = "Localized! Wait for the host to start the game";
                        else
                        {
                            GameUI.HelperText.text = "Game start!";
                            GameController.StartGame();
                        }

                    }
                }
                else
                {
                    GameUI.HelperText.text = "Localized! Press the button to start the game";
                    GameUI.StartGameBtn.gameObject.SetActive(true);
                }
                PlacenoteShowPoints.SetActive(false);
                EnvironmentScannerController.Instance.OnPlacenoteStatusChange.RemoveListener(PlacenoteStatusChange);
            }
            else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING)
            {
                GameUI.HelperText.text = "Mapping";
            }
            else if (currStatus == LibPlacenote.MappingStatus.LOST)
            {
                GameUI.HelperText.text = "Move and look to where the map was created to localize.";
            }
            else if (currStatus == LibPlacenote.MappingStatus.WAITING)
            {
            }
        }

        #endregion Placenote events


        #region Game events 

        public void QuitGame()
        {
            Debug.Log("Removing Listener from OnPlacenoteStatusChange...");
            EnvironmentScannerController.Instance.OnPlacenoteStatusChange.RemoveListener(PlacenoteStatusChange);
        }

        public void StartGame()
        {
            if (PhotonNetwork.connected)
            {
                // Room changes its status to playing
                Srv.CurrRoomStatus = SrvController.RoomStatus.Playing;
                GetComponent<PhotonView>().RPC("StartGameRPC", PhotonTargets.Others);
            }

            GameUI.HelperText.text = "Game start!"; // TODO fix this so that ui is being handled in the controller
            GameController.StartGame();
            GameUI.StartGameBtn.gameObject.SetActive(false);
        }

        [PunRPC]
        private void StartGameRPC()
        {
            if (isLocalized)
            {
                GameUI.HelperText.text = "Game start!";
                GameController.StartGame();
            }
            else
            {
                GameUI.HelperText.text = "The game has started. You need to localize before you can play!";
            }
        }

        #endregion Game events
    }
}
