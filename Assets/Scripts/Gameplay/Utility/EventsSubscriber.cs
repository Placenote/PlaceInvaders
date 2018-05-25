using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class that allows subscribing to GameData events:
/// NotifySomethingHappened and NotifySomeDataChanged.
/// </summary>
public class EventsSubscriber : MonoBehaviour
{
    #region event handlers to override

    virtual protected void NotifySomeDataChanged () { }

    virtual protected void NotifySomethingHappened (GameData.SomethingId id)
    {
        switch (id)
        {
            case GameData.SomethingId.PlayerDied:
                break;
        }
    }

    #endregion event handlers to override


    #region Subscribing to events

    bool doSubscibe = true;

    virtual protected void OnEnable ()
    {
        doSubscibe = true;
    }

    virtual protected void OnDisable ()
    {
        if (GameController.Data != null)
        {
            GameController.Data.NotifySomeDataChanged -= NotifySomeDataChanged;
            GameController.Data.NotifySomethingHappened -= NotifySomethingHappened;
        }
    }

    protected virtual void Subscribe ()
    {
        GameController.Data.NotifySomeDataChanged += NotifySomeDataChanged;
        GameController.Data.NotifySomethingHappened += NotifySomethingHappened;
    }

    protected virtual void Update ()
    {
        if (doSubscibe)
        {
            doSubscibe = false;
            Subscribe ();
        }
    }

    #endregion Subscribing to events
}