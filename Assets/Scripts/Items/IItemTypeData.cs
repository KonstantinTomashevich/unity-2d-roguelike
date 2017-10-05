using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public interface IItemTypeData {
	Sprite sprite { get; }
	bool passable { get; }
	bool destructable { get; }
	bool pickable { get; }

	IItem CreateItem (XmlNode xml);
}
