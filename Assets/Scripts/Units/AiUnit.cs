using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiUnit : UnitBase {
	public const float BASIC_ATTACK_PLAYER_IF_ITS_NEAR_POINTS = 200.0f;
	public const float BASIC_RUN_AWAY_POINTS = 175.0f;
	public const float BASIC_GO_TO_PLAYER_POINTS = 100.0f;

	public const float BASIC_PATROL_POINTS = 100.0f;
	public const float BASIC_REST_POINTS = 50.0f;

	private Vector2 lastFindPathTarget_;
	private List <Vector2> lastFindPathResult_;
	private List <Vector2> patrolTargets_;
	private int currentPatrolTargetIndex_;

	public AiUnit (string unitType, float health = STANDART_UNIT_MAX_HEALTH) : base (unitType, health) {
		lastFindPathTarget_ = Vector2.zero;
		lastFindPathResult_ = new List <Vector2> ();

		patrolTargets_ = new List <Vector2> ();
		currentPatrolTargetIndex_ = 0;
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
		IUnit playerUnit = FindPlayerUnit (map, unitsManager);
		actions.Add ((playerUnit == null) ? BASIC_REST_POINTS : 0.0f, null);
		UpdateCurrentPatrolTarget ();

		AddAttackPlayerIfsItNearAction (map, unitsManager, itemsManager, playerUnit, actions);
		AddGoToPlayerIfItsVisibleAction (map, unitsManager, itemsManager, playerUnit, actions);
		AddRunAwayAction (map, unitsManager, itemsManager, playerUnit, actions);
		AddPatrolAction (map, unitsManager, itemsManager, playerUnit, actions);
		return actions.Values [actions.Count - 1];
	}

	private IUnit FindPlayerUnit (Map map, UnitsManager unitsManager) {
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
		}

		return player;
	}

	private void UpdateCurrentPatrolTarget () {
		if (patrolTargets.Count > 0 && position == patrolTargets_ [currentPatrolTargetIndex_]) {
			currentPatrolTargetIndex_++;
			if (currentPatrolTargetIndex_ >= patrolTargets_.Count) {
				currentPatrolTargetIndex_ = 0;
			}
		}
	}

	private void AddAttackPlayerIfsItNearAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager, 
		IUnit playerUnit, SortedList <float, IAction> actions) {

		if (playerUnit != null) {
			Vector2 direction = playerUnit.position - position;
			if (direction.magnitude == 1.0f) {
				actions [BASIC_ATTACK_PLAYER_IF_ITS_NEAR_POINTS - playerUnit.health] = new MeleeAttackAction (this, direction);
			}
		}
	}

	private void AddGoToPlayerIfItsVisibleAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager,
		IUnit playerUnit, SortedList <float, IAction> actions) {

		if (playerUnit != null) {
			if (lastFindPathResult_.Count == 0 || playerUnit != unitsManager.GetUnitOnTile (lastFindPathTarget_)) {
				lastFindPathTarget_ = playerUnit.position;
				lastFindPathResult_ = map.FindPath (position, lastFindPathTarget_, true);
			}

			if (lastFindPathResult_.Count > 1) {
				Vector2 direction = lastFindPathResult_ [1] - lastFindPathResult_ [0];
				lastFindPathResult_.RemoveAt (0);
				actions [BASIC_GO_TO_PLAYER_POINTS * (1.0f - lastFindPathResult_.Count / visionRange)] = new MoveAction (this, direction);

			} else {
				return;
			}
		}
	}

	private void AddRunAwayAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager, 
		IUnit playerUnit, SortedList <float, IAction> actions) {

		if (playerUnit != null) {
			float runAwayPoints = BASIC_RUN_AWAY_POINTS - health;
			Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

			foreach (Vector2 direction in directions) {
				if ((playerUnit.position - position - direction).magnitude > 1) {
					actions [runAwayPoints] = new MoveAction (this, direction);
					return;
				}
			}
		}
	}

	private void AddPatrolAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager, 
		IUnit playerUnit, SortedList <float, IAction> actions) {

		if (playerUnit == null && patrolTargets_.Count > 0) {
			Vector2 currentPatrolTarget = patrolTargets_ [currentPatrolTargetIndex_];

			if (lastFindPathTarget_ != currentPatrolTarget) {
				lastFindPathTarget_ = currentPatrolTarget;
				bool isGoingToAttackMode = unitsManager.GetUnitOnTile (currentPatrolTarget) != null;
				lastFindPathResult_ = map.FindPath (position, currentPatrolTarget, isGoingToAttackMode);
			}

			if (lastFindPathResult_.Count > 1) {
				Vector2 direction = lastFindPathResult_ [1] - lastFindPathResult_ [0];
				lastFindPathResult_.RemoveAt (0);
				actions [BASIC_PATROL_POINTS * (health / STANDART_UNIT_MAX_HEALTH)] = new MoveAction (this, direction);
			}
			return;
		}
	}

	public List <Vector2> patrolTargets {
		get {
			return patrolTargets_;
		}

		set {
			Debug.Assert (value != null);
			patrolTargets_ = value;
			currentPatrolTargetIndex_ = 0;
		}
	}
}
