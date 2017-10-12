using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class UnitsManager : MonoBehaviour {
	public Map map;
	public ItemsManager itemsManager;

	private List <IUnit> units_;
	private Dictionary <int, GameObject> unitsObjects_;
	private Dictionary <string, UnitTypeData> unitsTypesData_;
	private IUnit visionMapProviderUnit_;

	public UnitsManager () {
		unitsTypesData_ = new Dictionary <string, UnitTypeData> ();
		visionMapProviderUnit_ = null;
	}

	~UnitsManager () {
	}

	void Start () {
		units_ = new List <IUnit> ();
		unitsObjects_ = new Dictionary <int, GameObject> ();
	}

	void Update () {
	}

	public void AddUnit (IUnit unit) {
		int id = units_.Count + 1;
		if (id > 1) {
			while (units_ [units_.Count - 1].id == id) {
				id++;
			}
		}

		units_.Add (unit);
		unit.id = id;
		GameObject spriteObject = new GameObject ("unit" + id);
		unit.unitObject = spriteObject;

		spriteObject.transform.SetParent (transform);
		spriteObject.transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
		unitsObjects_.Add (id, spriteObject);

		UnitTypeData unitTypeData = unitsTypesData_ [unit.unitType];
		Debug.Assert (unitTypeData != null);

		unit.armor = unitTypeData.defaultArmor;
		unit.regeneration = unitTypeData.defaultRegeneration;
		unit.attackForce = unitTypeData.defaultAttackForce;
		unit.moveSpeed = unitTypeData.defaultMoveSpeed;
		unit.attackSpeed = unitTypeData.defaultAttackSpeed;
		unit.visionRange = unitTypeData.defaultVisionRange;

		unit.InitVisionMap (map.width, map.height);
		SpriteRenderer spriteRenderer = spriteObject.AddComponent <SpriteRenderer> ();
		spriteRenderer.sprite = unitTypeData.sprite;
		spriteRenderer.drawMode = SpriteDrawMode.Sliced;
		spriteRenderer.size = Vector2.one;
	}

	public int RemoveUnit (int id) {
		int index = IndexOfUnit (id);
		if (index != -1) {
			
			IUnit unit = units_ [index];
			MessageUtils.SendMessageToObjectsWithTag (tag, "UnitDie", unit);
			units_.RemoveAt (index);

			GameObject spriteObject = unitsObjects_ [id];
			unitsObjects_.Remove (id);
			Destroy (spriteObject);
		}

		return index;
	}

	public IUnit GetUnitById (int id) {
		return GetUnitByIndex (IndexOfUnit (id));
	}

	public IUnit GetUnitOnTile (Vector2 tilePosition) {
		foreach (IUnit unit in units_) {
			if (unit.position.Equals (tilePosition)) {
				return unit;
			}
		}
		return null;
	}

	public int GetUnitsCount () {
		return units_.Count;
	}

	public IUnit GetUnitByIndex (int index) {
		return (index > -1 && index < units_.Count) ? units_ [index] : null;
	}

	public GameObject GetUnitObjectById (int id) {
		GameObject result;
		return unitsObjects_.TryGetValue (id, out result) ? result : null;
	}

	public GameObject GetUnitObject (IUnit unit) {
		return GetUnitObjectById (unit.id);
	}

	public void LoadUnitsTypes (XmlNode rootNode) {
		string spritesPathPrefix = rootNode.Attributes ["spritesPrefix"].InnerText;
		foreach (XmlNode node in rootNode.ChildNodes) {
			unitsTypesData_ [node.LocalName] = new UnitTypeData (node, spritesPathPrefix);
		}
	}

	public PlayerUnit SpawnPlayerFromXml (XmlNode xml, bool updateVisionMap = true) {
		PlayerUnit playerUnit = SpawnUnitFromXml <PlayerUnit> (xml, (unitType, health) => new PlayerUnit (health));
		MessageUtils.SendMessageToObjectsWithTag (tag, "PlayerUnitCreated", playerUnit);

		if (updateVisionMap) {
			playerUnit.UpdateVisionMap (map);
		}

		SetVisionMapProviderUnit (playerUnit);
		return playerUnit;
	}

	public AiUnit SpawnAiUnitFromXml (XmlNode xml, bool updateVisionMap = true) {
		AiUnit unit = SpawnUnitFromXml <AiUnit> (xml, (unitType, health) => new AiUnit (unitType, health));

		if (updateVisionMap) {
			unit.UpdateVisionMap (map);
		}

		GameObject unitObject = unitsObjects_ [unit.id];
		GameObject textObject = new GameObject ("infoText");
		textObject.transform.SetParent (unitObject.transform);
		textObject.transform.localPosition = new Vector3 (0.0f, 0.5f, 0.0f);

		TextMesh unitText = textObject.AddComponent <TextMesh> ();
		unitText.offsetZ = -1.0f;
		unitText.characterSize = 0.15f;
		unitText.anchor = TextAnchor.UpperCenter;
		unitText.text = unit.unitType +  ": " + unit.health + " HP";
		return unit;
	}
		
	public void ProcessXmlSpawner (XmlNode xml) {
		int count = Random.Range (XmlHelper.GetIntAttribute (xml, "minCount"), XmlHelper.GetIntAttribute (xml, "maxCount"));
		Rect spawnRect = XmlHelper.GetRectAttribute (xml, "worldRect");

		int minPatrolTargets = 0;
		int maxPatrolTargets = 0;

		if (XmlHelper.HasAttribute (xml, "generatePatrols") && XmlHelper.GetBoolAttribute (xml, "generatePatrols")) {
			minPatrolTargets = XmlHelper.GetIntAttribute (xml, "minPatrolTargets");
			maxPatrolTargets = XmlHelper.GetIntAttribute (xml, "maxPatrolTargets");

			if (minPatrolTargets < 2) {
				minPatrolTargets = 2;
			}
		}

		for (int index = 0; index < count; index++) {
			Vector2 spawnPosition = GetValidSpawnPosition (spawnRect);

			AiUnit unit = SpawnAiUnitFromXml (xml, false);
			unit.position = spawnPosition;
			unitsObjects_ [unit.id].transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
			unit.UpdateVisionMap (map);

			if (minPatrolTargets != 0 && maxPatrolTargets != 0) {
				int patrolTargetsCount = Random.Range (minPatrolTargets, maxPatrolTargets + 1);
				List <Vector2> patrolTargets = new List <Vector2> ();

				patrolTargets.Add (spawnPosition);
				patrolTargetsCount--;

				while (patrolTargetsCount > 0) {
					Vector2 patrolTarget = GetValidSpawnPosition (spawnRect);
					patrolTargets.Add (patrolTarget);
					patrolTargetsCount--;
				}
				unit.patrolTargets = patrolTargets;
			}
		}
	}

	public void UpdateUnitsSpritesByVisionMap () {
		if (visionMapProviderUnit_ != null) {
			foreach (IUnit unit in units_) {
				Vector2 mapCoords = map.RealCoordsToMapCoords (unit.position);
				unitsObjects_ [unit.id].SetActive (visionMapProviderUnit_.visionMap.GetPixel (
						Mathf.RoundToInt (mapCoords.x), Mathf.RoundToInt (mapCoords.y)) == UnitBase.VISIBLE_COLOR);
			}
		}
	}

	private int IndexOfUnit (int id) {
		int currentIndex = 0;
		foreach (IUnit unit in units_) {

			if (unit.id == id) {
				return currentIndex;
			}
			currentIndex++;
		}
		return -1;
	}
		
	private T SpawnUnitFromXml <T> (XmlNode xml, System.Func <string, float, T> Construct) where T : IUnit {
		T unit = Construct (xml.Attributes ["type"].InnerText, XmlHelper.GetFloatAttribute (xml, "health"));
		unit.position = map.GetWorldTransformFromXml (xml);
		AddUnit (unit);

		if (XmlHelper.HasAttribute (xml, "deltaMoveSpeed")) {
			unit.moveSpeed += XmlHelper.GetFloatAttribute (xml, "deltaMoveSpeed");
		}

		if (XmlHelper.HasAttribute (xml, "deltaAttackSpeed")) {
			unit.attackSpeed += XmlHelper.GetFloatAttribute (xml, "deltaAttackSpeed");
		}

		if (XmlHelper.HasAttribute (xml, "deltaAttack")) {
			unit.attackForce += XmlHelper.GetVector2Attribute (xml, "deltaMinAttack", "deltaMaxAttack");
		}

		if (XmlHelper.HasAttribute (xml, "deltaArmor")) {
			unit.armor += XmlHelper.GetFloatAttribute (xml, "deltaArmor");
		}

		if (XmlHelper.HasAttribute (xml, "deltaRegeneration")) {
			unit.regeneration += XmlHelper.GetFloatAttribute (xml, "deltaRegeneration");
		}

		if (XmlHelper.HasAttribute (xml, "deltaVisionRange")) {
			int visionRange = (int) unit.visionRange;
			visionRange += XmlHelper.GetIntAttribute (xml, "deltaVisionRange");

			if (visionRange < 1) {
				visionRange = 1;
			}

			unit.visionRange = (uint) visionRange;
		}

		if (XmlHelper.HasAttribute (xml, "deltaMaximumInventoryWeight")) {
			unit.maximumInventoryWeight += XmlHelper.GetFloatAttribute (xml, "deltaMaximumInventoryWeight");
		}

		return unit;
	}

	private void SetVisionMapProviderUnit (IUnit unit) {
		map.GetComponent <MeshRenderer> ().material.SetFloat ("_MapWidth", map.width);
		map.GetComponent <MeshRenderer> ().material.SetFloat ("_MapHeight", map.height);
		map.GetComponent <MeshRenderer> ().material.SetTexture ("_FogOfWar", unit.visionMap); 
		visionMapProviderUnit_ = unit;;
	}

	private Vector2 GetValidSpawnPosition (Rect positionRect) {
		Vector2 position = Vector2.zero;
		Tile tile = null;

		do {
			position.x = Mathf.Round (Random.Range (positionRect.xMin, positionRect.xMax));
			position.y = Mathf.Round (Random.Range (positionRect.yMin, positionRect.yMax));
			tile = map.GetTile (position);

		} while (GetUnitOnTile (position) != null || tile == null || !tile.passable);
		return position;
	}

	public IUnit visionMapProvider {
		get {
			return visionMapProviderUnit_;
		}
	}
}
