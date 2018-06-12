using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Placenote;

namespace GameUiNs
{
    public class RoomButtonsPanel : PlacenotePunMultiplayerBehaviour
    {
        // Child object references
        public Text StatusText;

        // Button to instantiate prefab
        [SerializeField] GameObject roomButtonPrefab;

        // Range in meters to search for rooms
        private static float GPS_SEARCH_RANGE = 100f;

        public void OnEnable ()
        {
            GenerateViewRooms ();
        }

        /// <summary>
        /// Generates all the available rooms that are nearby based on GPS
        /// </summary>
        public void GenerateViewRooms ()
        {
            // Clear the rooms first.
            DeleteViewRooms ();
            RoomInfo[] roomsArray = PlacenoteMultiplayerManager.Instance.GetListOfNearbyRooms (GPS_SEARCH_RANGE);
            if (roomsArray.Length > 0)
            {
                StatusText.gameObject.SetActive (false);
                foreach (RoomInfo game in roomsArray)
                {
                    GameObject room = (GameObject)Instantiate (roomButtonPrefab);

                    room.GetComponent<Button> ().onClick.AddListener (
                        () => {
                            room.GetComponent<RoomButton> ().JoinRoom ();
                        });
                    room.GetComponent<RoomButton> ().RoomName = game.Name;
                    room.transform.GetChild (0).GetComponent<Text> ().text = game.Name;
                    room.transform.GetChild (1).GetComponent<Text> ().text = game.PlayerCount + "/" + game.MaxPlayers;
                    room.transform.SetParent (gameObject.transform, false);
                }
            }
            else
            {
                StatusText.gameObject.SetActive (true);
                StatusText.text = "There are no rooms available..."
                + "\nMake sure your device is connected to the internet.";
            }
        }

        /// <summary>
        /// Removes all rooms from viewRoomsPanel
        /// </summary>
        void DeleteViewRooms ()
        {
            foreach (Transform child in gameObject.transform)
            {
                // Don't delete the Status text with tag "Text"
                if (!child.gameObject.CompareTag ("Text"))
                    GameObject.Destroy (child.gameObject);
            }
        }

        // Generate rooms again if received an update.
        protected override void OnRoomListUpdate ()
        {
            GenerateViewRooms ();
        }
    }
}
