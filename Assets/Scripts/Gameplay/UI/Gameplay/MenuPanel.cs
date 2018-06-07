using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Placenote;

namespace GameUiNs
{
    public class MenuPanel : PlacenotePunMultiplayerBehaviour
    {
        public Text LocalizedPlayers;
        public Text PlayersInRoom;
        public Text MaxPlayersAllowed;

        private void OnEnable ()
        {
            UpdatePlayerValues ();
        }

        override protected void OnPlayerValueUpdate ()
        {
            UpdatePlayerValues ();
        }

        private void UpdatePlayerValues ()
        {
            if (PhotonNetwork.connected)
            {
                LocalizedPlayers.text = "Players Playing: " +
                PlacenoteMultiplayerManager.Instance.
                TotalPlayersPlaying.ToString ();

                PlayersInRoom.text = "Players in Room: " +
                PlacenoteMultiplayerManager.Instance.
                TotalPlayersInRoom.ToString ();

                MaxPlayersAllowed.text = "Max Players Allowed: " +
                PlacenoteMultiplayerManager.Instance.
                MaxPlayersInRoom.ToString ();
            }
        }
    }
}
