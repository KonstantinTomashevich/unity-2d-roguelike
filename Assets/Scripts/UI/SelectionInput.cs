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
	private float playerElapsedTime_;

	void Start () {
		isProcessingTurn_ = false;
		playerElapsedTime_ = 0.0f;
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
			if (CanMove (direction)) {
				spriteRenderer.color = moveColor;
			} else if (CanAttack (direction)) {
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
		playerElapsedTime_ = 0.0f;
	}

	void TurnFinished () {
		isProcessingTurn_ = false;
	}

	void AllImmediateActionsFinished () {
		isProcessingTurn_ = false;
		// Use 1.00001f instead of 1.0f because 0.8f + 0.2f > 1.0f in C# math. :)
		if (playerElapsedTime_ + MoveAction.StaticTime (playerUnit_) > 1.00001f &&
			playerElapsedTime_ + MeleeAttackAction.StaticTime (playerUnit_) > 1.00001f) {

			MessageUtils.SendMessageToObjectsWithTag (tag, "NextTurnRequest", null);
		}
	}

	void ImmediateActionsMaxTimeReached () {
		MessageUtils.SendMessageToObjectsWithTag (tag, "NextTurnRequest", null);
	}

	void ImmediateActionStart (IAction action) {
		isProcessingTurn_ = true;
		if (action is IUnitAction && (action as IUnitAction).unit == playerUnit_) {
			playerElapsedTime_ += action.time;
		}
	}

	void ScreenPressed () {
		if (playerUnit_ != null && !isProcessingTurn_) {
			Vector2 direction = CalculateDirection ();
			if (CanMove (direction)) {
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionRequest", new MoveAction (playerUnit_, direction));

			} else if (CanAttack (direction)) {
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionRequest", new MeleeAttackAction (playerUnit_, direction));
			}
		}
	}

	private bool CanMove (Vector2 direction) {
		return MoveAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, direction) &&
			playerElapsedTime_ + MoveAction.StaticTime (playerUnit_) <= 1.0001f;
	}

	private bool CanAttack (Vector2 direction) {
		return MeleeAttackAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, direction) &&
			playerElapsedTime_ + MeleeAttackAction.StaticTime (playerUnit_) <= 1.0001f;
	}

	private Vector2 CalculateDirection () {
		return (new Vector2 (transform.position.x, transform.position.y)) - playerUnit_.position;
	}
}
