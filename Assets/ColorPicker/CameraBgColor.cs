using UnityEngine;
using System.Collections;

public class CameraBgColor : MonoBehaviour {
	
	void OnSetColor(Color color)
	{
		camera.backgroundColor = color;
	}

	void OnGetColor(ColorPicker picker)
	{
		picker.NotifyColor(camera.backgroundColor);
	}
}
