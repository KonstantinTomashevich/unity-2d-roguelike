using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit {
	int id { get; set; }
	Vector2 position { get; set; }
	float health { get; }
	string unitType { get; }

	Vector2 attackForce { get; set; }
	float attackSpeed { get; set; }
	float moveSpeed { get; set; }
	float armor { get; set; }

	void ApplyDamage (float damage);
	IAction NextAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager);
}
