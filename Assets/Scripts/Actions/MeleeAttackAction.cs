using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAction : IUnitAction {
	private IUnit unit_;
	private Vector2 direction_;

	public MeleeAttackAction (IUnit unit, Vector2 direction) {
		unit_ = unit;
		Debug.Assert (direction == Vector2.up || direction == Vector2.down ||
			direction == Vector2.left || direction == Vector2.right);
		direction_ = direction;
	}

	~MeleeAttackAction () {
	}

	public bool IsValid (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		return StaticValidation (map, unitsManager, itemsManager, unit_, direction_);
	}

	public void SetupAnimations (string objectsTag, Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		MessageUtils.SendMessageToObjectsWithTag (objectsTag, "RequestAnimation", 
			new MeleeAttackAnimation (unitsManager.GetUnitObject (unit_), direction_));
	}

	public void Commit (Map map, UnitsManager unitsManager, ItemsManager itemsManager) {
		Vector2 attackPosition = unit_.position + direction_;
		IUnit attacked = unitsManager.GetUnitOnTile (attackPosition);
		attacked.ApplyDamage (Random.Range (unit_.attackForce.x, unit_.attackForce.y));

		GameObject attackedUnitObject = unitsManager.GetUnitObject (attacked);
		TextMesh attackedUnitText = attackedUnitObject.transform.GetComponentInChildren <TextMesh> ();
		if (attackedUnitText != null) {
			attackedUnitText.text = attacked.unitType + ": " + Mathf.FloorToInt (attacked.health) + " HP";
		}
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
		IUnit unit, Vector2 direction) {
		if (direction == Vector2.up || direction == Vector2.down ||
			direction == Vector2.left || direction == Vector2.right) {

			return unitsManager.GetUnitOnTile (unit.position + direction) != null;
		} else {
			return false;
		}
	}

	public static float StaticTime (IUnit unit) {
		return 1.0f / unit.attackSpeed;
	}
}
