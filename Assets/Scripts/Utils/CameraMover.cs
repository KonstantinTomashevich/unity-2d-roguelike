using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {
	public float speed;
	public KeyCode up;
	public KeyCode down;
	public KeyCode left;
	public KeyCode right;

	void Start () {

	}

	void Update () {
		Vector3 translate = new Vector3 ();
		if (Input.GetKey (up)) {
			translate.y += speed;
		}

		if (Input.GetKey (down)) {
			translate.y -= speed;
		}

		if (Input.GetKey (left)) {
			translate.x -= speed;
		}

		if (Input.GetKey (right)) {
			translate.x += speed;
		}

		translate *= Time.deltaTime;
		transform.Translate (translate);
	}
}
