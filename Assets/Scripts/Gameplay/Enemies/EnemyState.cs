using GameplayNs;
using PlayerNs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemiesNs
{
    public class EnemyState : Photon.PunBehaviour, IPunObservable
    {
        public float CurrentHealth = 1f;
        public GameObject DeathPrefab;

        public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
        {
            

            if (stream.isWriting) {
            	stream.SendNext (CurrentHealth);
            } else {
                this.CurrentHealth = (float) stream.ReceiveNext ();
            }
        }

        private void Update ()
        {
            if (!(CurrentHealth < 0)) return;
            
            
            if (DeathPrefab != null)
                if (PhotonNetwork.connected)
                    PhotonNetwork.Instantiate ("BigExplosion", transform.position, transform.rotation, 0);
                else
                    Instantiate (DeathPrefab, transform.position, transform.rotation);

            Destroy (gameObject);
        }

        public void Damage (float damage)
        {
            if (CurrentHealth - damage < 0) {
                if (0 <= CurrentHealth)
                    GameController.Data.Kills++;
            }

            if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                DecreseHealth (damage);
            else
                photonView.RPC ("DecreseHealth", PhotonTargets.All, damage);
        }

        [PunRPC]
        private void DecreseHealth (float healthDecreaseVal)
        {
            CurrentHealth -= healthDecreaseVal;
        }
    }
}
