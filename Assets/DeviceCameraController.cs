using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceCameraController : MonoBehaviour {
	public RawImage image;
	private WebCamDevice cameraDevice;
	private WebCamTexture cameraTexture;

	// Use this for initialization
	void Start () {
		// Check for device cameras
		if (WebCamTexture.devices.Length == 0) {
			Debug.Log ("No devices cameras found");
			return;
		}
	
		cameraDevice = WebCamTexture.devices.First();
		cameraTexture = new WebCamTexture(cameraDevice.name);

         image.texture = cameraTexture;
         image.material.mainTexture = cameraTexture;
 
         cameraTexture.Play();
	}

	// Update is called once per frame
	void Update () {

	}
}