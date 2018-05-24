using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameplayNs
{
    public class ExplosionsDestroyer : MonoBehaviour
    {
        // allows a particle system to exist for a specified duration,
        // then shuts off emission, and waits for all particles to expire
        // before destroying the gameObject

        public float minDuration = 8;
        public float maxDuration = 10;

        private float m_MaxLifetime;
        private bool m_EarlyStop;

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

        private void Start ()
        {
            StartCoroutine (TimerCoroutine ());
        }

        public void RestartSelfDestruction ()
        {
            StopAllCoroutines ();
            StartCoroutine (TimerCoroutine ());
        }

        IEnumerator TimerCoroutine ()
        {
            var systems = GetComponentsInChildren<ParticleSystem> ();
            var audio = GetComponentInChildren<AudioSource> ();
            if (audio != null)
                audio.Play ();

            // find out the maximum lifetime of any particles in this effect
            foreach (var system in systems)
            {
                m_MaxLifetime = Mathf.Max (system.main.startLifetime.constant, m_MaxLifetime);
            }

            // wait for random duration

            float stopTime = Time.time + Random.Range (minDuration, maxDuration);

            while (Time.time < stopTime && !m_EarlyStop)
            {
                yield return null;
            }

            // turn off emission
            foreach (var system in systems)
            {
                var emission = system.emission;
                emission.enabled = false;
            }
            BroadcastMessage ("Extinguish", SendMessageOptions.DontRequireReceiver);

            // wait for any remaining particles to expire
            yield return new WaitForSeconds (m_MaxLifetime);

            if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                Destroy (gameObject);
            else if (photonView.isMine)
                PhotonNetwork.Destroy (gameObject);

        }

        public void DoEarlyStop ()
        {
            // stops the particle system early
            m_EarlyStop = true;
        }
    }
}
