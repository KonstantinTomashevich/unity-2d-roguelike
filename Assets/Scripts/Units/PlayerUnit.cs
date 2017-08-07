using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : UnitBase {
	private List <IAction> thisTurnActions_;

	public PlayerUnit () {
		thisTurnActions_ = new List <IAction> ();
		unitType = UnitType.PLAYER;
	}

	~PlayerUnit () {
	}

	public override IAction[] MakeTurn (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		IAction[] actions = thisTurnActions_.ToArray ();
		thisTurnActions_.Clear ();
		return actions;
	}

	public float CalculateActionsTime () {
		float time = 0.0f;
		foreach (IAction action in thisTurnActions_) {
			if (action.GetType ().IsAssignableFrom (System.Type.GetType ("MoveAction"))) {
				time += (1.0f / moveSpeed);
			} else if (action.GetType ().IsAssignableFrom (System.Type.GetType ("MeleeAttackAction"))) {
				time += (1.0f / attackSpeed);
			}
		}
		return time;
	}

	public void AddAction (IAction action) {
		thisTurnActions_.Add (action);
	}
}
