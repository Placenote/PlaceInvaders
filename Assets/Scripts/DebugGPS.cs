using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using PunServerNs;

public class DebugGPS : MonoBehaviour {

	public SrvController Srv;
	public Text gps;

	void Update () {
		gps.text = "Latitude: " + Srv.latitude + "\nLongitude: " + Srv.longitude;
	}
}
