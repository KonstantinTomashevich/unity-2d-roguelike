using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem {
	Vector2 GetPosition ();
	bool IsPassable ();
	bool IsDestroyable ();
	bool IsPickable ();

	void Destroy (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit destroyer);
	void Pickup (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit pickuper);
}
