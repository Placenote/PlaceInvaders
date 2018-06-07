using _DevicePosNs;
using System;
using UnityEngine;

namespace GameplayNs
{
    public enum GameStateId
    {
        GameWaitingStart = 0,
        GamePlaying,
        GameOver
    }

    [System.Serializable]
    public class PlayerData
    {
        public int Lives = 10;
        public int Kills = 0;
        public bool IsPlayerDead = false;

        public void Reset ()
        {
            Lives = 10;
            Kills = 0;
            IsPlayerDead = false;
        }
    }

    /// <summary>
    /// Simple class mostly for UI  to store game data with support of notifications about changes,
    /// UI component have to be responsible itself to decide when it have be notified and which data should be shown
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public GameStateId GameState = GameStateId.GameWaitingStart;

        #region data section

        [Tooltip ("public just for debug purposes")]
        public PlayerData PlayerData = new PlayerData ();

        #endregion data section

        #region Events section

        public enum SomethingId
        {
            GameStart,
            PlayerDied,
            PlayerResurrected,
            GameOver,
            ToMainMenu,
        }

        public event Action<SomethingId> OnGameEvent = ((id) => Debug.Log ("Sent " + id));
        public event Action OnGameDataChanged = delegate { };

        public int TotalEnemies { get; set; }

        public void Reset ()
        {
            PlayerData.Reset ();
            OnGameDataChanged ();
            TotalEnemies = 0;
        }

        public void OnStartGame ()
        {
            GameState = GameStateId.GamePlaying;
            OnGameEvent (SomethingId.GameStart);
        }

        public void OnToMainMenu ()
        {
            Reset ();
            GameState = GameStateId.GameWaitingStart;
            OnGameEvent (SomethingId.ToMainMenu);
        }

        #endregion Events section

        #region properties section

        public int Kills
        {
            get { return PlayerData.Kills; }
            set { PlayerData.Kills = value; OnGameDataChanged (); }
        }

        public int Lives
        {
            get { return PlayerData.Lives; }
            set
            {
                PlayerData.Lives = (value < 0 ? 0 : value);
                OnGameDataChanged ();
            }
        }

        public bool IsPlayerDead
        {
            get { return PlayerData.IsPlayerDead; }
        }

        public bool ResurrectPlayer ()
        {
            if (PlayerData.Lives == 0 || !PlayerData.IsPlayerDead)
                return false;

            PlayerData.Lives--;
            PlayerData.IsPlayerDead = false;
            OnGameEvent (SomethingId.PlayerResurrected);
            OnGameDataChanged ();
            Debug.Log ("Kills " + Kills);
            Debug.Log ("Lives " + Lives);
            return true;
        }

        public void RegisterPlayerDeath ()
        {
            PlayerData.IsPlayerDead = true;
            OnGameEvent (SomethingId.PlayerDied);
            OnGameDataChanged ();

            if (PlayerData.Lives == 0)
            {
                OnGameEvent (SomethingId.GameOver);
            }
        }

        #endregion properties section
    }
}