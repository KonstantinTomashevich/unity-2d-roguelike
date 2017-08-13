using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class UnitsManager : MonoBehaviour {
	public Map map;
	public ItemsManager itemsManager;

	private Dictionary <int, IUnit> units_;
	private Dictionary <int, GameObject> unitsSprites_;
	private Dictionary <string, UnitTypeData> unitsTypesData_;

	private int currentProcessingUnitIndex_;
	private IAction[] currentProcessingUnitActions_;
	private int currentProcessingActionIndex_;
	private float currentProcessingElapsedTime_;

	public UnitsManager () {
		unitsTypesData_ = new Dictionary <string, UnitTypeData> ();
	}

	~UnitsManager () {
	}

	void Start () {
		units_ = new Dictionary <int, IUnit> ();
		unitsSprites_ = new Dictionary <int, GameObject> ();

		currentProcessingUnitIndex_ = -1;
		currentProcessingActionIndex_ = -1;
		currentProcessingElapsedTime_ = 0.0f;
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

		UnitTypeData unitTypeData = unitsTypesData_ [unit.unitType];
		Debug.Assert (unitTypeData != null);

		unit.armor = unitTypeData.defaultArmor;
		unit.attackForce = unitTypeData.defaultAttackForce;
		unit.moveSpeed = unitTypeData.defaultMoveSpeed;
		unit.attackSpeed = unitTypeData.defaultAttackSpeed;

		SpriteRenderer spriteRenderer = spriteObject.AddComponent <SpriteRenderer> ();
		spriteRenderer.sprite = unitTypeData.sprite;
		spriteRenderer.drawMode = SpriteDrawMode.Sliced;
		spriteRenderer.size = Vector2.one;
	}

	public bool RemoveUnit (int id) {
		bool exists = units_.Remove (id) && unitsSprites_.ContainsKey (id);
		if (exists) {
			GameObject spriteObject = unitsSprites_ [id];
			unitsSprites_.Remove (id);
			Destroy (spriteObject);
		}
		return exists;
	}

	public IUnit GetUnitById (int id) {
		if (units_.ContainsKey (id)) {
			return units_ [id];
		} else {
			return null;
		}
	}

	public IUnit GetUnitOnTile (Vector2 tilePosition) {
		foreach (IUnit unit in units_.Values) {
			if (unit.position.Equals (tilePosition)) {
				return unit;
			}
		}
		return null;
	}

	public GameObject GetUnitSpriteById (int id) {
		if (unitsSprites_.ContainsKey (id)) {
			return unitsSprites_ [id];
		} else {
			return null;
		}
	}

	public GameObject GetUnitSprite (IUnit unit) {
		return GetUnitSpriteById (unit.id);
	}

	public PlayerUnit SpawnPlayerFromXml (XmlNode xml) {
		PlayerUnit playerUnit = SpawnUnitFromXml <PlayerUnit> (xml, (unitType, health) => new PlayerUnit (health));
		MessageUtils.SendMessageToObjectsWithTag (tag, "PlayerUnitCreated", playerUnit);
		return playerUnit;
	}

	public AiUnit SpawnAiUnitFromXml (XmlNode xml) {
		return SpawnUnitFromXml <AiUnit> (xml, (unitType, health) => new AiUnit (unitType, health));;
	}

	public void ProcessXmlSpawner (XmlNode xml) {
	}

	void LoadUnitsTypes (XmlNode rootNode) {
		string spritesPathPrefix = rootNode.Attributes ["spritesPrefix"].InnerText;
		foreach (XmlNode node in rootNode.ChildNodes) {
			unitsTypesData_ [node.LocalName] = new UnitTypeData (node, spritesPathPrefix);
		}
	}

	void NextTurnRequest () {
		currentProcessingUnitIndex_ = 0;
		currentProcessingActionIndex_ = -1;
		currentProcessingElapsedTime_ = 0.0f;
		ProcessNextUnitTurn ();
	}

	void AllAnimationsFinished () {
		ProcessCurrentActionAndStartNext ();
	}

	private void ProcessNextUnitTurn () {
		CorrectPreviousUnitSpritePosition ();
		IUnit unit = null;
		while (currentProcessingUnitIndex_ < units_.Count && unit == null) {
			unit = GetUnitByIndex (currentProcessingUnitIndex_);
			if (unit.health <= 0.0f) {
				RemoveUnit (unit.id);
				unit = null;
			}
		}

		if (unit != null) {
			currentProcessingUnitActions_ = unit.MakeTurn (map, this, itemsManager);
			currentProcessingElapsedTime_ = 0.0f;
			currentProcessingActionIndex_ = 0;
			SetupNextAction ();

		} else {
			MessageUtils.SendMessageToObjectsWithTag (tag, "TurnFinished", null);
		}
	}

	private T SpawnUnitFromXml <T> (XmlNode xml, System.Func <string, float, T> Construct) where T : IUnit {
		T unit = Construct (xml.Attributes ["type"].InnerText, float.Parse (xml.Attributes ["health"].InnerText));
		unit.position = map.GetWorldTransformFromXml (xml);
		AddUnit (unit);

		if (xml.Attributes ["deltaMoveSpeed"] != null) {
			unit.moveSpeed += float.Parse (xml.Attributes ["deltaMoveSpeed"].InnerText);
		}

		if (xml.Attributes ["deltaAttackSpeed"] != null) {
			unit.attackSpeed += float.Parse (xml.Attributes ["deltaAttackSpeed"].InnerText);
		}

		if (xml.Attributes ["deltaAttack"] != null) {
			float delta = float.Parse (xml.Attributes ["deltaAttack"].InnerText);
			unit.attackForce += new Vector2 (delta, delta);
		}

		if (xml.Attributes ["deltaArmor"] != null) {
			unit.armor += float.Parse (xml.Attributes ["deltaArmor"].InnerText);
		}
		return unit;
	}

	private void ProcessCurrentActionAndStartNext () {
		IAction action = currentProcessingUnitActions_ [currentProcessingActionIndex_];
		action.Commit (map, this, itemsManager);
		currentProcessingActionIndex_++;

		IUnit unit = GetUnitByIndex (currentProcessingUnitIndex_);
		if (unit.health <= 0.0f) {
			RemoveUnit (unit.id);
			ProcessNextUnitTurn ();
		} else {
			SetupNextAction ();
		}
	}

	private void SetupNextAction () {
		IAction next = null;
		while (next == null && currentProcessingActionIndex_ < currentProcessingUnitActions_.Length) {
			next = currentProcessingUnitActions_ [currentProcessingActionIndex_];
			if (!next.IsValid (map, this, itemsManager)) {
				next = null;
				currentProcessingActionIndex_++;
			}
		}

		if (next != null) {
			currentProcessingElapsedTime_ += next.time;
		}

		if (currentProcessingActionIndex_ >= currentProcessingUnitActions_.Length || currentProcessingElapsedTime_ > 1.0f) {
			currentProcessingUnitIndex_++;
			ProcessNextUnitTurn ();
		} else {
			next.SetupAnimations (tag, map, this, itemsManager);
		}
	}

	private IUnit GetUnitByIndex (int index) {
		int currentIndex = 0;
		foreach (KeyValuePair <int, IUnit> pair in units_) {
			if (currentIndex == index) {
				return pair.Value;
			}
			currentIndex++;
		}
		return null;
	}

	private void CorrectPreviousUnitSpritePosition () {
		if (currentProcessingUnitIndex_ > 0) {
			IUnit unit = GetUnitByIndex (currentProcessingUnitIndex_ - 1);
			unitsSprites_ [unit.id].transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
		}
	}
}
