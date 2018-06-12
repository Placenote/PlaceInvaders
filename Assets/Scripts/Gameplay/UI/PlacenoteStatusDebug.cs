using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Placenote;
using UnityEngine.UI;

public class PlacenoteStatusDebug : MonoBehaviour, PlacenoteListener 
{
    public Text StatusText;

    void Start ()
    {
        StatusText = GetComponent<Text> ();
        LibPlacenote.Instance.RegisterListener (this);
    }

    public void OnPose (Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

    public void OnStatusChange (LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
    {
        if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
        {
            StatusText.text = "Localized";
        }
        else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING)
        {
            StatusText.text = "Mapping";
        }
        else if (currStatus == LibPlacenote.MappingStatus.LOST)
        {
            StatusText.text = "Lost";
        }
        else if (currStatus == LibPlacenote.MappingStatus.WAITING)
        {
            StatusText.text = "Waiting";
        }
    }
}
