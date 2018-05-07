using GameplayNs;
using PlayerNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponNs
{
    public class WeaponController : MonoBehaviour
    {

       
        [Header("Current Animated gun model")]
        public AnimatedGun GunObject;


        [Header("Setup Aiming")]
        [Tooltip("Special weapon camera if exists")]
        public AimingProperties AimingProps;

        public AimingData AimData;

        public GameObject ExplosionPrefab;

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

        private void Awake()
        {
            if (AimData == null)
                AimData = new AimingData();
        }

        PlayerController player;
        // Use this for initialization
        void Start()
        {

            if (GunObject == null)
                GunObject = GetComponentInChildren<AnimatedGun>();
            player = player = GetComponentInParent<PlayerController>();
            GunObject.SetAimData(AimData);
        }


        bool IsAlreadyShooting;
        IEnumerator Shooting()
        {
            if (IsAlreadyShooting)
                yield break;
            IsAlreadyShooting = true;
            do
            {
                DoShot();
                yield return new WaitForSeconds(GunObject.ShotDuration);

                if(IsShooting)
                    yield return new WaitForSeconds(GunObject.PauseBetweenShots);
             
            } while (IsShooting && GunObject.IsAutomaticFire);
            IsAlreadyShooting = false;
        }

        bool IsShooting;

        public void CallRPCShotTrigger(bool isPressed)
        {
            if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                ShotTrigger(isPressed);
            else if (photonView.isMine)
                   photonView.RPC("ShotTrigger", PhotonTargets.All, isPressed);
        }

        [PunRPC]
        public void ShotTrigger(bool isPressed)
        {
            IsShooting = isPressed;
        //    Debug.Log("ShotTrigger:" + IsShooting);
        if(IsShooting)
            StartCoroutine(Shooting());
        }

        /// <summary>
        /// Shooting, if ray on target do hit shot, else do missed shot
        /// </summary>
        public void DoShot()
        {
            //Debug.Log("Shot!!!");

           
           // Vector3 centralScreenPoint = new Vector3(0.5f, 0.5f, 0.0f);
   
            AimingProperties props = AimingProps;

            Ray ray = player.GetAimingRay();

            RaycastHit HitInfo;
            bool isHit = Physics.Raycast(ray, out HitInfo, props.MaxDistance, props.ShotableItems, QueryTriggerInteraction.Collide);
            
            if(isHit)
                GunObject.DoHitShot(HitInfo, props.MaxDistance);
            else 
                GunObject.DoMissedShot(ray.GetPoint(props.MaxDistance));

        }

        // Update is called once per frame
        void Update()
        {
           // Shot();
        }
    }
}
