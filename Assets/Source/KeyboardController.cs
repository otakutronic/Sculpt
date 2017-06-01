using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour {

	public Transform target;
	public float distanceMin = 15.0F;
	public float distanceMax = 20.0F;
	public float distanceInitial = 17.5F;
	public float scrollSpeed = 1.0F;

	public float xSpeed = 250.0F;
	public float ySpeed = 120.0F;

	public int yMinLimit = -20;
	public int yMaxLimit = 80;

	private float x = 0.0F;
	private float y = 0.0F;
	private float distanceCurrent = 0.0F;

	[AddComponentMenu("Camera-Control/Key Mouse Orbit")]

	void Start () {
		var angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

		distanceCurrent = distanceInitial;

		Rigidbody rigidb = GetComponent<Rigidbody>();

		// Make the rigid body not change rotation
		if (rigidb)
			rigidb.freezeRotation = true;
	}

	void LateUpdate () {
		if (target) {
			x += Input.GetAxis("Horizontal") * xSpeed * 0.02F;
			y -= Input.GetAxis("Vertical") * ySpeed * 0.02F;
			distanceCurrent -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;

			distanceCurrent = Mathf.Clamp(distanceCurrent, distanceMin, distanceMax);
			y = ClampAngle(y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler(y, x, 0);
			Vector3 position = rotation * new Vector3(0.0F, 0.0F, -distanceCurrent) + target.position;

			transform.rotation = rotation;
			transform.position = position;
		}
	}

	static float ClampAngle (float angle, float min, float max) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
}
