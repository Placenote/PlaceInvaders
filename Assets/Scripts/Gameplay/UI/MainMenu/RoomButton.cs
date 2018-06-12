using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Placenote;

namespace GameUiNs
{
    public class RoomButton : MonoBehaviour
    {
        public string RoomName { private get; set;  }

        public void JoinRoom ()
        {
            PlacenoteMultiplayerManager.Instance.JoinRoom (RoomName);
        }
    }
}
