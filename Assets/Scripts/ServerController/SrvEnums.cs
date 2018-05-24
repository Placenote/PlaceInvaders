using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PunServerNs
{
    public enum NetGameStateId
    {
        Disconnected,
        ConnectedOutOfRoom,
        Connecting,
        ConnectedInRoom,
        Failed
    }
}