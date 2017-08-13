using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit {
	int id { get; set; }
	Vector2 position { get; set; }
	float health { get; }
	string unitType { get; }

	Vector2 attackForce { get; }
	float attackSpeed { get; }
	float moveSpeed { get; }
	float armor { get; }

	void ApplyDamage (float damage);
	IAction[] MakeTurn (Map map, UnitsManager unitsManager, ItemsManager itemsManager);
}
