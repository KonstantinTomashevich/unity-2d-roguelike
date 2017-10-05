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
}
