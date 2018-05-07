using GameplayNs;
using GameUiNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponNs;

namespace PlayerNs
{
    public class PlayerController : EventsSubscriber
    {
        public Renderer PlayerCubeRenderer;
        
        public float FullHealth = 1;
        public float CurrentHealth = 1;
        PhotonView _thisPhotonView;
        public PhotonView photonView
        {
            get
            {
                if (_thisPhotonView == null)
                    _thisPhotonView = GetComponent<PhotonView>();
                return _thisPhotonView;
            }
        }
        public void SwitchWeapon(WeaponController newWeapon)
        {
            CurrentWeapon = newWeapon;
        }
        public WeaponController CurrentWeapon;
        Vector3 centralScreenPoint = new Vector3(0.5f, 0.5f, 0.0f);

        public bool IsLocalPlayer
        {
            get
            {

                return PhotonNetwork.offlineMode
                    || !PhotonNetwork.connected;
                    //|| photonView.isMine;

            }
        }

        public bool IsCurrentNetworkPlayer
        {
            get
            {
                return  photonView.isMine;
            }
        }

        
        public bool IsMe()
        {
            return PhotonNetwork.offlineMode
                    || !PhotonNetwork.connected
                    || photonView.isMine;
        }


        public Camera _aimingCamera;
        public Camera AimingCamera
        {
           get
           {
                if (_aimingCamera == null)
                    _aimingCamera = Camera.main;
                return _aimingCamera;

            }
  
        }
        
        
        public Vector3 GetPlayerEyesWorldPos()
        {

            // return AimingCamera.ViewportToWorldPoint(centralScreenPoint);
            return transform.position;

        }
        
        public Ray GetAimingRay()
        {
          //  return new Ray(GetPlayerEyesWorldPos(), transform.forward);
            return AimingCamera.ViewportPointToRay(centralScreenPoint);
        }

        public bool IsDead { get { return CurrentHealth <= 0; } }
        // Use this for initialization
        void Start()
        {
            CurrentHealth = FullHealth;
            transform.parent = GameController.WorldRootObject.transform;
            GameController.RegisterPlayer(this);

            if ((!PhotonNetwork.offlineMode || PhotonNetwork.connected) && photonView.isMine)
                photonView.RPC("SetCubeColor", PhotonTargets.All, Random.ColorHSV(0, 255, 0, 255, 0, 255, 200, 255));   
        }

        [PunRPC]
        private void SetCubeColor(Color color)
        {
            PlayerCubeRenderer.sharedMaterial.color = color;
        }
        
        protected override void Update()
        {
            base.Update();
        }

        public void Damage(float damageAmount)
        {
            if (CurrentHealth > 0)
            {
                if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                {
                    CurrentHealth -= damageAmount;
                }
                else
                    photonView.RPC("DecreaseHealth", PhotonTargets.All, damageAmount);
            }
        }

        [PunRPC]
        private void DecreaseHealth(float healDecreaseAmount)
        {
            CurrentHealth -= healDecreaseAmount;
            
            if (CurrentHealth < 0 && photonView.isMine)
            {
                Debug.Log("Player dead!!! ");
                GameController.Data.RegisterPlayerDeath();
            }
        }

        private void OnDestroy()
        {
            GameController.RemovePlayer(this);
        }
        override protected void NotifySomethingHappened(GameData.SomethingId id)
        {
            switch (id)
            {

                case GameData.SomethingId.PlayerResurrected:
                    CurrentHealth = FullHealth;
                    Debug.Log("Player ressurected!");
                    break;

            }
        }


    }
}
