using UnityEngine;
using UnityEngine.UI;
using Placenote;

namespace GameUiNs
{
    /// <summary>
    /// Gives user feedback during mapping session as host or as regular player.
    /// Calls LoadLatestMap because contains functions for feedback callbacks.
    /// </summary>
    public class MappingText : PlacenotePunMultiplayerBehaviour
    {
        private Text mappingText;

        private void Awake ()
        {
            mappingText = gameObject.GetComponent<Text> ();
        }

        private void OnEnable ()
        {
            if (PlacenoteMultiplayerManager.Instance.IsHost)
                mappingText.text = "Preparing to map...";
            else
                mappingText.text = "Wait while host maps the area.";
        }

        protected override void OnMappingStart ()
        { 
            if (PlacenoteMultiplayerManager.Instance.IsHost)
                mappingText.text = "Move your phone around to create a map.";
        }

        protected override void OnMappingFailed ()
        {
            mappingText.text = "Failed to start mapping session. Please restart app...";
        }

        #region Mapping callbacks

        public void MapSavingProgress (float progress)
        {
            mappingText.text = "Saving Progress..." + (progress * 100f) + "%";
        }

        public void MapSavingStatus (bool savingResult)
        {
            mappingText.text = "Saving " + (savingResult ? "success!" : "Error!");
        }

        public void MapLoadingProgress (float progress)
        {
            mappingText.text = "Loading Map..." + (progress * 100f) + "%";
        }

        public void MapLoadingStatus (bool loadingResult)
        {
            mappingText.text = "Loading " + (loadingResult ? "success!" : "Error!");
        }

        #endregion Mapping callbacks

        // When mapping is complete all current players should being loading the map
        protected override void OnMappingComplete ()
        {
            GetComponent<PhotonView> ().RPC ("LoadMapRPC", PhotonTargets.All);
        }

        // If player joins an already mapped room, then begin loading the map
        protected override void OnJoinedMappedRoom ()
        {
            PlacenoteMultiplayerManager.Instance.LoadLatestMap (MapLoadingStatus, MapLoadingProgress);
        }

        [PunRPC]
        public void LoadMapRPC ()
        {
            PlacenoteMultiplayerManager.Instance.LoadLatestMap (MapLoadingStatus, MapLoadingProgress);
        }

        protected override void OnLocalizationLost ()
        {
            mappingText.text = "Move and look to where the map was created to localize.";
        }

        protected override void OnLocalized ()
        {
            if (PlacenoteMultiplayerManager.Instance.IsHost || PlacenoteMultiplayerManager.Instance.HasRoomStarted)
                mappingText.text = "Localized! Press the button to start the game";
            else
                mappingText.text = "Localized! Wait for host to start the game";
        }
    }
}