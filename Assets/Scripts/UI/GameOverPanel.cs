using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour {
	public GameObject gameOverPanelObject;
	private IUnit playerUnit_;

	void Start () {
		gameOverPanelObject.SetActive (false);
		foreach (Transform child in gameOverPanelObject.transform) {
			child.gameObject.SetActive (false);
		}
	}

	void Update () {
	}

	public void ExitPressed () {
		Application.Quit ();
	}

	void PlayerUnitCreated (PlayerUnit unit) {
		playerUnit_ = unit;
	}

	void UnitDie (IUnit unit) {
		if (playerUnit_ == unit) {
			gameOverPanelObject.SetActive (true);
			foreach (Transform child in gameOverPanelObject.transform) {
				child.gameObject.SetActive (true);
			}
		}
	}
}
