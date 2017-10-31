using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour {
	public GUISkin skin;
	private IUnit playerUnit_;

	void Start () {
	}

	void Update () {
	}

	void OnGUI () {
		GUI.skin = skin;

		int W = Screen.width;
		int H = Screen.height;
		float hW = W / 2.0f;
		float hH = H / 2.0f;

		skin.label.fontSize = H / 30;
		skin.button.fontSize = H / 23;
		skin.GetStyle ("title").fontSize = H / 15;

		if (playerUnit_ == null) {
			GUILayout.Window (3, new Rect (hW - H / 3.0f, hH - H / 6.0f, H / 1.5f, H / 3.0f), (int id) => {
				GUILayout.Label ("Game over!", skin.GetStyle ("title"));

				if (GUILayout.Button ("Restart game.")) {
					SceneManager.LoadScene (0);
				}

				if (GUILayout.Button ("Exit from game.")) {
					Application.Quit ();
				}

			}, "");
		}
		GUI.skin = null;
	}

	void PlayerUnitCreated (PlayerUnit unit) {
		playerUnit_ = unit;
	}

	void UnitDie (IUnit unit) {
		if (playerUnit_ == unit) {
			playerUnit_ = null;
		}
	}
}