using UnityEngine;
using System;
using System.Xml;

[System.Serializable]
public class UnitTypeData {
	private Sprite sprite_;

	public UnitTypeData (XmlNode xml, string spritesPathPrefix) {
		sprite_ = Resources.Load <Sprite> (spritesPathPrefix + xml.Attributes ["sprite"].InnerText);
	}

	~UnitTypeData () {
	}

	public Sprite sprite {
		get {
			return sprite_;
		}
	}
}

