using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
    public class ErrorMessage : MonoBehaviour
    {
        public Text ErrorText;

        public void SetErrorText (string message)
        {
            ErrorText.text = ("Could not create room because \n" + message + "\n Please try again");
        }
    }
}