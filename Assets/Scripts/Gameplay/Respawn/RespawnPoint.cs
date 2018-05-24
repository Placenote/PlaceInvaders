using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemiesNs
{
    public class RespawnPoint : MonoBehaviour
    {
        private float TimeBetweenSpawns = 8;

        void Start ()
        {
            GameController.RegisterRespawnPoint (this);
            StartCoroutine (Creation ());
        }

        IEnumerator Creation ()
        {
            yield return null;
            while (true)
            {
                if (GameController.Data.GameState == GameStateId.GamePlaying)
                    GameController.CreateRandomEnemy ();
                yield return new WaitForSeconds (TimeBetweenSpawns);
            }
        }

        bool DoRestart = true;
        private void OnDisable ()
        {
            StopCoroutine (Creation ());
            DoRestart = false;
        }

        private void OnEnable ()
        {
            DoRestart = true;
        }

        void Update ()
        {
            if (DoRestart && GameController.IsGamePlaying)
            {
                DoRestart = false;
                StartCoroutine (Creation ());
            }
        }
    }
}