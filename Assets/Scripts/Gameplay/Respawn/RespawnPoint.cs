using GameplayNs;
using System.Collections;
using UnityEngine;
using System;

namespace EnemiesNs
{
    public class RespawnPoint : EventsSubscriber
    {
        private float mTimeBetweenSpawns;
        // The number of spawning rounds per wave
        private const int ROUNDS_OF_SPAWN = 4;
        private const int TOTAL_ENEMIES_PER_PLAYER = 4;

        void Start ()
        {
            GameController.RegisterRespawnPoint (this);
            mTimeBetweenSpawns = 8f;
        }

        /// <summary>
        /// Spawns enemies for a set amount of rounds. Calls onWaveFinishCb on completion.
        /// Invoked by StartNextWave ()
        /// </summary>
        IEnumerator WaveSpawning (float timeBetweenSpawns, int roundsOfSpawn, Action onWaveFinishCb)
        {
            for (int i = 0; i < roundsOfSpawn; i++)
            {
                // Creates enemy
                if (GameController.Data.TotalEnemies < TOTAL_ENEMIES_PER_PLAYER * GameController.Data.PlayersInRoom)
                {
                    // Create 1 enemy per player in the room
                    for (int j = 0; j < GameController.Data.PlayersInRoom; j++)
                        GameController.CreateRandomEnemy ();
                }

                // Wait before spawning again
                yield return new WaitForSecondsRealtime (timeBetweenSpawns);
            }
            onWaveFinishCb ();
        }

        /// <summary>
        /// Event Listener for GameData. Starts and stops spawning of enemies based on
        /// when game starts and stops.
        /// </summary>
        override protected void NotifySomethingHappened (GameData.SomethingId id)
        {
            switch (id)
            {
                case GameData.SomethingId.GameStart:
                    Debug.Log ("STARTING SPAWNING");
                    StartNextWave ();
                    break;
                case GameData.SomethingId.ToMainMenu:
                    Debug.Log ("STOPPING SPAWNING");
                    StopCoroutine (WaveSpawning (mTimeBetweenSpawns, ROUNDS_OF_SPAWN, StartNextWave));
                    break;
            }
        }

        /// <summary>
        /// Starts the next wave.
        /// This is passed as a callback to WaveSpawning, so that the next wave can start
        /// when the first is complete. Note the mtimeBetweenSpawns is decreased every wave.
        /// </summary>
        public void StartNextWave ()
        {
            Debug.Log ("STARTING NEXT WAVE ");
            StartCoroutine (WaveSpawning (mTimeBetweenSpawns, ROUNDS_OF_SPAWN, StartNextWave));
            mTimeBetweenSpawns--;
        }
    }
}
