using GameplayNs;
using System.Collections;
using UnityEngine;

namespace EnemiesNs
{
    public class RespawnPoint : EventsSubscriber
    {
        private float TimeBetweenSpawns = 8f;
        private int LoopsUntilSpawnDecrease = 6;

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
                if (GameController.Data.GameState == GameStateId.GamePlaying
                    && GameController.Data.TotalEnemies < 4 * GameController.Data.PlayersInRoom)
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
                    LoopsUntilSpawnDecrease = 6;
                    if (TimeBetweenSpawns > 4f)
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
                    LoopsUntilSpawnDecrease = 6;
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