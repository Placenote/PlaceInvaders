using _DevicePosNs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayNs
{


    [System.Serializable]
    public class OnePlayerData
    {

        public int Lifes;
        public int Kills;

        public bool IsPlayerDead;


        public void Reset ()
        {
            Lifes = 0;
            Kills = 0;
        }

    }

    public enum GameStateId
    {
        GameWaitingStart = 0,
        GamePlaying,
        GameOver
    }
    
    /// <summary>
    /// Simple class mostly for UI  to store game data with support of notifications about changes,
    /// UI component have to be responsible itself to decide when it have be notified and which data should be shown
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public GameStateId GameState = GameStateId.GameWaitingStart;

        #region Events section
        public enum SomethingId
        {
			WorldRootPlaced,
            AskToStartGame,
			GamePreparing,
            GameStart,
			ToggleGameMenu,
            PlayerDied,
            PlayerResurrected,
            GameOver, 
			ToMainMenu,
        }

        // ---------------------------------------------------------------------------
        // Rules about notifications: if it have to be sent both 
        // notification about the event and  data changes,
        // it should sent the notification about event first,
        // and only after that - the information about the data changes
        // That's because it is supposed that those who got notification 
        // about action, will got required event-specific data themself on receiving this event,
        // and this will affect the component state
        // but all other data such component receives on regular base which is not related with event
        // so should be received after changing the state of component.
        // ---------------------------------------------------------------------------
        public event Action<SomethingId> NotifySomethingHappened =  //delegate { };
             ( (id) =>  Debug.Log ("Sent " + id)  );
        public event Action NotifySomeDataChanged = delegate { };



        #endregion Events section

        public void Reset ()
        {
            PlayerData.Reset ();
        }

		public void OnPrepareGame ()
		{
			GameState = GameStateId.GameWaitingStart;
			NotifySomethingHappened (SomethingId.GamePreparing);
			NotifySomeDataChanged ();
		}

        public void OnStartGame ()
        {
            GameState = GameStateId.GamePlaying;
            NotifySomethingHappened (SomethingId.GameStart);
            NotifySomeDataChanged ();
        }

		public void TogglePlayerMenu ()
		{
			NotifySomethingHappened (SomethingId.ToggleGameMenu);
		}

		public void OnToMainMenu ()
		{
			GameState = GameStateId.GameWaitingStart;
			NotifySomethingHappened (SomethingId.ToMainMenu);
		}

        /// <summary>
        /// Call DoNotifySomethingHappened action outside of this class
        /// </summary>
        /// <param name="id"></param>
        public void NotifyWorldRootPlaced ()
        {
            NotifySomethingHappened (SomethingId.WorldRootPlaced);
        }

        #region data section, add new here
        [Tooltip ("public just for debug purposes")]
        public OnePlayerData PlayerData = new OnePlayerData ();


        #endregion data section, add new here


        #region properties section, add new here



        public int Kills
        {
            get { return PlayerData.Kills; }
            set { PlayerData.Kills = value; NotifySomeDataChanged (); }
        }

        public bool ResurrectPlayer ()
        {
            if (PlayerData.Lifes == 0 || !PlayerData.IsPlayerDead)
                return false;

            PlayerData.Lifes--;
            PlayerData.IsPlayerDead = false;
            NotifySomethingHappened (SomethingId.PlayerResurrected);
            NotifySomeDataChanged ();
            return true;
        }

        public void RegisterPlayerDeath ()
        {
            PlayerData.IsPlayerDead = true;
            NotifySomethingHappened (SomethingId.PlayerDied);
            NotifySomeDataChanged ();

            if (PlayerData.Lifes == 0) {
                NotifySomethingHappened (SomethingId.GameOver);
            }
        }

        public bool IsPlayerDead
        {
            get { return PlayerData.IsPlayerDead; }
           
        }


        public int Lifes
        {
            get { return PlayerData.Lifes; }
            set
            {
                PlayerData.Lifes = (value <0? 0: value);
                NotifySomeDataChanged ();
            }
        }
        #endregion properties section, add new here

    }
}
