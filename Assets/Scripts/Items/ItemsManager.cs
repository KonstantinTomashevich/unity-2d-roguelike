using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ItemsManager : MonoBehaviour {
	public Map map;
	public UnitsManager unitsManager;

	private Dictionary <int, IItem> items_;
	private Dictionary <int, GameObject> itemsObjects_;
	private Dictionary <string, IItemTypeData> itemsTypesData_;

	public ItemsManager () {
		itemsTypesData_ = new Dictionary <string, IItemTypeData> ();
	}

	~ItemsManager () {
	}

	void Start () {
		items_ = new Dictionary <int, IItem> ();
		itemsObjects_ = new Dictionary <int, GameObject> ();
	}

	void Update () {
	}
}
