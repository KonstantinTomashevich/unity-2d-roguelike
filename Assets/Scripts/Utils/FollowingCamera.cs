using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour {
	public float speed;
	public int borderSize;

	private IUnit followingUnit_;
	private bool scrollingToUnit_;
	private Vector2 mapSize_;

	void Start () {
		scrollingToUnit_ = false;
	}

	void Update () {
		if (scrollingToUnit_) {
			CenterOnUnit ();
		} else {
			ProcessCameraTranslation ();
		}

		bool[] bordersReached = CorrectCameraPosition ();
		if (scrollingToUnit_ && !CheckIsScrollingNeeded (bordersReached)) {
			scrollingToUnit_ = false;
		}
	}

	void MapSize (Vector2 mapSize) {
		mapSize_ = mapSize;
	}

	void PlayerUnitCreated (IUnit unit) {
		followingUnit_ = unit;
		scrollingToUnit_ = true;
	}

	void NextTurnRequest () {
		scrollingToUnit_ = true;
	}

	void TurnFinished () {
	}

	public IUnit followingUnit {
		get {
			return followingUnit_;
		}

		set {
			Debug.Assert (value != null);
			followingUnit_ = value;
		}
	}

	private void ProcessCameraTranslation () {
		Vector3 translate = new Vector3 ();
		Vector2 mousePosition = Input.mousePosition;
		if (mousePosition.x <= borderSize) {
			translate.x -= speed;
		} else if (mousePosition.x >= Screen.width - borderSize) {
			translate.x += speed;
		}

		if (mousePosition.y <= borderSize) {
			translate.y -= speed;
		} else if (mousePosition.y >= Screen.height - borderSize) {
			translate.y += speed;
		}

		translate *= Time.smoothDeltaTime;
		transform.Translate (translate);
	}

	private void CenterOnUnit () {
		Vector2 cameraPosition = new Vector2 (transform.position.x, transform.position.y);
		Vector2 unitPosition = followingUnit.position;
		Vector2 diff = unitPosition - cameraPosition;
		if (diff.magnitude > Time.smoothDeltaTime * speed) {
			diff = diff.normalized * Time.smoothDeltaTime * speed;
		}
		transform.Translate (new Vector3 (diff.x, diff.y, 0.0f));
	}

	private bool[] CorrectCameraPosition () {
		Vector3 cameraPosition = transform.position;
		Camera currentCamera = gameObject.GetComponent <Camera> ();
		float ortho = currentCamera.orthographicSize;
		float aspect = currentCamera.aspect;

		if (cameraPosition.x + ortho * aspect > mapSize_.x / 2 - 0.5f) {
			cameraPosition.x = mapSize_.x / 2 - ortho * aspect - 0.5f;
		} else if (cameraPosition.x - ortho * aspect < -mapSize_.x / 2 - 0.5f) {
			cameraPosition.x = -mapSize_.x / 2 + ortho * aspect - 0.5f;
		}

		if (cameraPosition.y + ortho > mapSize_.y / 2 - 0.5f) {
			cameraPosition.y = mapSize_.y / 2 - ortho - 0.5f;
		} else if (cameraPosition.y - ortho < -mapSize_.y / 2 - 0.5f) {
			cameraPosition.y = -mapSize_.y / 2 + ortho - 0.5f;
		}

		bool[] bordersReached = { transform.position.x != cameraPosition.x, transform.position.y != cameraPosition.y };
		transform.position = cameraPosition;
		return bordersReached;
	}

	private bool CheckIsScrollingNeeded (bool[] bordersReached) {
		Vector2 cameraPosition = new Vector2 (transform.position.x, transform.position.y);
		Vector2 unitPosition = followingUnit.position;
		bool shouldScrollX = !Mathf.Approximately (unitPosition.x, cameraPosition.x) && !bordersReached [0];
		bool shouldScrollY = !Mathf.Approximately (unitPosition.y, cameraPosition.y) && !bordersReached [1];
		return shouldScrollX || shouldScrollY;
	}
}
