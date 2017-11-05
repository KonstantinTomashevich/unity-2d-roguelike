using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit {
	int id { get; set; }
	GameObject unitObject { set; }

	Vector2 position { get; set; }
	float health { get; }
	float regeneration { get; set; }
	string unitType { get; }

	Vector2 attackForce { get; set; }
	float attackSpeed { get; set; }
	float moveSpeed { get; set; }
	float armor { get; set; }

	float maximumInventoryWeight { get; set; }
	uint visionRange { get; set; }
	Texture2D visionMap { get; }

	void ApplyDamage (float damage);
	void TurnBegins ();
	IAction NextAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager);

	void InitVisionMap (int mapWidth, int mapHeight);
	void UpdateVisionMap (Map map);

	bool AddToInventory (IItem item);
	bool CanAddToInventory (IItem item);
	bool RemoveFromInventory (IItem item);

	int GetItemsInInventoryCount ();
	IItem GetItemFromInventoryByIndex (int index);
	int GetCountOfItemsInInventoryWithType (string itemType);
	List <IItem> GetItemsInInventory ();
}
