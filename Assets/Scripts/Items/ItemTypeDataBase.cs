using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public abstract class ItemTypeDataBase : IItemTypeData {
	private string name_;
	private Sprite sprite_;
	private float defaultWeight_;

	private bool passable_;
	private bool destructable_;
	private bool pickable_;

	public ItemTypeDataBase (XmlNode xml, string spritesPathPrefix) {
		name_ = xml.LocalName;
		sprite_ = Resources.Load <Sprite> (spritesPathPrefix + xml.Attributes ["sprite"].InnerText);
		defaultWeight_ = XmlHelper.GetFloatAttribute (xml, "weight");

		passable_ = XmlHelper.GetBoolAttribute (xml, "passable");
		destructable_ = XmlHelper.GetBoolAttribute (xml, "destructable");
		pickable_ = XmlHelper.GetBoolAttribute (xml, "pickable");
	}

	~ItemTypeDataBase () {
	}

	public abstract IItem CreateItem (Map map, UnitsManager unitsManager, ItemsManager itemsManager, XmlNode xml);
	protected void InitBasicItemProperties (Map map, UnitsManager unitsManager, ItemsManager itemsManager, IItem item, XmlNode xml) {
		
		item.weight = defaultWeight_;
		if (XmlHelper.HasAttribute (xml, "deltaWeight")) {
			item.weight += XmlHelper.GetFloatAttribute (xml, "deltaWeight");
		}

		item.passable = passable_;
		item.destructable = destructable_;
		item.pickable = pickable_;

		item.position = map.GetWorldTransformFromXml (xml);
		if (XmlHelper.HasAttribute (xml, "held") && XmlHelper.GetBoolAttribute (xml, "held")) {
			IUnit unit = unitsManager.GetUnitOnTile (item.position);

			if (unit != null) {
				item.Pick (map, unitsManager, itemsManager, unit);
			}
		}
	}

	public string name {
		get {
			return name_;
		}
	}

	public Sprite sprite { 
		get {
			return sprite_;
		}
	}

	public float defaultWeight {
		get {
			return defaultWeight_;
		}
	}

	public bool passable { 
		get {
			return passable_;
		}
	}

	public bool destructable { 
		get {
			return destructable_;
		}
	}

	public bool pickable { 
		get {
			return pickable_;
		}
	}
}
