using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem {
	string itemType { get; set; }
	GameObject itemObject { set; }

	Vector2 position { get; set; }
	IUnit holder { get; }

	bool passable { get; set; }
	bool destroyable { get; set; }
	bool pickable { get; set; }

	void ProcessTurn (Map map, UnitsManager unitsManager, ItemsManager itemsManager);
	void Destroy (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit destroyer);
	void Pick (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit pickuper);
	void Throw (Map map, UnitsManager unitsManager, ItemsManager itemsManager, Vector2 direction);
}
