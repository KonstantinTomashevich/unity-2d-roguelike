using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoItem : ItemBase {
	public CargoItem (string itemType) : base (itemType) {
	}

	~CargoItem () {
	}

	public override void ProcessTurn (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
	}
}
