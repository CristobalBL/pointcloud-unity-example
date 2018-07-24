using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {


	#if UNITY_ANDROID && !UNITY_EDITOR 
	Vector2?[] oldTouchPositions = { null, null };
	Vector2 oldTouchVector;
	float oldTouchDistance;
	private Camera camera;

	void Start(){
		camera = Camera.main;
		if (camera.orthographic == false) { 
			camera.orthographic = true;
		}
	}
	#endif

	#if UNITY_EDITOR
	public int speed = 50;
	#endif

	void Update() {
		#if UNITY_ANDROID && !UNITY_EDITOR 
		if (Input.touchCount == 0) {
			oldTouchPositions[0] = null;
			oldTouchPositions[1] = null;
		}
		else if (Input.touchCount == 1) {
		if (oldTouchPositions[0] == null || oldTouchPositions[1] != null) {
			oldTouchPositions[0] = Input.GetTouch(0).position;
			oldTouchPositions[1] = null;
		}
		else {
			Vector2 newTouchPosition = Input.GetTouch(0).position;

			transform.position += transform.TransformDirection((Vector3)((oldTouchPositions[0] - newTouchPosition) * camera.orthographicSize / camera.pixelHeight * 2f));

			oldTouchPositions[0] = newTouchPosition;
		}
		}
		else {
			if (oldTouchPositions[1] == null) {
				oldTouchPositions[0] = Input.GetTouch(0).position;
				oldTouchPositions[1] = Input.GetTouch(1).position;
				oldTouchVector = (Vector2)(oldTouchPositions[0] - oldTouchPositions[1]);
				oldTouchDistance = oldTouchVector.magnitude;
			}
			else {
				Vector2 screen = new Vector2(camera.pixelWidth, camera.pixelHeight);

				Vector2[] newTouchPositions = {
					Input.GetTouch(0).position,
					Input.GetTouch(1).position
				};
				Vector2 newTouchVector = newTouchPositions[0] - newTouchPositions[1];
				float newTouchDistance = newTouchVector.magnitude;

				transform.position += transform.TransformDirection((Vector3)((oldTouchPositions[0] + oldTouchPositions[1] - screen) * camera.orthographicSize / screen.y));
				transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, Mathf.Asin(Mathf.Clamp((oldTouchVector.y * newTouchVector.x - oldTouchVector.x * newTouchVector.y) / oldTouchDistance / newTouchDistance, -1f, 1f)) / 0.0174532924f));
				camera.orthographicSize *= oldTouchDistance / newTouchDistance;
				transform.position -= transform.TransformDirection((newTouchPositions[0] + newTouchPositions[1] - screen) * camera.orthographicSize / screen.y);

				oldTouchPositions[0] = newTouchPositions[0];
				oldTouchPositions[1] = newTouchPositions[1];
				oldTouchVector = newTouchVector;
				oldTouchDistance = newTouchDistance;
			}
		}
		#endif

		#if UNITY_EDITOR
		if(Input.GetKey(KeyCode.D))
		{
			transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.A))
		{
			transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.S))
		{
			transform.Translate(new Vector3(0,-speed * Time.deltaTime,0));
		}
		if(Input.GetKey(KeyCode.W))
		{
			transform.Translate(new Vector3(0,speed * Time.deltaTime,0));
		}
		if(Input.GetKey(KeyCode.LeftAlt))
		{
			transform.Translate(new Vector3(0,0,-speed * Time.deltaTime));
		}
		if(Input.GetKey(KeyCode.LeftControl))
		{
			transform.Translate(new Vector3(0,0,speed * Time.deltaTime));
		}
		#endif
	}
}
