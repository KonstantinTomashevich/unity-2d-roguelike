using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class UnitsManager : MonoBehaviour {
	public Map map;
	public ItemsManager itemsManager;

	private Dictionary <int, IUnit> units_;
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
		units_ = new Dictionary <int, IUnit> ();
		unitsObjects_ = new Dictionary <int, GameObject> ();
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
		int indexOfUnit = 0;
		bool exists = false;

		foreach (KeyValuePair <int, IUnit> unitPair in units_) {
			if (unitPair.Value.id == id) {
				exists = true;
				break;
			}
			indexOfUnit++;
		}

		if (exists) {
			IUnit unit = units_ [id];
			MessageUtils.SendMessageToObjectsWithTag (tag, "UnitDie", unit);
			units_.Remove (id);

			GameObject spriteObject = unitsObjects_ [id];
			unitsObjects_.Remove (id);
			Destroy (spriteObject);
		}
		return exists ? indexOfUnit : -1;
	}

	public IUnit GetUnitById (int id) {
		IUnit result;
		return units_.TryGetValue (id, out result) ? result : null;
	}

	public IUnit GetUnitOnTile (Vector2 tilePosition) {
		foreach (IUnit unit in units_.Values) {
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
		int currentIndex = 0;
		foreach (KeyValuePair <int, IUnit> pair in units_) {
			if (currentIndex == index) {
				return pair.Value;
			}
			currentIndex++;
		}
		return null;
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
		int count = Random.Range (
			            int.Parse (xml.Attributes ["minCount"].InnerText),
			            int.Parse (xml.Attributes ["maxCount"].InnerText));
		
		Vector2 xMinMax = new Vector2 (
			                  float.Parse (xml.Attributes ["worldRectX0"].InnerText),
			                  float.Parse (xml.Attributes ["worldRectX1"].InnerText));

		Vector2 yMinMax = new Vector2 (
			float.Parse (xml.Attributes ["worldRectY0"].InnerText),
			float.Parse (xml.Attributes ["worldRectY1"].InnerText));

		int minPatrolTargets = 0;
		int maxPatrolTargets = 0;

		if (xml.Attributes ["generatePatrols"] != null && bool.Parse (xml.Attributes ["generatePatrols"].InnerText)) {
			minPatrolTargets = int.Parse (xml.Attributes ["minPatrolTargets"].InnerText);
			maxPatrolTargets = int.Parse (xml.Attributes ["maxPatrolTargets"].InnerText);

			if (minPatrolTargets < 2) {
				minPatrolTargets = 2;
			}
		}

		for (int index = 0; index < count; index++) {
			Vector2 position = Vector2.zero;
			Tile tile = null;

			do {
				position.x = Mathf.Round (Random.Range (xMinMax.x, xMinMax.y));
				position.y = Mathf.Round (Random.Range (yMinMax.x, yMinMax.y));
				tile = map.GetTile (position);
			} while (GetUnitOnTile (position) != null || tile == null || !tile.passable);

			AiUnit unit = SpawnAiUnitFromXml (xml, false);
			unit.position = position;
			unitsObjects_ [unit.id].transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
			unit.UpdateVisionMap (map);

			if (minPatrolTargets != 0 && maxPatrolTargets != 0) {
				int patrolTargetsCount = Random.Range (minPatrolTargets, maxPatrolTargets + 1);
				List <Vector2> patrolTargets = new List <Vector2> ();

				patrolTargets.Add (position);
				patrolTargetsCount--;

				while (patrolTargetsCount > 0) {
					Vector2 patrolTarget = Vector2.zero;
					Tile targetTile = null;

					do {
						patrolTarget.x = Mathf.Round (Random.Range (xMinMax.x, xMinMax.y));
						patrolTarget.y = Mathf.Round (Random.Range (yMinMax.x, yMinMax.y));
						targetTile = map.GetTile (patrolTarget);
					} while (targetTile == null || !targetTile.passable);

					patrolTargets.Add (patrolTarget);
					patrolTargetsCount--;
				}
				unit.patrolTargets = patrolTargets;
			}
		}
	}

	public void UpdateUnitsSpritesByVisionMap () {
		if (visionMapProviderUnit_ != null) {
			foreach (KeyValuePair <int, IUnit> unitPair in units_) {
				IUnit unit = unitPair.Value;
				Vector2 mapCoords = map.RealCoordsToMapCoords (unit.position);
				unitsObjects_ [unitPair.Key].SetActive (visionMapProviderUnit_.visionMap.GetPixel (
						Mathf.RoundToInt (mapCoords.x), Mathf.RoundToInt (mapCoords.y)) == UnitBase.VISIBLE_COLOR);
			}
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

		if (xml.Attributes ["deltaRegeneration"] != null) {
			unit.regeneration += float.Parse (xml.Attributes ["deltaRegeneration"].InnerText);
		}

		if (xml.Attributes ["deltaVisionRange"] != null) {
			int visionRange = (int) unit.visionRange;
			visionRange += int.Parse (xml.Attributes ["deltaVisionRange"].InnerText);

			if (visionRange < 1) {
				visionRange = 1;
			}

			unit.visionRange = (uint) visionRange;
		}
		return unit;
	}

	private void SetVisionMapProviderUnit (IUnit unit) {
		map.GetComponent <MeshRenderer> ().material.SetFloat ("_MapWidth", map.width);
		map.GetComponent <MeshRenderer> ().material.SetFloat ("_MapHeight", map.height);
		map.GetComponent <MeshRenderer> ().material.SetTexture ("_FogOfWar", unit.visionMap); 
		visionMapProviderUnit_ = unit;;
	}
}
