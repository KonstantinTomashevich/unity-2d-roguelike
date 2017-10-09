using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class CargoItemTypeData : ItemTypeDataBase {
	public CargoItemTypeData (XmlNode xml, string spritesPathPrefix) : base (xml, spritesPathPrefix) {
	}

	~CargoItemTypeData () {
	}

	public override IItem CreateItem (Map map, UnitsManager unitsManager, ItemsManager itemsManager, XmlNode xml) {
		CargoItem item = new CargoItem (name);
		InitBasicItemProperties (map, unitsManager, itemsManager, item, xml);
		return item;
	}
}
