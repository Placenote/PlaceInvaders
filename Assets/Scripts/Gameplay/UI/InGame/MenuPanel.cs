using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
    public class MenuPanel : EventsSubscriber
    {
        public Text LocalizedPlayers;
        public Text PlayersInRoom;
        public Text MaxPlayersAllowed;

        protected override void OnEnable ()
        {
            base.OnEnable ();
            if (PhotonNetwork.connected)
            {
                base.OnEnable ();
                LocalizedPlayers.text = "Localized Players: " + GameController.Data.LocalizedPlayers.ToString ();
                PlayersInRoom.text = "Players in Room: " + GameController.Data.PlayersInRoom.ToString ();
                MaxPlayersAllowed.text = "Max Players Allowed: " + GameController.Data.MaxPlayersAllowed.ToString ();
            }
            else
            {
                LocalizedPlayers.text = "";
                PlayersInRoom.text = "";
                MaxPlayersAllowed.text = "";
            }
        }

        override protected void NotifySomeDataChanged ()
        {
            if (PhotonNetwork.connected)
            {
                LocalizedPlayers.text = "Localized Players: " + GameController.Data.LocalizedPlayers.ToString ();
                PlayersInRoom.text = "Players in Room: " + GameController.Data.PlayersInRoom.ToString ();
                MaxPlayersAllowed.text = "Max Players Allowed: " + GameController.Data.MaxPlayersAllowed.ToString ();
            }
        }

        protected override void Update ()
        {
            base.Update ();
        }
    }
}