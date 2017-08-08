using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAction : IAction {
	private IUnit unit_;
	private Vector2 direction_;

	public MeleeAttackAction (IUnit unit, Vector2 direction) {
		unit_ = unit;
		Debug.Assert (direction == Vector2.up || direction == Vector2.down ||
			direction == Vector2.left || direction == Vector2.right);
		direction_ = direction;
	}

	~MeleeAttackAction () {
	}

	public void Commit (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		Vector2 attackPosition = unit_.position + direction_;
		IUnit attacked = unitsManager.GetUnitOnTile (attackPosition);
		if (attacked != null) {
			attacked.ApplyDamage (Random.Range (unit_.attackForce.x, unit_.attackForce.y));
		}
	}
}
