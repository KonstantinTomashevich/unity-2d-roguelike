using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : IAction {
	private IUnit unit_;
	private Vector2 direction_;

	public MoveAction (IUnit unit, Vector2 direction) {
		unit_ = unit;
		Debug.Assert (direction == Vector2.up || direction == Vector2.down ||
			direction == Vector2.left || direction == Vector2.right);
		direction_ = direction;
	}

	~MoveAction () {
	}

	public void SetupAnimations (string objectsTag, Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		MessageUtils.SendMessageToObjectsWithTag (objectsTag, "RequestAnimation", 
			new MoveAnimation (unitsManager.GetUnitSprite (unit_), direction_));
	}

	public void Commit (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		Vector2 newPosition = unit_.position + direction_;
		if (unitsManager.GetUnitOnTile (newPosition) == null) {
			unit_.position = newPosition;
		}
	}

	public float time { 
		get {
			return 1.0f / unit_.attackSpeed;
		}
	}
}
