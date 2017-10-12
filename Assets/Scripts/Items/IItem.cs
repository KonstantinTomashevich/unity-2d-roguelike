using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem {
	int id { get; set; }
	string itemType { get; }
	GameObject itemObject { set; }

	Vector2 position { get; set; }
	IUnit holder { get; }
	float weight { get; set; }

	bool passable { get; set; }
	bool destructable { get; set; }
	bool pickable { get; set; }

	void ProcessTurn (Map map, UnitsManager unitsManager, ItemsManager itemsManager);
	void Destruct (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit destructor);
	void Pick (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit pickuper);
	void Throw (Map map, UnitsManager unitsManager, ItemsManager itemsManager, Vector2 direction);
}
