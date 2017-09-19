using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiUnit : UnitBase {
	private Vector2 lastFindPathTarget_;
	private List <Vector2> lastFindPathResult_;

	public AiUnit (string unitType, float health = STANDART_UNIT_MAX_HEALTH) : base (unitType, health) {
		lastFindPathTarget_ = Vector2.zero;
		lastFindPathResult_ = new List <Vector2> ();
	}

	~AiUnit () {
	}

	public override void TurnBegins () {
		base.TurnBegins ();
		lastFindPathTarget_ = Vector2.zero;
		lastFindPathResult_.Clear ();
	}

	public override IAction NextAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		IAction action = AttackPlayerIfsItNear (map, unitsManager, itemsManager);
		if (action == null) {
			action = GoToPlayerIfItsVisible (map, unitsManager, itemsManager);
		}
		return action;
	}

	private IAction AttackPlayerIfsItNear (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
		foreach (Vector2 direction in directions) {
			IUnit unit = unitsManager.GetUnitOnTile (position + direction);
			if (unit != null && unit.unitType.Equals ("player")) {
				return new MeleeAttackAction (this, direction);
			}
		}
		return null;
	}

	private IAction GoToPlayerIfItsVisible (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		IUnit player = null;
		if (lastFindPathResult_.Count > 0) {
			player = unitsManager.GetUnitOnTile (lastFindPathTarget_);
		}

		if (player == null || player.unitType != "player") {
			player = null;

			foreach (KeyValuePair <Vector2, uint> positionPair in lastVisionMapUpdateVisibleTiles) {
				Vector2 scanningPosition = positionPair.Key;
				IUnit unitOnPosition = unitsManager.GetUnitOnTile (scanningPosition);

				if (unitOnPosition != null && unitOnPosition.unitType == "player") {
					player = unitOnPosition;
					break;
				}
			}

			if (player != null) {
				lastFindPathTarget_ = player.position;
				lastFindPathResult_ = map.FindPath (position, lastFindPathTarget_, true);
			}
		}

		if (lastFindPathResult_.Count > 1) {
			Vector2 direction = lastFindPathResult_ [1] - lastFindPathResult_ [0];
			lastFindPathResult_.RemoveAt (0);
			return new MoveAction (this, direction);

		} else {
			return null;
		}
	}
}
