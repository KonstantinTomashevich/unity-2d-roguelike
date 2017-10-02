using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public interface IItemTypeData {
	Sprite sprite { get; }
	bool passable { get; }
	bool destroyable { get; }
	bool pickable { get; }

	IItem InitItem (XmlNode xml);
}
