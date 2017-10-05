using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public abstract class ItemTypeDataBase : IItemTypeData {
	private Sprite sprite_;
	private bool passable_;
	private bool destructable_;
	private bool pickable_;

	public ItemTypeDataBase (XmlNode xml, string spritesPathPrefix) {
		sprite_ = Resources.Load <Sprite> (spritesPathPrefix + xml.Attributes ["sprite"].InnerText);
		passable_ = bool.Parse (xml.Attributes ["passable"].InnerText);
		destructable_ = bool.Parse (xml.Attributes ["destructable"].InnerText);
		pickable_ = bool.Parse (xml.Attributes ["pickable"].InnerText);
	}

	~ItemTypeDataBase () {
	}

	public abstract IItem InitItem (XmlNode xml);
	public Sprite sprite { 
		get {
			return sprite_;
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
