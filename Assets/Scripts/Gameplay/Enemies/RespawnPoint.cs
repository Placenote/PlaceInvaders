using GameplayNs;
using System.Collections;
using UnityEngine;
using System;
using Placenote;

namespace EnemiesNs
{
    public class RespawnPoint : GameEventsSubscriber
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
                // Only MasterClient/Host can use Photon Instantiate
                if (PhotonNetwork.isMasterClient)
                {
                    // Create 1 enemy per player playing
                    for (int j = 0; j < PlacenoteMultiplayerManager.Instance.TotalPlayersPlaying; j++)
                    {
                        if (GameController.Instance.Data.TotalEnemies < TOTAL_ENEMIES_PER_PLAYER
                            * PlacenoteMultiplayerManager.Instance.TotalPlayersPlaying)
                        {
                            GameController.Instance.CreateRandomEnemyAtPoint ();
                        }
                    }
                }
                else
                {
                    Debug.Log ("Only master client can spawn enemies...");
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
        override protected void OnGameEvent (GameData.SomethingId id)
        {
            switch (id)
            {
                case GameData.SomethingId.GameStart:
                    // The room must be in playing status to start spawning 
                    mTimeBetweenSpawns = 8f;
                    if (PlacenoteMultiplayerManager.Instance.HasRoomStarted)
                    {
                        Debug.Log ("STARTING SPAWNING");
                        StartNextWave ();
                    }
                    else{
                        Debug.Log ("ROOM ALREADY STARTED NOT SPAWNING MORE");
                    }
                    break;
                case GameData.SomethingId.ToMainMenu:
                    if (PlacenoteMultiplayerManager.Instance.HasRoomStarted)
                    {
                        Debug.Log ("STOPPING SPAWNING");
                        StopCoroutine (WaveSpawning (mTimeBetweenSpawns, ROUNDS_OF_SPAWN, StartNextWave));
                    }
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
            if (mTimeBetweenSpawns <= 3f)
                mTimeBetweenSpawns = 3f;
        }
    }
}
