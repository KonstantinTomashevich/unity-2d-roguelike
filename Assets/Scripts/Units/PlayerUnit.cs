using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : UnitBase {
	private List <IAction> thisTurnActions_;

	public PlayerUnit (float health = STANDART_UNIT_MAX_HEALTH) : base ("player", health) {
		thisTurnActions_ = new List <IAction> ();
	}

	~PlayerUnit () {
	}

	public override IAction NextAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		if (thisTurnActions_.Count > 0) {
			IAction action = thisTurnActions_ [0];
			thisTurnActions_.RemoveAt (0);
			return action;
		} else {
			return null;
		}
	}

	public float CalculateActionsTime () {
		float time = 0.0f;
		foreach (IAction action in thisTurnActions_) {
			time += action.time;
		}
		return time;
	}

	public void AddAction (IAction action) {
		thisTurnActions_.Add (action);
	}

	public void ClearActions () {
		thisTurnActions_.Clear ();
	}
}
