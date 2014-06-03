using UnityEngine;
using System.Collections;

public class myLocationScript : MonoBehaviour
{

		public GUISkin myGUISkin;
		private NetworkHelper mNetworkHelper;
		private myGUIScript localGui;
		private bool working = false;

		// Use this for initialization
		IEnumerator Start ()
		{
				localGui = GameObject.Find ("myGUI").GetComponentInChildren<myGUIScript> ();
				mNetworkHelper = GameObject.Find ("myNetworkHelper").GetComponentInChildren<NetworkHelper> ();

				if (this.myGUISkin == null) {  
						localGui.addDebug ("Please assign a GUIskin on the editor!");  
						this.enabled = false;  
						yield return false;  
				} 

				if (!Input.location.isEnabledByUser) {
						localGui.addDebug ("User has not enabled Location");
						yield return false;
				}

				Input.location.Start (1f, 1f);

				// Wait until service initializes
				int maxWait = 20;
				while ((Input.location.status == LocationServiceStatus.Initializing) && (maxWait > 0)) {
						yield return new WaitForSeconds (1);
						maxWait--;
				}

				// Service didn't initialize in 20 seconds
				if (maxWait < 1) {
						print ("Timed out");
						yield return false;
				}

				// Connection has failed
				if (Input.location.status == LocationServiceStatus.Failed) {
						localGui.addDebug ("Unable to determine device location");
						return false;
				} else {
						localGui.addDebug ("First Location: " + Input.location.lastData.latitude + " " +
								Input.location.lastData.longitude + " " +
								Input.location.lastData.altitude);
						working = true;
				}

		}

		float lastLongitude = -1;
		float lastLatitude = -1;
		float lastAltitude = -1;
		bool changed = false;
	
		// Update is called once per frame
		void Update ()
		{
				if (working) {
						// check to see if the location has changed since last Update
						if (lastLongitude != Input.location.lastData.longitude || lastLatitude != Input.location.lastData.latitude || lastAltitude != Input.location.lastData.altitude) {
								lastLongitude = Input.location.lastData.longitude;
								lastLatitude = Input.location.lastData.latitude;
								lastAltitude = Input.location.lastData.altitude;
								changed = true;
						} else
								changed = false;

						if (changed) {
								localGui.addDebug ("Location Changed: " + lastLatitude + " " +
										lastLongitude + " " +
										lastAltitude);
			
								if (localGui.penDown) {
										localGui.addDebug ("New Location Recorded: " + lastLatitude + " " +
												lastLongitude + " " +
												lastAltitude);
										// add new location (point) to NetworkHelper
										mNetworkHelper.addPoint ("" + localGui.strokeName, (long)Input.location.lastData.timestamp, lastLatitude, lastLongitude, lastAltitude);
								}

								changed = false;
						}
				}
		}

		void OnGUI ()
		{
				GUI.skin = this.myGUISkin;

				GUI.Label (new Rect (0, Screen.height - 100, Screen.width, 40), "Strokes/Points: " + mNetworkHelper.numStrokes + "/" + mNetworkHelper.numPoints);
				GUI.Label (new Rect (0, Screen.height - 50, Screen.width, 40), "Current Location: " + lastLatitude + " " + lastLongitude + " " + lastAltitude);
		}
}

