using GameplayNs;
using System.Collections;
using UnityEngine;

namespace EnemiesNs
{
    public class RespawnPoint : EventsSubscriber
    {
        private float TimeBetweenSpawns = 8f;
        private int LoopsUntilSpawnDecrease = 3;

        void Start ()
        {
            GameController.RegisterRespawnPoint (this);
        }

        IEnumerator Creation ()
        {
            yield return null;
            while (GameController.Data.GameState == GameStateId.GamePlaying)
            {
                // Creates enemy
                if (GameController.Data.GameState == GameStateId.GamePlaying)
                {
                    // Create 1 enemy per player in the room
                    for (int i = 0; i < GameController.Data.PlayersInRoom; i++)
                        GameController.CreateRandomEnemy ();
                    LoopsUntilSpawnDecrease--;
                }

                // Decrease time betweens spawning as the game goes own
                yield return new WaitForSecondsRealtime (TimeBetweenSpawns);
                if (LoopsUntilSpawnDecrease <= 0)
                {
                    LoopsUntilSpawnDecrease = 3;
                    if (TimeBetweenSpawns > 2f)
                        TimeBetweenSpawns--;
                }
            }
        }

        override protected void NotifySomethingHappened (GameData.SomethingId id)
        {
            switch (id)
            {
                case GameData.SomethingId.GameStart:
                    Debug.Log ("STARTING SPAWNING");
                    TimeBetweenSpawns = 8f;
                    LoopsUntilSpawnDecrease = 3;
                    StartCoroutine (Creation ());
                    break;
                case GameData.SomethingId.ToMainMenu:
                    Debug.Log ("STOPPING SPAWNING");
                    StopCoroutine (Creation ());
                    break;
            }
        }
    }
}