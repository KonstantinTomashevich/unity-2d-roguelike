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
	private bool cursorShouldBeUpdated_;
	private float playerElapsedTime_;
	private Vector2 lastCursorPosition_;

	void Start () {
		isProcessingTurn_ = false;
		playerElapsedTime_ = 0.0f;
		cursorShouldBeUpdated_ = true;
		lastCursorPosition_ = new Vector2 (transform.position.x, transform.position.y);
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
			cursorShouldBeUpdated_ = true;

		} else {
			Vector2 cursorPosition = new Vector2 (transform.position.x, transform.position.y);
			if (!cursorPosition.Equals (lastCursorPosition_) || cursorShouldBeUpdated_) {
				lastCursorPosition_ = cursorPosition;
				cursorShouldBeUpdated_ = false;

				if (Map.HeuristicDistance (playerUnit_.position, lastCursorPosition_) <= playerUnit_.moveSpeed &&
					lastCursorPosition_ != playerUnit_.position) {

					bool isTileWithEnemy = unitsManager.GetUnitOnTile (cursorPosition) != null;
					List <Vector2> path = map.FindPath (playerUnit_.position, cursorPosition, isTileWithEnemy);
					if (path.Count > 0) {
						path.RemoveAt (0);

						float time = playerElapsedTime_ + MoveAction.StaticTime (playerUnit_) * path.Count;
						if (isTileWithEnemy) {
							time += MeleeAttackAction.StaticTime (playerUnit_);
						}

						if (time <= 1.0f) {
							spriteRenderer.color = isTileWithEnemy ? attackColor : moveColor;
						} else {
							spriteRenderer.color = disabledColor;
						}

					} else {
						spriteRenderer.color = disabledColor;
					}
				} else {
					spriteRenderer.color = disabledColor;
				}
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
			
			Vector2 cursorPosition = new Vector2 (transform.position.x, transform.position.y);
			bool isTileWithEnemy = unitsManager.GetUnitOnTile (cursorPosition) != null;
			List <Vector2> path = map.FindPath (playerUnit_.position, cursorPosition, isTileWithEnemy);

			if (path.Count > 0) {
				path.RemoveAt (0);
			}

			float time = playerElapsedTime_ + MoveAction.StaticTime (playerUnit_) * path.Count;
			if (time <= 1.0f) {
				Vector2 previous = playerUnit_.position;
				foreach (Vector2 step in path) {
					Vector2 direction = step - previous;
					previous = step;
					MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionRequest", new MoveAction (playerUnit_, direction));
				}

				if (isTileWithEnemy) {
					Vector2 direction = cursorPosition - previous;
					MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionRequest", new MeleeAttackAction (playerUnit_, direction));
				}
			}
		}
	}
}
