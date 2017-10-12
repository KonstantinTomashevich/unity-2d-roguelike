using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MapLoader : MonoBehaviour {
	public string mapName;
	public Map map;
	public UnitsManager unitsManager;
	public ItemsManager itemsManager;

	private bool initialized_;

	void Start () {
		initialized_ = false;
	}

	void Update () {
		if (!initialized_) {
			Load ();
			initialized_ = true;
		}
	}

	private void Load () {
		XmlDocument mapXml = new XmlDocument ();
		mapXml.LoadXml ((Resources.Load ("Maps/" + mapName + "/Map") as TextAsset).text);

		LoadMap (mapXml);
		LoadUnits (mapXml);
		LoadItems (mapXml);
	}

	private void LoadMap (XmlDocument mapXml) {
		XmlNode tilesInfoNode = mapXml.DocumentElement ["tiles"];
		Debug.Assert (tilesInfoNode.Attributes ["type"].InnerText == "image");

		string tilesImagePath = "Maps/" + mapName + "/" + tilesInfoNode.Attributes ["file"].InnerText;
		Texture2D tilesImage = Resources.Load (tilesImagePath) as Texture2D;
		Debug.Assert (tilesImage != null);

		map.LoadTilesFromImage (tilesImage);
		map.GenerateMesh ();
	}

	private void LoadUnits (XmlDocument mapXml) {
		LoadUnitsTypes (mapXml);
		SpawnUnits (mapXml);
	}

	private void LoadItems (XmlDocument mapXml) {
		LoadItemsTypes (mapXml);
		SpawnItems (mapXml);
	}

	private void LoadUnitsTypes (XmlDocument mapXml) {
		XmlNode unitsTypesNode = mapXml.DocumentElement ["unitsTypes"];
		Debug.Assert (unitsTypesNode != null);
		string type = unitsTypesNode.Attributes ["type"].InnerText;

		XmlNode nodeToLoad = null;
		if (type.Equals ("inner")) {
			nodeToLoad = unitsTypesNode;

		} else if (type.Equals ("file")) {
			string fileName = unitsTypesNode.Attributes ["file"].InnerText;
			XmlDocument unitsTypesDocument = new XmlDocument ();
			unitsTypesDocument.LoadXml ((Resources.Load ("Maps/" + mapName + "/" + fileName) as TextAsset).text);
			nodeToLoad = unitsTypesDocument.DocumentElement;

		} else {
			XmlDocument unitsTypesDocument = new XmlDocument ();
			unitsTypesDocument.LoadXml ((Resources.Load ("DefaultUnits") as TextAsset).text);
			nodeToLoad = unitsTypesDocument.DocumentElement;
		}

		unitsManager.LoadUnitsTypes (nodeToLoad);
	}

	private void SpawnUnits (XmlDocument mapXml) {
		foreach (XmlNode xml in mapXml.DocumentElement ["units"]) {
			if (xml.LocalName == "player") {
				unitsManager.SpawnPlayerFromXml (xml);
			} else if (xml.LocalName == "unit") {
				unitsManager.SpawnAiUnitFromXml (xml);
			} else if (xml.LocalName == "spawner") {
				unitsManager.ProcessXmlSpawner (xml);
			}
		}
		unitsManager.UpdateUnitsSpritesByVisionMap ();
	}

	private void LoadItemsTypes (XmlDocument mapXml) {
		XmlNode itemsTypesNode = mapXml.DocumentElement ["itemsTypes"];
		Debug.Assert (itemsTypesNode != null);
		string type = itemsTypesNode.Attributes ["type"].InnerText;

		XmlNode nodeToLoad = null;
		if (type.Equals ("inner")) {
			nodeToLoad = itemsTypesNode;

		} else if (type.Equals ("file")) {
			string fileName = itemsTypesNode.Attributes ["file"].InnerText;
			XmlDocument itemsTypesDocument = new XmlDocument ();
			itemsTypesDocument.LoadXml ((Resources.Load ("Maps/" + mapName + "/" + fileName) as TextAsset).text);
			nodeToLoad = itemsTypesDocument.DocumentElement;

		} else {
			XmlDocument itemsTypesDocument = new XmlDocument ();
			itemsTypesDocument.LoadXml ((Resources.Load ("DefaultItems") as TextAsset).text);
			nodeToLoad = itemsTypesDocument.DocumentElement;
		}

		itemsManager.LoadItemsTypes (nodeToLoad);
	}

	private void SpawnItems (XmlDocument mapXml) {
		foreach (XmlNode xml in mapXml.DocumentElement ["items"]) {
			if (xml.LocalName == "item") {
				itemsManager.SpawnItemFromXml (xml);
			} else if (xml.LocalName == "spawner") {
				itemsManager.ProcessItemsSpawner (xml);
			}
		}
	}
}
