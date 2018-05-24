using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace GameUiNs
{
    public class KeyboardClickableBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public bool SimulateClickWithKeyboard;
        public KeyCode ClickReplacement;

        private void LogNotOverriden (string finctionName)
        {
            Debug.LogError (this.GetType ().ToString () + " at gameobject " + gameObject.name + finctionName + " is not overriden ");

        }
        virtual public void OnPointerDown (PointerEventData eventData)
        {
            LogNotOverriden ("OnPointerDown");
        }

        virtual public void OnPointerUp (PointerEventData eventData)
        {
            LogNotOverriden ("OnPointerUp");
        }

        virtual public void OnPointerClick (PointerEventData eventData)
        {
            LogNotOverriden ("OnPointerClick");
        }


#if UNITY_EDITOR

        private void Update ()
        {
            if (SimulateClickWithKeyboard)
            {
                if (Input.GetKeyDown (ClickReplacement))
                {
                    ExecuteEvents.Execute (gameObject, new PointerEventData (EventSystem.current),
                        ExecuteEvents.pointerEnterHandler);

                    ExecuteEvents.Execute (gameObject, new PointerEventData (EventSystem.current),
                        ExecuteEvents.pointerDownHandler);
                }

                if (Input.GetKeyUp (ClickReplacement))
                {
                    ExecuteEvents.Execute (gameObject, new PointerEventData (EventSystem.current),
                          ExecuteEvents.pointerUpHandler);

                    ExecuteEvents.Execute (gameObject, new PointerEventData (EventSystem.current),
                            ExecuteEvents.pointerClickHandler);

                    ExecuteEvents.Execute (gameObject, new PointerEventData (EventSystem.current),
                        ExecuteEvents.pointerExitHandler);
                }
            }
        }
#endif
    }
}