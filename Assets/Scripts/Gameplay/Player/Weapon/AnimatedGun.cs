using DigitalRuby.LightningBolt;
using GameplayNs;
using System.Collections;
using UnityEngine;

namespace WeaponNs
{
    public class AnimatedGun : MonoBehaviour
    {
        public Transform Muzzle;
        [Range (0, 1)]
        public float DamageAmount = 0.5f;

        public bool IsAutomaticFire = true;

        [Tooltip ("Time between shots")]
        public float PauseBetweenShots = 0.5f;

        [Tooltip ("Time between shots")]
        public float ShotDuration = 0.5f;

        public GameObject Blast;

        [Tooltip ("Third party component used by this script")]
        public LightningBoltScript lightning;
        /// <summary>
        /// required by LightningBoltScript 
        /// </summary>
        LineRenderer lineRenderer;
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

        public void DoHitShot (RaycastHit hit, float maxDistance)
        {
            MakeDamage (hit, DamageAmount);
            AnimateShot ();
            Debug.DrawLine (Muzzle.position, hit.point, Color.yellow, 0.5f);
        }

        public void DoMissedShot (Vector3 targetPosition)
        {
            AnimateShot ();
            Debug.DrawLine (Muzzle.position, targetPosition, Color.blue, 0.5f);
        }

        void AnimateShot ()
        {
            StartCoroutine (AnimateShotCoroutine ());
        }

        void MakeDamage (RaycastHit hit, float DamageAmount)
        {
            if ((PhotonNetwork.offlineMode || !PhotonNetwork.connected) || photonView.isMine)
            {
                // TODO If in preparing game send message that is target
                if (GameController.Instance.Data.GameState == GameStateId.GameWaitingStart)
                    hit.transform.SendMessage ("Hit", SendMessageOptions.DontRequireReceiver);
                // TODO if in game send message that is enemy
                else
                    hit.transform.SendMessage (GameController.Instance.EnemyDamageReceiverName, DamageAmount, SendMessageOptions.DontRequireReceiver);
            }
        }

        bool IsMakingSecondaryDamageNow = false;
        IEnumerator AnimateShotCoroutine ()
        {
            yield return null;
            lightning.enabled = true;
            yield return null;
            yield return new WaitForEndOfFrame ();
            IsMakingSecondaryDamageNow = true;
            lineRenderer.enabled = true;
            audioSource.Play ();
            yield return new WaitForSeconds (ShotDuration);
            IsMakingSecondaryDamageNow = false;
            lineRenderer.enabled = false;
            lightning.enabled = false;
        }

        void Start ()
        {
            Init ();
        }

        AudioSource audioSource;
        private void Init ()
        {
            if (lightning == null)
                lightning = GetComponentInChildren<LightningBoltScript> ();

            lineRenderer = lightning.GetComponent<LineRenderer> ();
            lineRenderer.enabled = false;
            lightning.enabled = false;
            audioSource = GetComponent<AudioSource> ();
        }

        AimingData AimData;
        public void SetAimData (AimingData newAimData)
        {
            AimData = newAimData;
        }

        GameObject activeBlast;
        void ShowHit (Vector3 point)
        {
            if (activeBlast == null)
            {
                if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                {
                    activeBlast = Instantiate<GameObject> (Blast, GameController.WorldRootObject.transform);
                }
                else
                {
                    activeBlast = PhotonNetwork.Instantiate (Blast.name, point, Quaternion.identity, 0);
                }
                activeBlast.transform.parent = GameController.WorldRootObject.transform;
            }
            else
            {
                activeBlast.SendMessage ("RestartSelfDestruction", SendMessageOptions.DontRequireReceiver);
            }
            activeBlast.transform.SetPositionAndRotation (point, Quaternion.identity);
        }

        void Update ()
        {
            if (IsMakingSecondaryDamageNow)
            {
                if (AimData.IsHit
                    && AimData.HitOfRay.transform != null
                    && AimData.HitOfRay.transform.CompareTag (GameController.Instance.EnemyTag))
                {
                    ShowHit (AimData.HitOfRay.point);
                    MakeDamage (AimData.HitOfRay, DamageAmount * Time.deltaTime);
                }
            }
        }
    }
}
