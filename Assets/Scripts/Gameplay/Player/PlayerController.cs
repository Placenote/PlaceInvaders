using GameplayNs;
using UnityEngine;
using WeaponNs;

namespace PlayerNs
{
    public class PlayerController : EventsSubscriber
    {
        public WeaponController CurrentWeapon;
        public GameSetupController GameSetup;
        Vector3 centralScreenPoint = new Vector3 (0.5f, 0.5f, 0.0f);

        public float FullHealth = 1;
        public float CurrentHealth = 1;
        PhotonView _thisPhotonView;

        public PhotonView photonView
        {
            get
            {
                if (_thisPhotonView == null)
                    _thisPhotonView = GetComponent<PhotonView> ();
                return _thisPhotonView;
            }
        }

        public void SwitchWeapon (WeaponController newWeapon)
        {
            CurrentWeapon = newWeapon;
        }

        public bool IsLocalPlayer
        {
            get
            {
                return PhotonNetwork.offlineMode
                    || !PhotonNetwork.connected;
            }
        }

        public bool IsCurrentNetworkPlayer
        {
            get
            {
                return photonView.isMine;
            }
        }

        public bool IsMe ()
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

        public Vector3 GetPlayerEyesWorldPos ()
        {
            return transform.position;
        }

        public Ray GetAimingRay ()
        {
            return AimingCamera.ViewportPointToRay (centralScreenPoint);
        }

        public bool IsDead { get { return CurrentHealth <= 0; } }

        void Start ()
        {
            CurrentHealth = FullHealth;
            transform.parent = GameController.WorldRootObject.transform;
            GameController.RegisterPlayer (this);
            gameObject.transform.GetChild (0).gameObject.SetActive (false);

            if (GameSetup == null)
                GameSetup = FindObjectOfType<GameSetupController> ();
        }

        private bool activateGun = true;
        protected override void Update ()
        {
            base.Update ();
            if (activateGun)
            {
                if (GameController.IsGamePlaying)
                {
                    activateGun = false;
                    gameObject.transform.GetChild (0).gameObject.SetActive (true);
                }
            }
        }

        public void Damage (float damageAmount)
        {
            if (CurrentHealth > 0)
            {
                if (!PhotonNetwork.connected)
                {

                    CurrentHealth -= damageAmount;
                    GameController.Data.RegisterPlayerDeath ();


                }
                else
                {
                    photonView.RPC ("DecreaseHealth", PhotonTargets.All, damageAmount);
                }

            }
        }

        [PunRPC]
        private void DecreaseHealth (float healDecreaseAmount)
        {
            CurrentHealth -= healDecreaseAmount;
            if (CurrentHealth < 0 && photonView.isMine && GameSetup.IsLocalized == true)
            {
                Debug.Log ("Player dead!!! ");
                GameController.Data.RegisterPlayerDeath ();
            }
        }

        private void OnDestroy ()
        {
            GameController.RemovePlayer (this);
        }

        override protected void NotifySomethingHappened (GameData.SomethingId id)
        {
            switch (id)
            {
                case GameData.SomethingId.PlayerResurrected:
                    CurrentHealth = FullHealth;
                    Debug.Log ("Player ressurected!");
                    break;
            }
        }
    }
}