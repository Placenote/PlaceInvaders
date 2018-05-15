using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameplayNs;


namespace TargetNs
{
	public class TargetController : MonoBehaviour 
	{
		// TODO Add different hitboxes so that the user must move around their camera to hit them all.
		public void Hit()
		{
			Debug.Log ("HIT");

			// Start game

			GameController.StartGame();
			// Removes target before game play
			Object.Destroy (this.gameObject);
		}
	}
}