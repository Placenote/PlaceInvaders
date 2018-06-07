using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class that allows subscribing to GameData events:
/// OnGameEvent and OnGameDataChanged.
/// </summary>
public class GameEventsSubscriber : MonoBehaviour
{
    #region event handlers to override

    virtual protected void OnGameDataChanged () { }

    virtual protected void OnGameEvent (GameData.SomethingId id) { } 

    #endregion event handlers to override


    #region Subscribing to events

    bool doSubscibe = true;

    virtual protected void OnEnable ()
    {
        doSubscibe = true;
    }

    virtual protected void OnDisable ()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.Data.OnGameDataChanged -= OnGameDataChanged;
            GameController.Instance.Data.OnGameEvent -= OnGameEvent;
        }
    }

    protected virtual void Subscribe ()
    {
        GameController.Instance.Data.OnGameDataChanged += OnGameDataChanged;
        GameController.Instance.Data.OnGameEvent += OnGameEvent;
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
