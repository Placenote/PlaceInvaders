using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
	public class GameMenuPanel : MonoBehaviour {

		public Button MainMenuBtn; 

		private void Start ()
		{			
			MainMenuBtn.onClick.AddListener (GoToMainMenu);
		}

		private void GoToMainMenu ()
		{
			// Remove player is handeled by PlayerPhotonGenerator
			// Remove enemies
			GameController.RemoveAllEnemies ();
			GameController.Instance.QuitGame ();
			GameController.Data.OnToMainMenu ();
		}
	}
}