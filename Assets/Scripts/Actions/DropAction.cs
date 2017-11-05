using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropAction : IUnitAction {
	private IUnit unit_;
	private IItem item_;

	public DropAction (IUnit unit, IItem item) {
		unit_ = unit;
		item_ = item;
	}

	~DropAction () {
	}

	public bool IsValid (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		return StaticValidation (map, unitsManager, itemsManager, unit_, item_);
	}

	public void SetupAnimations (string objectsTag, Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		MessageUtils.SendMessageToObjectsWithTag (objectsTag, "AllAnimationsFinished", null);
	}

	public void Commit (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		item_.Throw (map, unitsManager, itemsManager, Vector2.zero);
	}

	public float time { 
		get {
			return StaticTime (unit_);
		}
	}

	public IUnit unit {
		get {
			return unit_;
		}
	}

	public static bool StaticValidation (Map map, UnitsManager unitsManager, ItemsManager itemsManager,
		IUnit unit, IItem item) {

		return unit != null && item != null && item.holder == unit;
	}

	public static float StaticTime (IUnit unit) {
		return 1.0f;
	}
}