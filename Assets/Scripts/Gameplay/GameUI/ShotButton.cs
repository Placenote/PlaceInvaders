using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameUiNs
{

    public class ShotButton : KeyboardClickableBase
    {


        void Shoting(bool IsShoting)
        {

                GameController.Instance.Weapon.CallRPCShotTrigger(IsShoting);
        }

        override public void OnPointerDown(PointerEventData eventData)
        {
;
            Shoting(true);

        }
        public void StopFiring()
        {
            Shoting(false);
        }

        override public void OnPointerUp(PointerEventData eventData) {StopFiring(); }

        override public void OnPointerClick(PointerEventData eventData)  {}
       

    }
}
