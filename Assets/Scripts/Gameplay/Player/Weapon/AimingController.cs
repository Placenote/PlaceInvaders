using GameplayNs;
using PlayerNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponNs
{
    [System.Serializable]
    public class AimingProperties
    {
        public float MaxDistance = 10;
        public LayerMask ShotableItems;

        public void CheckAndFix ()
        {
            if (MaxDistance < 0.01)
                MaxDistance = 20;
            if (ShotableItems.value == 0)
                ShotableItems.value = 1;
            Debug.Log ("Layer id: " + ShotableItems.value);
        }
    }

    public class AimingData
    {
        public Ray AimingRay;
        public RaycastHit HitOfRay;
        public bool IsHit;
        public Vector3 HitPoint;
    }

    [RequireComponent (typeof (WeaponController))]
    public class AimingController : MonoBehaviour
    {
        public float CoroutineTimeout = 0.1f;
        AimingProperties props;
        AimingData aimData;

        WeaponController weaponController;
        public Transform LaserPointer;

        PlayerController player;

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

        void Start ()
        {
            player = GetComponentInParent<PlayerController> ();
            weaponController = GetComponent<WeaponController> ();
            player.SwitchWeapon (weaponController);
            props = weaponController.AimingProps;
            aimData = weaponController.AimData;
        }

        private void OnDisable ()
        {
            isCoroutineRunning = false;
        }

        bool isCoroutineRunning = false;

        void UpdateAimData ()
        {
            aimData.AimingRay = player.GetAimingRay ();
            aimData.IsHit = Physics.Raycast (aimData.AimingRay, out aimData.HitOfRay, props.MaxDistance, props.ShotableItems, QueryTriggerInteraction.Collide);
            aimData.HitPoint = (aimData.IsHit ? aimData.HitOfRay.point : aimData.AimingRay.GetPoint (props.MaxDistance));
        }

        void UpdateLaserPointer ()
        {
            if (LaserPointer != null)
            {
                LaserPointer.position = aimData.HitPoint;
                if (!aimData.IsHit)
                    LaserPointer.LookAt (player.GetPlayerEyesWorldPos ());
                else
                {
                    LaserPointer.rotation = Quaternion.FromToRotation (Vector3.forward, aimData.HitOfRay.normal);
                }
            }
        }

        IEnumerator UpdateAimingData ()
        {
            isCoroutineRunning = true;
            yield return null;
            while (isCoroutineRunning)
            {
                UpdateAimData ();
                UpdateLaserPointer ();
                Debug.DrawLine (player.GetPlayerEyesWorldPos (), aimData.HitPoint, Color.red, 0.5f * CoroutineTimeout);
                yield return new WaitForSeconds (CoroutineTimeout);
            }
        }

        private float mAimTurnRotate = 60;

        void LateUpdate ()
        {
            if (!player.IsLocalPlayer && !player.IsCurrentNetworkPlayer)
            {
                isCoroutineRunning = false;
                return;
            }

            if (!isCoroutineRunning)
                StartCoroutine (UpdateAimingData ());
            else
            {
                Transform gun = weaponController.GunObject.transform;
                Vector3 relativePos = aimData.HitPoint - gun.position;
                Quaternion targetRotation = Quaternion.LookRotation (relativePos);
                targetRotation = Quaternion.RotateTowards (gun.rotation, targetRotation, mAimTurnRotate * Time.deltaTime);

                gun.rotation = targetRotation;
            }
        }
    }
}