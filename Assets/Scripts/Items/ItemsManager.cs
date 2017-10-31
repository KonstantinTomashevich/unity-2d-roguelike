using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ItemsManager : MonoBehaviour {
	public Map map;
	public UnitsManager unitsManager;

	private List <IItem> items_;
	private Dictionary <int, GameObject> itemsObjects_;
	private Dictionary <string, IItemTypeData> itemsTypesData_;

	public ItemsManager () {
		itemsTypesData_ = new Dictionary <string, IItemTypeData> ();
	}

	~ItemsManager () {
	}

	void Start () {
		items_ = new List <IItem> ();
		itemsObjects_ = new Dictionary <int, GameObject> ();
	}

	void Update () {
	}

	public void AddItem (IItem item) {
		int id = items_.Count + 1;
		if (id > 1) {
			while (items_ [items_.Count - 1].id == id) {
				id++;
			}
		}

		items_.Add (item);
		item.id = id;
		GameObject spriteObject = new GameObject ("item" + id);
		item.itemObject = spriteObject;

		spriteObject.transform.SetParent (transform);
		spriteObject.transform.position = new Vector3 (item.position.x, item.position.y, 0.0f);
		itemsObjects_.Add (id, spriteObject);

		IItemTypeData itemTypeData = itemsTypesData_ [item.itemType];
		Debug.Assert (itemTypeData != null);

		item.passable = itemTypeData.passable;
		item.destructable = itemTypeData.destructable;
		item.pickable = itemTypeData.pickable;

		SpriteRenderer spriteRenderer = spriteObject.AddComponent <SpriteRenderer> ();
		spriteRenderer.sprite = itemTypeData.sprite;
		spriteRenderer.sortingOrder = -1;
		spriteRenderer.drawMode = SpriteDrawMode.Sliced;
		spriteRenderer.size = Vector2.one;
	}

	public int RemoveItem (int id) {
		int index = IndexOfItem (id);
		if (index != -1) {
			items_.RemoveAt (index);

			GameObject itemObject = itemsObjects_ [id];
			itemsObjects_.Remove (id);
			Destroy (itemObject);
		}

		return index;
	}

	public IItem GetItemById (int id) {
		return GetItemByIndex (IndexOfItem (id));
	}

	public int GetItemsCount () {
		return items_.Count;
	}

	public IItem GetItemByIndex (int index) {
		return (index > -1 && index < items_.Count) ? items_ [index] : null;
	}

	public GameObject GetItemObject (IItem item) {
		return GetItemObjectById (item.id);
	}

	public GameObject GetItemObjectById (int id) {
		GameObject result;
		return itemsObjects_.TryGetValue (id, out result) ? result : null;
	}

	public List <IItem> GetItemsOnTile (Vector2 tilePosition, bool includeHeld = true) {
		List <IItem> itemsOnTile = new List <IItem> ();

		foreach (IItem item in items_) {
			if (item.position.Equals (tilePosition) && (includeHeld || item.holder == null)) {
				itemsOnTile.Add (item);
			}
		}

		return itemsOnTile;
	}

	public bool IsTilePassable (Vector2 tilePosition) {
		foreach (IItem item in items_) {
			if (item.position.Equals (tilePosition) && item.holder == null && !item.passable) {
				return false;
			}
		}
		return true;
	}

	public void LoadItemsTypes (XmlNode rootNode) {
		string spritesPathPrefix = rootNode.Attributes ["spritesPrefix"].InnerText;
		foreach (XmlNode node in rootNode.ChildNodes) {
			string itemTypeClass = node.Attributes ["class"].InnerText;

			if (itemTypeClass == "cargo") {
				itemsTypesData_ [node.LocalName] = new CargoItemTypeData (node, spritesPathPrefix);
			} else {
				Debug.LogError ("Unknown item type class: " + itemTypeClass);
			}
		}
	}

	public IItem SpawnItemFromXml (XmlNode xml, bool addItem = true) {
		IItem item = null;
		string itemType = xml.Attributes ["type"].InnerText;

		if (itemsTypesData_.ContainsKey (itemType)) {
			item = itemsTypesData_ [itemType].CreateItem (map, unitsManager, this, xml);
		} else {
			Debug.LogError ("Unknown item type: " + itemType);
		}

		if (item != null && addItem) {
			AddItem (item);
		}

		return item;
	}

	public void ProcessItemsSpawner (XmlNode xml) {
		int count = Random.Range (XmlHelper.GetIntAttribute (xml, "minCount"), XmlHelper.GetIntAttribute (xml, "maxCount"));
		Rect spawnRect = XmlHelper.GetRectAttribute (xml, "worldRect");

		for (int index = 0; index < count; index++) {
			Vector2 spawnPosition = GetValidSpawnPosition (spawnRect);
			IItem item = SpawnItemFromXml (xml, false);

			if (item != null) {
				item.position = spawnPosition;
				AddItem (item);
			}
		}
	}

	public void UpdateItemsSpritesByVisionMap (IUnit visionMapProvider) {
		if (visionMapProvider != null) {
			foreach (IItem item in items_) {
				Vector2 mapCoords = map.RealCoordsToMapCoords (item.position);
				itemsObjects_ [item.id].SetActive (item.holder == null && 
					visionMapProvider.visionMap.GetPixel (Mathf.RoundToInt (mapCoords.x), Mathf.RoundToInt (mapCoords.y)) == UnitBase.VISIBLE_COLOR);
			}
		}
	}

	private int IndexOfItem (int id) {
		int currentIndex = 0;
		foreach (IItem item in items_) {
			
			if (item.id == id) {
				return currentIndex;
			}
			currentIndex++;
		}
		return -1;
	}

	private Vector2 GetValidSpawnPosition (Rect positionRect) {
		Vector2 position = Vector2.zero;
		Tile tile = null;

		do {
			position.x = Mathf.Round (Random.Range (positionRect.xMin, positionRect.xMax));
			position.y = Mathf.Round (Random.Range (positionRect.yMin, positionRect.yMax));
			tile = map.GetTile (position);

		} while (tile == null || !tile.passable);

		return position;
	}
}
