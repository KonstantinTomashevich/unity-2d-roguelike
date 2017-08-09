using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAnimation : IAnimation {
	public const float ANIMATION_TIME = 0.25f;

	private float elapsedTime_;
	private GameObject sprite_;	
	private Vector2 speed_;

	public MeleeAttackAnimation (GameObject sprite, Vector2 direction) {
		elapsedTime_ = 0.0f;
		sprite_ = sprite;
		Debug.Assert (direction == Vector2.up || direction == Vector2.down ||
			direction == Vector2.left || direction == Vector2.right);
		speed_ = direction / ANIMATION_TIME;
	}

	~MeleeAttackAnimation () {
	}

	public void Init (GameObject hostObject) {
	}

	public void Process () {
		elapsedTime_ += Time.deltaTime;
		Vector2 translate = speed_ * Time.deltaTime;
		if (elapsedTime_ >= ANIMATION_TIME / 2.0f) {
			translate *= -1;
		}
		sprite_.transform.Translate (translate);
	}

	public bool isFinished { 
		get {
			return elapsedTime_ >= ANIMATION_TIME;
		}
	}
}
