using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour {
	public UnitsManager unitsManager;
	public KeyCode keyUp;
	public KeyCode keyDown;
	public KeyCode keyRight;
	public KeyCode keyLeft;

	private PlayerUnit playerUnit_;
	private bool isProcessingTurn_;

	void Start () {
		isProcessingTurn_ = false;
	}

	void Update () {
		if (playerUnit_ != null && !isProcessingTurn_) {
			if (Input.GetKeyUp (keyUp)) {
				AddMoveOrAttackAction (Vector2.up);

			} else if (Input.GetKeyUp (keyDown)) {
				AddMoveOrAttackAction (Vector2.down);

			} else if (Input.GetKeyUp (keyRight)) {
				AddMoveOrAttackAction (Vector2.right);

			} else if (Input.GetKeyUp (keyLeft)) {
				AddMoveOrAttackAction (Vector2.left);
			}

			if (playerUnit_.CalculateActionsTime () >= 1.0f) {
				MessageUtils.SendMessageToObjectsWithTag (tag, "NextTurnRequest", null);
			}
		}
	}

	void PlayerUnitCreated (PlayerUnit unit) {
		playerUnit_ = unit;
	}

	void NextTurnRequest () {
		isProcessingTurn_ = true;
	}

	void TurnFinished () {
		isProcessingTurn_ = false;
	}

	private void AddMoveOrAttackAction (Vector2 direction) {
		if (unitsManager.GetUnitOnTile (playerUnit_.position + direction) != null) {
			playerUnit_.AddAction (new MeleeAttackAction (playerUnit_, direction));
		} else {
			playerUnit_.AddAction (new MoveAction (playerUnit_, direction));
		}
	}
}
