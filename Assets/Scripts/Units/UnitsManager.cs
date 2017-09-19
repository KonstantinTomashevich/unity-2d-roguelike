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

	private bool isProcessingTurn_;
	private List <IAction> immediateActionsQueue_;
	private float immediateActionsElapsedTime_;

	private int currentProcessingUnitIndex_;
	private IAction currentProcessingAction_;
	private float currentProcessingElapsedTime_;
	private IUnit visionMapProviderUnit_;

	public UnitsManager () {
		unitsTypesData_ = new Dictionary <string, UnitTypeData> ();
		visionMapProviderUnit_ = null;
	}

	~UnitsManager () {
	}

	void Start () {
		units_ = new Dictionary <int, IUnit> ();
		unitsSprites_ = new Dictionary <int, GameObject> ();

		isProcessingTurn_ = false;
		immediateActionsQueue_ = new List <IAction> ();
		immediateActionsElapsedTime_ = 0.0f;

		currentProcessingAction_ = null;
		currentProcessingUnitIndex_ = -1;
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
			unitsSprites_ [unit.id].transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
			unit.UpdateVisionMap (map);
		}
	}

	public void UpdateUnitsSpritesByVisionMap () {
		if (visionMapProviderUnit_ != null) {
			foreach (KeyValuePair <int, IUnit> unitPair in units_) {
				IUnit unit = unitPair.Value;
				Vector2 mapCoords = map.RealCoordsToMapCoords (unit.position);
				unitsSprites_ [unitPair.Key].GetComponent <SpriteRenderer> ().enabled =
					visionMapProviderUnit_.visionMap.GetPixel (
					    Mathf.RoundToInt (mapCoords.x), Mathf.RoundToInt (mapCoords.y)) == UnitBase.VISIBLE_COLOR;
			}
		}
	}

	void LoadUnitsTypes (XmlNode rootNode) {
		string spritesPathPrefix = rootNode.Attributes ["spritesPrefix"].InnerText;
		foreach (XmlNode node in rootNode.ChildNodes) {
			unitsTypesData_ [node.LocalName] = new UnitTypeData (node, spritesPathPrefix);
		}
	}

	void NextTurnRequest () {
		if (!isProcessingTurn_) {
			currentProcessingUnitIndex_ = 0;
			currentProcessingAction_ = null;
			currentProcessingElapsedTime_ = 0.0f;

			isProcessingTurn_ = true;
			immediateActionsQueue_.Clear ();
			immediateActionsElapsedTime_ = 0.0f;
			ProcessNextUnitTurn ();
		}
	}

	void AllAnimationsFinished () {
		if (isProcessingTurn_) {
			ProcessCurrentActionAndStartNext ();
		} else {
			ProcessImmediateAction ();
		}
		UpdateUnitsSpritesByVisionMap ();
	}

	void ImmediateActionRequest (IAction action) {
		if (!isProcessingTurn_ && immediateActionsElapsedTime_ <= 1.0f) {
			immediateActionsQueue_.Add (action);
			if (immediateActionsQueue_.Count == 1) {
				SetupNextImmediateAction ();
			}
		}
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
			currentProcessingElapsedTime_ = 0.0f;
			unit.TurnBegins ();
			SetupNextAction ();

		} else {
			isProcessingTurn_ = false;
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
		currentProcessingAction_.Commit (map, this, itemsManager);
		IUnit unit = GetUnitByIndex (currentProcessingUnitIndex_);
		if (unit.health <= 0.0f) {
			RemoveUnit (unit.id);
			ProcessNextUnitTurn ();
		} else {
			SetupNextAction ();
		}
	}

	private void SetupNextAction () {
		IUnit unit = GetUnitByIndex (currentProcessingUnitIndex_);
		currentProcessingAction_ = null;
		do {
			currentProcessingAction_ = unit.NextAction (map, this, itemsManager);
		} while (currentProcessingAction_ != null && !currentProcessingAction_.IsValid (map, this, itemsManager));

		if (currentProcessingAction_ != null) {
			currentProcessingElapsedTime_ += currentProcessingAction_.time;
		}

		if (currentProcessingAction_ == null || currentProcessingElapsedTime_ > 1.0f) {
			currentProcessingUnitIndex_++;
			ProcessNextUnitTurn ();
		} else {
			currentProcessingAction_.SetupAnimations (tag, map, this, itemsManager);
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

	private void ProcessImmediateAction () {
		IAction action = immediateActionsQueue_ [0];
		action.Commit (map, this, itemsManager);
		if (action is IUnitAction) {
			IUnit unit = (action as IUnitAction).unit;
			unitsSprites_ [unit.id].transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
		}

		immediateActionsQueue_.RemoveAt (0);
		MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionFinished", action);
		SetupNextImmediateAction ();
	}

	private void SetupNextImmediateAction () {
		if (immediateActionsQueue_.Count > 0) {
			IAction action = immediateActionsQueue_ [0];
			immediateActionsElapsedTime_ += action.time;

			if (immediateActionsElapsedTime_ > 1.0f) {
				immediateActionsQueue_.Clear ();
				MessageUtils.SendMessageToObjectsWithTag (tag, "AllImmediateActionsFinished", null);
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionsMaxTimeReached", null);

			} else {
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionStart", action);
				action.SetupAnimations (tag, map, this, itemsManager);
			}
		} else {
			MessageUtils.SendMessageToObjectsWithTag (tag, "AllImmediateActionsFinished", null);
			if (immediateActionsElapsedTime_ >= 1.0f) {
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionsMaxTimeReached", null);
			}
		}
	}

	private void SetVisionMapProviderUnit (IUnit unit) {
		map.GetComponent <MeshRenderer> ().material.SetFloat ("_MapWidth", map.width);
		map.GetComponent <MeshRenderer> ().material.SetFloat ("_MapHeight", map.height);
		map.GetComponent <MeshRenderer> ().material.SetTexture ("_FogOfWar", unit.visionMap); 
		visionMapProviderUnit_ = unit;;
	}
}
