﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : IUnitAction {
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

	public bool IsValid (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		return StaticValidation (map, unitsManager, itemsManager, unit_, direction_);
	}

	public void SetupAnimations (string objectsTag, Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		MessageUtils.SendMessageToObjectsWithTag (objectsTag, "RequestAnimation", 
			new MoveAnimation (unitsManager.GetUnitSprite (unit_), direction_));
	}

	public void Commit (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		unit_.position = unit_.position + direction_;
	}

	public float time { 
		get {
			return 1.0f / unit_.moveSpeed;
		}
	}

	public static bool StaticValidation (Map map, UnitsManager unitsManager, ItemsManager itemsManager,
		IUnit unit, Vector2 direction) {
		if (direction == Vector2.up || direction == Vector2.down ||
		    direction == Vector2.left || direction == Vector2.right) {

			Vector2 newPosition = unit.position + direction;
			Tile tile = map.GetTile (newPosition);
			return unitsManager.GetUnitOnTile (newPosition) == null && tile != null && tile.passable;
		} else {
			return false;
		}
	}

	public IUnit unit {
		get {
			return unit_;
		}
	}
}
