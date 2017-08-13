using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanelHandler : MonoBehaviour {
	public Scrollbar healthBar;
	public Text healthText;
	public Text armorText;
	public Text attackText;
	public float floorPrecision;

	private IUnit playerUnit_;

	void Start () {
		playerUnit_ = null;
	}

	void Update () {
		if (playerUnit_ != null) {
			healthBar.size = playerUnit_.health / UnitBase.STANDART_UNIT_MAX_HEALTH;
			healthText.text = Mathf.Round (playerUnit_.health / floorPrecision) * floorPrecision + " HP";
			armorText.text = Mathf.Round (playerUnit_.armor / floorPrecision) * floorPrecision + 
				"    " + Mathf.Floor (playerUnit_.moveSpeed) + " moves per turn";

			attackText.text = Mathf.Floor (playerUnit_.attackForce.x / floorPrecision) * floorPrecision + "-" +
				Mathf.Round (playerUnit_.attackForce.y / floorPrecision) * floorPrecision + " (" +
				Mathf.Floor (playerUnit_.attackSpeed) + " attacks per turn)";
		}
	}

	void PlayerUnitCreated (IUnit unit) {
		playerUnit_ = unit;
	}
}
