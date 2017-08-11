using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionInput : MonoBehaviour {
	public Camera mainCamera;
	public Map map;
	public UnitsManager unitsManager;
	public ItemsManager itemsManager;

	public Color disabledColor;
	public Color moveColor;
	public Color attackColor;

	private PlayerUnit playerUnit_;
	private bool isProcessingTurn_;

	void Start () {
		isProcessingTurn_ = false;
	}

	void Update () {
		Vector3 selectionPosition = mainCamera.ScreenToWorldPoint (Input.mousePosition);
		selectionPosition.x = Mathf.Round (selectionPosition.x);
		selectionPosition.y = Mathf.Round (selectionPosition.y);
		selectionPosition.z = 0;
		transform.position = selectionPosition;

		SpriteRenderer spriteRenderer = gameObject.GetComponent <SpriteRenderer> ();
		if (isProcessingTurn_ || playerUnit_ == null) {
			spriteRenderer.color = disabledColor;
		} else {
			
			Vector2 direction = CalculateDirection ();
			if (MoveAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, direction)) {
				spriteRenderer.color = moveColor;
			} else if (MeleeAttackAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, direction)) {
				spriteRenderer.color = attackColor;
			} else {
				spriteRenderer.color = disabledColor;
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

	void ScreenPressed () {
		if (playerUnit_ != null && !isProcessingTurn_) {
			Vector2 direction = CalculateDirection ();
			if (MoveAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, direction)) {
				playerUnit_.AddAction (new MoveAction (playerUnit_, direction));

			} else if (MeleeAttackAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, direction)) {
				playerUnit_.AddAction (new MeleeAttackAction (playerUnit_, direction));
			}

			if (playerUnit_.CalculateActionsTime () >= 1.0f) {
				MessageUtils.SendMessageToObjectsWithTag (tag, "NextTurnRequest", null);
			}
		}
	}

	private Vector2 CalculateDirection () {
		return (new Vector2 (transform.position.x, transform.position.y)) - playerUnit_.position;
	}
}
