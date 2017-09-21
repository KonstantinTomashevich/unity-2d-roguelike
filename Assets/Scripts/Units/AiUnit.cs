using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiUnit : UnitBase {
	public const float BASIC_ATTACK_PLAYER_IF_ITS_NEAR_POINTS = 200.0f;
	public const float BASIC_GO_TO_PLAYER_POINTS = 100.0f;

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
		SortedList <float, IAction> actions = new SortedList <float, IAction> ();
		actions.Add (0.0f, null);

		AddAttackPlayerIfsItNearAction (map, unitsManager, itemsManager, actions);
		AddGoToPlayerIfItsVisibleAction (map, unitsManager, itemsManager, actions);
		return actions.Values [actions.Count - 1];
	}

	private void AddAttackPlayerIfsItNearAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager, SortedList <float, IAction> actions) {
		Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
		foreach (Vector2 direction in directions) {
			IUnit unit = unitsManager.GetUnitOnTile (position + direction);

			if (unit != null && unit.unitType.Equals ("player")) {
				actions.Add (BASIC_ATTACK_PLAYER_IF_ITS_NEAR_POINTS - unit.health, new MeleeAttackAction (this, direction));
				return;
			}
		}
	}

	private void AddGoToPlayerIfItsVisibleAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager, SortedList <float, IAction> actions) {
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
			actions.Add (BASIC_GO_TO_PLAYER_POINTS * (1.0f - lastFindPathResult_.Count / visionRange), new MoveAction (this, direction));

		} else {
			return;
		}
	}
}
