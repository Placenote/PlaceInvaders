using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemiesNs
{
    public class RespawnPoint : MonoBehaviour
    {
        public int Id;
        public float TimeOut = 5;
        // Use this for initialization
        
        void Start()
        {
            /*if (Id == null)
                Id = gameObject.name;
                */
            GameController.RegisterRespawnPoint(this);
            StartCoroutine(Creation());
        }

        IEnumerator Creation()
        {
            yield return null;
            while (true)
            {
                yield return new WaitForSeconds(TimeOut);
                if (GameController.Data.GameState == GameStateId.GamePlaying)
                    GameController.CreateRandomEnemy();
            }
        }

        bool DoRestart = true;
        private void OnDisable()
        {
            StopCoroutine(Creation());
            DoRestart = false;
        }

        private void OnEnable()
        {
            DoRestart = true;
        }
        // Update is called once per frame
        void Update()
        {
            if (DoRestart && GameController.IsGamePlaying)
            {
                DoRestart = false;
                StartCoroutine(Creation());
            }
        }
    }
}
