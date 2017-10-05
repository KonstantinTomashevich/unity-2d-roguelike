using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : IItem {
	private int id_;
	private string itemType_;
	private GameObject itemObject_;

	private Vector2 position_;
	private IUnit holder_;

	private bool passable_;
	private bool destructable_;
	private bool pickable_;

	public ItemBase (string itemType) {
		id_ = 0;
		itemType_ = itemType;
		itemObject_ = null;

		position_ = Vector2.zero;
		holder_ = null;

		passable_ = false;
		destructable_ = false;
		pickable_ = false;
	}

	~ItemBase () {
	}

	public abstract void ProcessTurn (Map map, UnitsManager unitsManager, ItemsManager itemsManager);
	public void Destruct (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit destructor) {
		itemsManager.RemoveItem (id_);
	}

	public void Pick (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IUnit pickuper) {
		pickuper.AddToInventory (this);
	}

	public void Throw (Map map, UnitsManager unitsManager, ItemsManager itemsManager, Vector2 direction) {
		holder_ = null;
	}

	public int id { 
		get {
			return id_;
		}

		set {
			id_ = value;
		}
	}

	public string itemType { 
		get {
			return itemType_;
		}
	}

	public GameObject itemObject { 
		set {
			Debug.Assert (value != null);
			itemObject_ = value;
		}
	}

	public Vector2 position {
		get {
			return position_;
		}

		set {
			position_ = value;
		}
	}

	public IUnit holder { 
		get {
			return holder_;
		}
	}

	public bool passable { 
		get {
			return passable_;
		}

		set {
			passable_ = value;
		}
	}

	public bool destructable { 
		get {
			return destructable_;
		}

		set {
			destructable_ = value;
		}
	}

	public bool pickable { 
		get {
			return pickable_;
		}

		set {
			pickable_ = value;
		}
	}
}
