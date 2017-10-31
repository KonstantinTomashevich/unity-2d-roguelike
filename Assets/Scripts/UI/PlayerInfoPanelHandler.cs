using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanelHandler : MonoBehaviour {
	public GUISkin skin;
	public Texture2D attackTexture;
	public Texture2D armorTexture;
	public float floorPrecision;

	private IUnit playerUnit_;

	void Start () {
		playerUnit_ = null;
	}

	void Update () {
	}

	void OnGUI () {
		GUI.skin = skin;

		int W = Screen.width;
		int H = Screen.height;

		skin.label.fontSize = H / 30;
		skin.button.fontSize = H / 23;
		skin.GetStyle ("healthSliderButton").fontSize = H / 23;
		skin.GetStyle ("title").fontSize = H / 15;

		if (playerUnit_ != null) {

			GUILayout.Window (0, new Rect (0, 0, H / 2.0f, H / 4.0f), (int id) => {
				// HP info
				GUILayout.Button ("HP: " + RoundWithPrecision (playerUnit_.health) + "/" + UnitBase.STANDART_UNIT_MAX_HEALTH,
					skin.GetStyle ("healthSliderButton"), GUILayout.Width ((H / 2.0f - 20) * playerUnit_.health / UnitBase.STANDART_UNIT_MAX_HEALTH));

				// Attack info
				GUILayout.Space (H / 24.0f);
				GUILayout.BeginHorizontal ();
				GUI.DrawTexture (new Rect (10, H / 8.0f, H / 12.0f, H / 12.0f), attackTexture);
				GUILayout.Space (H / 12.0f);

				GUILayout.Label (RoundWithPrecision (playerUnit_.attackForce.x) + "-" +
					RoundWithPrecision (playerUnit_.attackForce.y) + ", " + 
					RoundWithPrecision (1.0f / playerUnit_.attackSpeed) + " attacks per turn");
				GUILayout.EndHorizontal ();

				// Armor info
				GUILayout.Space (H / 24.0f);
				GUILayout.BeginHorizontal ();
				GUI.DrawTexture (new Rect (10, H / 4.5f, H / 12.0f, H / 12.0f), armorTexture);
				GUILayout.Space (H / 12.0f);

				GUILayout.Label (RoundWithPrecision (playerUnit_.armor) + ", " + 
					RoundWithPrecision (1.0f / playerUnit_.moveSpeed) + " moves per turn");
				GUILayout.EndHorizontal ();
			}, "");
		}
		GUI.skin = null;
	}

	void PlayerUnitCreated (IUnit unit) {
		playerUnit_ = unit;
	}

	private float RoundWithPrecision (float value) {
		return Mathf.Round (value / floorPrecision) * floorPrecision;
	}
}
