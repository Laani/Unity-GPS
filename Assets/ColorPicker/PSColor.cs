using UnityEngine;
using System.Collections;

public class PSColor : MonoBehaviour {

	void OnSetColor(Color color)
	{
		particleSystem.renderer.material.SetColor("_TintColor", color);
	}
	
	void OnGetColor(ColorPicker picker)
	{
		picker.NotifyColor(particleSystem.renderer.material.GetColor("_TintColor"));
	}
}
