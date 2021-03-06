﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class myGUIScript : MonoBehaviour
{

		public GUISkin myGUISkin;
		public List<string> debugs;
		private string groupName;
		private string drawingName;
		public bool penDown = false;
		private bool oldPen = false;
		public int strokeName;
		private Color strokeColor;
		ColorPicker[] mColorPicker;
		private NetworkHelper mNetworkHelper;

		// Use this for initialization
		void Start ()
		{
				if (this.myGUISkin == null) {
						addDebug ("Please assign a GUISkin on the editor!");
						this.enabled = false;
						return;
				}

				mNetworkHelper = GameObject.Find ("myNetworkHelper").GetComponentInChildren<NetworkHelper> ();

				groupName = "jkw";
				drawingName = "drawingtest";
				strokeName = 0;

				mColorPicker = GameObject.FindObjectsOfType<ColorPicker> ();
				foreach (ColorPicker elem in mColorPicker) {
						elem.useExternalDrawer = true;
				}
		}

		// Update is called once per frame
		void Update ()
		{
				if (oldPen != penDown) {
						// pen toggle changed
						if (penDown) {
								// pen is now on
								// start a new stroke
								strokeName++;
								// give current stroke color to the stroke
								mNetworkHelper.addStrokeColor ("" + strokeName, strokeColor);
								addDebug ("Pen on. Stroke name: " + strokeName + ", Color: " + strokeColor);
						} else {
								// pen is now off
								addDebug ("Pen off.");
						}

						oldPen = penDown;
				}
		}
	
		void OnGUI ()
		{
				GUI.skin = this.myGUISkin;
				groupName = GUI.TextField (new Rect (0, 0, Screen.width, 100), groupName);
				drawingName = GUI.TextField (new Rect (0, 110, Screen.width, 100), drawingName);

				foreach (ColorPicker cp in mColorPicker) {
						cp._DrawGUI ();
				}

				penDown = GUI.Toggle (new Rect (0, 340, Screen.width / 2 - 10, 75), penDown, "Pen");

				if (GUI.Button (new Rect (Screen.width / 2 + 5, 340, Screen.width / 2 - 10, 75), "Upload")) {
						// upload the data
						addDebug ("Uploading data with group name (" + groupName + ") and drawing name (" + drawingName + ")");
						mNetworkHelper.uploadPoints (groupName, drawingName);
				}

				int tempy = 440;
				foreach (string d in debugs) {
						GUI.Label (new Rect (0, tempy, Screen.width, 35), d);
						tempy += 45;
				}
				while (debugs.Count > 4) {
						debugs.RemoveAt (0);
				}
		}

		void OnSetColor (Color color)
		{
				strokeColor = color;
		}

		void OnGetColor (ColorPicker picker)
		{
				picker.NotifyColor (strokeColor);
		}

		public void addDebug (string e)
		{
				debugs.Add (e);
				Debug.Log (e);
		}
}