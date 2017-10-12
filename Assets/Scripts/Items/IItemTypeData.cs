using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public interface IItemTypeData {
	string name { get; }
	Sprite sprite { get; }
	float defaultWeight { get; }

	bool passable { get; }
	bool destructable { get; }
	bool pickable { get; }

	IItem CreateItem (Map map, UnitsManager unitsManager, ItemsManager itemsManager, XmlNode xml);
}
