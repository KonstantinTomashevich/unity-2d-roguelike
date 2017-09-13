using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiUnit : UnitBase {
	public AiUnit (string unitType, float health = STANDART_UNIT_MAX_HEALTH) : base (unitType, health) {
	}

	~AiUnit () {
	}

	public override IAction NextAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
		Vector2 selectedDirection;

		foreach (Vector2 direction in directions) {
			IUnit unit = unitsManager.GetUnitOnTile (position + direction);
			if (unit != null && unit.unitType.Equals ("player")) {
				return new MeleeAttackAction (this, direction);
			}
		}
		return null;
	}
}
