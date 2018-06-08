using EnemiesNs;
using PlayerNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponNs;
using Placenote;

namespace GameplayNs
{
    /// <summary>
    /// Mostly just mediator to simplify development.
    /// "Scene-only Singleton"  -  cleared and destroyed when game object deleted.
    /// Initialized at Awake so should not be called from other Awake() functions except
    /// generated at scene definitely after initialization.
    /// </summary>
    public class GameController : PlacenotePunMultiplayerBehaviour
    {
        [Header ("Setup enemies here")]
        public EnemyLibrary Lib;

        [Header ("Public for debug purposes only")]
        // Enemey spawn point
        public List<RespawnPoint> Respawns;
        // Game Data reference
        public GameData Data;
        // Enemy spawn distance
        [Range (5, 20)]
        public float spawnDistance;

        // Game information properties

        public bool IsGamePlaying
        {
            get { return Data.GameState == GameStateId.GamePlaying; }
        }

        #region Singleton Implementation

        private static GameController sInstance;
        public static GameController Instance
        {
            get { return sInstance; }
        }

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public static bool IsInitialized
        {
            get { return sInstance != null; }
        }

        /// <summary>
        /// Base awake method that sets the singleton's unique instance.
        /// </summary>
        protected virtual void Awake ()
        {
            if (sInstance != null)
            {
                Debug.LogError ("Trying to instantiate a second instance of  GameController singleton ");
            }
            else
            {
                Data = new GameData ();
                sInstance = this;
            }
            WorldRootObject = FindObjectOfType<WorldRoot> ();
        }

        protected virtual void OnDestroy ()
        {
            if (sInstance == this)
            {
                sInstance = null;
            }
        }

        #endregion Singleton Implementation


        #region Game State

        public void StartGame ()
        {
            // Set the world root to where the camera is
            Debug.Log ("Setting world Coordinates: " + Camera.main.transform.position);
            GameController.Instance.SetupWorldRootCoordinates
                          (Camera.main.transform.position, Camera.main.transform.rotation);
            // Start game for other players if host starts game
            if (PlacenoteMultiplayerManager.Instance.IsHost)
                GetComponent<PhotonView> ().RPC ("HostStartedGameRPC", PhotonTargets.Others);
            // Resets all player values
            Data.Reset ();
            Data.OnStartGame ();
        }

        // Players must be localized to start the game.
        [PunRPC]
        public void HostStartedGameRPC ()
        {
            if (PlacenoteMultiplayerManager.Instance.IsLocalized)
            {
                PlacenoteMultiplayerManager.Instance.StartGame ();
                StartGame ();
            }
        }

        public void QuitGame ()
        {
            // Remove enemies
            RemoveAllEnemies ();
            // Remove player is handeled by PlayerPhotonGenerator
            Data.OnToMainMenu ();
        }

        #endregion Game State

        #region World root

        /// <summary>
        /// direct changing position/rotation is forbidden. Reason:
        /// it have to be done either by positioning system or with initial setup of location
        /// Use SetupWorldRootCoordinates or _DevicePos, whatever will work
        /// </summary>
        public static WorldRoot WorldRootObject
        {
            get; private set;
        }

        /// <summary>
        /// called on initial setup procedures, during gameplay world
        /// coordinates can be updated without coordinates and non-stop
        /// </summary>
        /// <param name="newPos"> new initial world root position </param>
        /// <param name="newRot"> new initial world root rotation </param>
        public void SetupWorldRootCoordinates (Vector3 newPos, Quaternion newRot)
        {
            WorldRootObject.SetupWorldRootCoordinates (newPos, newRot);
        }

        #endregion World root

        #region Enemy

        public string EnemyTag
        {
            get { return Lib.EnemyTag; }
        }

        public string EnemyDamageReceiverName
        {
            get { return Lib.DamageReceiverMethodName; }
        }

        #region > Enemy Spawning and Despawning

        public static void RegisterRespawnPoint (RespawnPoint newRespawn)
        {
            if (!Instance.Respawns.Contains (newRespawn))
                Instance.Respawns.Add (newRespawn);
        }

        public void CreateRandomEnemyAtPoint ()
        {
            var prefab = Instance.Lib.GetRandomEnemy ().prefab;

            GameObject newEnemy = null;
            int angle = Random.Range (0, 359);
            // Create enemy at worldRoot
            newEnemy = PhotonNetwork.InstantiateSceneObject (
                    prefab.name,
                    WorldRootObject.transform.position,
                    Quaternion.identity,
                    0, null);
            // Offset enemy position by the spawnDistance
            newEnemy.transform.position += new Vector3 (spawnDistance * Mathf.Sin (angle), 0, spawnDistance * Mathf.Cos (angle));
            newEnemy.transform.LookAt (Vector3.zero);
            newEnemy.transform.parent = WorldRootObject.transform;

            Data.TotalEnemies++;
        }

        public void RemoveAllEnemies ()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");

            for (int i = 0; i < enemies.Length; i++)
            {
                Destroy (enemies[i]);
            }
        }

        #endregion > Enemy Spawning and Despawning

        #endregion Enemy

        #region Player

        public PlayerController CurrentPlayer;

        public List<PlayerController> Players;

        public void RegisterPlayer (PlayerController player)
        {
            Debug.Log ("Registering player");
            if (player.IsCurrentNetworkPlayer)
            {
                CurrentPlayer = player;
            }
            int idx = Instance.Players.FindIndex ((el) => el != null && el.Equals (player));
            if (idx >= 0)
                Instance.Players[idx] = player;
            else
                Instance.Players.Add (player);

            Debug.Log ("------------ Registered  player " + player.gameObject.name + " --------------");

        }

        public PlayerController GetRandomPlayer ()
        {
            var alivePlayers = Instance.Players.FindAll (el => el != null && !el.IsDead);
            Debug.Log ("Alive Player Count Is: " + alivePlayers.Count);
            if (alivePlayers.Count == 0)
                return null;
            int idx = Random.Range (0, alivePlayers.Count);
            return alivePlayers[idx];
        }

        public void ResurrectPlayer ()
        {
            Data.ResurrectPlayer ();
        }

        public void RemovePlayer (PlayerController player)
        {
            if (player == null)
                Instance.Players.RemoveAll (el => el == null);
            if (player.Equals (CurrentPlayer))
                CurrentPlayer = null;
            if (Instance.Players.Remove (player))
                Debug.Log ("------------ Removed  player " + player.gameObject.name + " --------------");
            else
                Debug.Log ("------------ Failed Removing  player " + player.gameObject.name + "--------------");
        }

        #region > Weapons

        /// <summary>
        /// Current weapon
        /// </summary>
        public WeaponController Weapon
        {
            get { return CurrentPlayer.CurrentWeapon; }
        }

        #endregion > Weapons

        #endregion Player
    }
}