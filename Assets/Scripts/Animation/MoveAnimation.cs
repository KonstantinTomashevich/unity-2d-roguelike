using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAnimation : IAnimation {
	public const float ANIMATION_TIME = 0.5f;

	private float elapsedTime_;
	private GameObject sprite_;	
	private Vector2 speed_;

	public MoveAnimation (GameObject sprite, Vector2 direction) {
		elapsedTime_ = 0.0f;
		sprite_ = sprite;
		Debug.Assert (direction == Vector2.up || direction == Vector2.down ||
			direction == Vector2.left || direction == Vector2.right);
		speed_ = direction / ANIMATION_TIME;
	}

	~MoveAnimation () {
	}

	public void Init (GameObject hostObject) {
	}

	public void Process () {
		elapsedTime_ += Time.deltaTime;
		Vector2 translate = speed_ * Time.deltaTime;
		sprite_.transform.Translate (translate);
	}

	public bool isFinished { 
		get {
			return elapsedTime_ >= ANIMATION_TIME;
		}
	}
}
