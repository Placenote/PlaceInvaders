using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerNs
{
    public class PlayerPhotonGenerator : Photon.PunBehaviour
    {
        public GameObject PlayerPrefab;
        PlayerController Player;
        // Use this for initialization
        void Start()
        {

        }

        virtual protected void OnGameStart()
        {
            GameObject player = null;
            if (Player != null)
            {
                //GameController.RemovePlayer(Player);
                Destroy(Player.gameObject);
            }
            if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                player = Instantiate<GameObject>(PlayerPrefab);
            else
                player = PhotonNetwork.Instantiate(PlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);

            if (!string.IsNullOrEmpty(PhotonNetwork.playerName))
            {
                player.name = PhotonNetwork.playerName;
            }

            Player = player.GetComponent<PlayerController>();

        }

        public override void OnJoinedRoom()
        {
            if (GameController.IsGamePlaying)
            {
                OnGameStart();
            }
        }

        public override void OnDisconnectedFromPhoton()
        {
            if (GameController.IsGamePlaying)
            {
                OnGameStart();
            }
        }



        #region event handlers to override



        virtual protected void NotifySomethingHappened(GameData.SomethingId id)
        {
            if (id == GameData.SomethingId.GameStart)
                OnGameStart();
            else if (id == GameData.SomethingId.GameOver && Player != null)
            {
                if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                    Destroy(Player.gameObject);
                else
                    Network.Destroy(Player.gameObject);
            }


        }
        #endregion event handlers to override


        #region Subscribing to events

        bool doSubscibe = true;


        private void OnEnable()
        {
            doSubscibe = true;
        }
        private void OnDisable()
        {
            if (GameController.Data != null)
                GameController.Data.NotifySomethingHappened -= NotifySomethingHappened;
        }

        protected virtual void Subscribe()
        {
            GameController.Data.NotifySomethingHappened += NotifySomethingHappened;
        }


        void Update()
        {
            if (doSubscibe)
            {
                doSubscibe = false;
                Subscribe();

            }
        }
        #endregion Subscribing to events
    }
}
