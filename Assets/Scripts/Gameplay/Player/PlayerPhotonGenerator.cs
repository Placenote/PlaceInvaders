using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerNs
{
    public class PlayerPhotonGenerator : GameEventsSubscriber
    {
        public GameObject PlayerPrefab;
        PlayerController Player;

        private void OnGameStart ()
        {
            GameObject player = null;
            if (Player != null)
            {
                Destroy (Player.gameObject);
            }
            player = PhotonNetwork.Instantiate (PlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);
            if (!string.IsNullOrEmpty (PhotonNetwork.playerName))
            {
                player.name = PhotonNetwork.playerName;
            }
            Player = player.GetComponent<PlayerController> ();
        }

        #region event handlers to override

        protected override void OnGameEvent (GameData.SomethingId id)
        {
            if (id == GameData.SomethingId.GameStart)
                OnGameStart ();
            else if (id == GameData.SomethingId.GameOver && Player != null)
            {
                if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                    Destroy (Player.gameObject);
                else
                    Network.Destroy (Player.gameObject);
            }
            else if (id == GameData.SomethingId.ToMainMenu && Player != null)
            {
                if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                {
                    Destroy (Player.gameObject);
                }
                else
                {
                    GameController.Instance.RemovePlayer (Player);
                    PhotonNetwork.Destroy (Player.gameObject);
                }
            }
        }
        #endregion event handlers to override
    }
}