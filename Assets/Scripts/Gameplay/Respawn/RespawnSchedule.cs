using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RespawnNs
{



    public class RespawnSchedule : MonoBehaviour
    {
        public RespawnConditions Schedule;
        public float TimeOutToCheck;
        public float TimelineDeltaTime = 1;

        public List<RespawnCondition> Conditions;
        public int CurrentCondition;

        // Use this for initialization
        
        void Start()
        {
            StartCoroutine(TimelineCoroutine());
        }

        

        IEnumerator TimelineCoroutine()
        {
            yield return null;
            while (true)
            {
                yield return new WaitForSeconds(TimelineDeltaTime);
                if (GameController.Data.GameState == GameStateId.GamePlaying)
                {
                    //GameController.CreateRandomEnemyAtPoint(Id);
                }
                    
            }
        }

        bool DoRestart = true;
        private void OnDisable()
        {
            StopCoroutine(TimelineCoroutine());
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
                StartCoroutine(TimelineCoroutine());
            }
        }
    }
}
