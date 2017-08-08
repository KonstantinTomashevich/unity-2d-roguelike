using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsProcessor : MonoBehaviour {
	private List <IAnimation> animationsQueue_;
	private IAnimation currentAnimation_;
	private GameObject animationHostObject_;

	void Start () {
		animationsQueue_ = new List <IAnimation> ();
		currentAnimation_ = null;
		animationHostObject_ = new GameObject ();
		animationHostObject_.transform.parent = transform;
	}

	void Update () {
		if (currentAnimation_ == null && animationsQueue_.Count > 0) {
			currentAnimation_ = animationsQueue_ [0];
			animationsQueue_.RemoveAt (0);
			currentAnimation_.Init (animationHostObject_);
		}

		if (currentAnimation_ != null) {
			currentAnimation_.Process ();
			if (currentAnimation_.isFinished) {
				Destroy (animationHostObject_);
				animationHostObject_ = new GameObject ();
				animationHostObject_.transform.parent = transform;

				MessageUtils.SendMessageToObjectsWithTag (tag, "AnimationFinished", currentAnimation_);
				currentAnimation_ = null;

				if (animationsQueue_.Count == 0) {
					MessageUtils.SendMessageToObjectsWithTag (tag, "AllAnimationsFinished", null);
				}
			}
		}
	}

	void RequestAnimation (IAnimation animation) {
		animationsQueue_.Add (animation);
	}
}
