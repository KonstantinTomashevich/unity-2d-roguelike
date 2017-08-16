using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : UnitBase {
	public PlayerUnit (float health = STANDART_UNIT_MAX_HEALTH) : base ("player", health) {
	}

	~PlayerUnit () {
	}

	public override IAction NextAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		return null;
	}
}
