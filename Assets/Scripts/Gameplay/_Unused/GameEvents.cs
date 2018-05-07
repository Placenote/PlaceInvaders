using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GameplayNs
{
    /// <summary>
    ///  This class is designed to be the base for those situations when we wants the flexibility in removing/adding 
    ///  new components
    /// </summary>

    public class GameEvents : MonoBehaviour
    {
        /// <summary>
        /// this field is shared between all instancies of this class
        /// </summary>
        static event Action<GameEventId, int> events = delegate { };

        /// <summary>
        /// this function have to be overriden
        /// </summary>
        /// <param name="id"> identifier of notification</param>
        /// <param name="data">
        /// some data, using depend on  purpose. As "undefined" valueit is recommended to use int.MinValue
        /// </param>
        protected virtual void OnNotification(GameEventId id, int data)
        {
            Debug.Log("Got notification "+ id+ data);
            Debug.LogError("GameEvents.OnNotification() of " + this.GetType() +" is not overriden for script attached to "+gameObject.name);

            switch (id)
            {
                case GameEventId.Undefined:

                    break;
                default:
                    break;
            }
        }

        #region static methods for subscibtion and unsubscription

        static public void Subscribe(Action<GameEventId, int> onNotification)
        {
            events += onNotification;
        }

        static public void UnSubscribe(Action<GameEventId, int> onNotification)
        {
            events -= onNotification;
        }

        static public void Notify(GameEventId id, int data)
        {
            events(id, data);
        }

        static public void Notify(GameEventId id)
        {
            events(id, int.MinValue);
        }

        #endregion static methods for subscibtion and unsubscription

        #region Unity standard functions
        protected virtual void OnEnable()
        {
            Subscribe(OnNotification);
        }


        protected virtual void OnDisable()
        {
            UnSubscribe(OnNotification);
        }

        #endregion Unity standard functions



    }
}
