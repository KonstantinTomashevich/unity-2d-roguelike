using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitsManager : MonoBehaviour {
	public Map map;
	public ItemsManager itemsManager;
	public UnitTypeData[] unitsTypesData;

	Dictionary <int, IUnit> units_;
	Dictionary <int, GameObject> unitsSprites_;

	void Start () {
		units_ = new Dictionary <int, IUnit> ();
		unitsSprites_ = new Dictionary <int, GameObject> ();
	}

	void Update () {
		
	}

	public void AddUnit (IUnit unit) {
		int id = units_.Count + 1;
		while (units_.ContainsKey (id)) {
			id++;
		}

		units_.Add (id, unit);
		unit.id = id;

		GameObject spriteObject = new GameObject ("unit" + id);
		spriteObject.transform.SetParent (transform);
		spriteObject.transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
		unitsSprites_.Add (id, spriteObject);

		UnitTypeData unitTypeData = GetUnitTypeData (unit.unitType);
		Debug.Assert (!unitTypeData.Equals (UnitTypeData.EMPTY));

		SpriteRenderer spriteRenderer = spriteObject.AddComponent <SpriteRenderer> ();
		spriteRenderer.sprite = unitTypeData.sprite;
		spriteRenderer.drawMode = SpriteDrawMode.Sliced;
		spriteRenderer.size = Vector2.one;
	}

	public bool RemoveUnit (int id) {
		return units_.Remove (id);
	}

	private UnitTypeData GetUnitTypeData (UnitType unitType) {
		foreach (UnitTypeData unitTypeData in unitsTypesData) {
			if (unitTypeData.unitType == unitType) {
				return unitTypeData;
			}
		}
		return UnitTypeData.EMPTY;
	}
}
