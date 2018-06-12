using UnityEngine;
using UnityEngine.UI;
using Placenote;

namespace GameUiNs
{
    /// <summary>
    /// Controls screens for the Placenote mapping session.
    /// </summary>
    public class PNMMScreens : PlacenotePunMultiplayerBehaviour
    {
        // Child object references
        public GameObject FinishMappingBtn;
        public GameObject StartButton;
        public GameObject MappingTextPanel;

        private void OnEnable ()
        {
            FinishMappingBtn.SetActive (false);
            StartButton.SetActive (false);
            MappingTextPanel.SetActive (true);
        }

        protected override void OnMappingSufficient ()
        {
            // Only activate Finish mapping button for the host
            if (PlacenoteMultiplayerManager.Instance.IsHost)
                FinishMappingBtn.SetActive (true);
        }

        protected override void OnMappingFailed ()
        {
            FinishMappingBtn.SetActive (false);
        }

        protected override void OnLocalized ()
        {
            // Only show button if host, or the room is already plaing
            if (PlacenoteMultiplayerManager.Instance.IsHost || PlacenoteMultiplayerManager.Instance.HasRoomStarted)
                StartButton.SetActive (true);
        }

        // Hide button on Placenote LOST Status
        protected override void OnLocalizationLost ()
        {
            StartButton.SetActive (false);
        }
    } 
}