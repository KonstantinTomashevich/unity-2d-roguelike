using UnityEngine;
using System;
using System.Xml;

public class UnitTypeData {
	private Sprite sprite_;
	private float defaultArmor_;
	private float defaultRegeneration_;
	private Vector2 defaultAttackForce_;
	private float defaultMoveSpeed_;
	private float defaultAttackSpeed_;
	private uint defaultVisionRange_;

	public UnitTypeData (XmlNode xml, string spritesPathPrefix) {
		sprite_ = Resources.Load <Sprite> (spritesPathPrefix + xml.Attributes ["sprite"].InnerText);
		defaultArmor_ = XmlHelper.GetFloatAttribute (xml, "armor");
		defaultRegeneration_ = XmlHelper.GetFloatAttribute (xml, "regeneration");;

		defaultAttackForce_ = XmlHelper.GetVector2Attribute (xml, "minAttack", "maxAttack");
		defaultMoveSpeed_ = XmlHelper.GetFloatAttribute (xml, "moveSpeed");;
		defaultAttackSpeed_ = XmlHelper.GetFloatAttribute (xml, "attackSpeed");;
		defaultVisionRange_ = XmlHelper.GetUIntAttribute (xml, "visionRange");;
	}

	~UnitTypeData () {
	}

	public Sprite sprite {
		get {
			return sprite_;
		}
	}

	public float defaultArmor {
		get {
			return defaultArmor_;
		}
	}

	public float defaultRegeneration {
		get {
			return defaultRegeneration_;
		}
	}

	public Vector2 defaultAttackForce {
		get {
			return defaultAttackForce_;
		}
	}

	public float defaultMoveSpeed {
		get {
			return defaultMoveSpeed_;
		}
	}

	public float defaultAttackSpeed {
		get {
			return defaultAttackSpeed_;
		}
	}

	public uint defaultVisionRange {
		get {
			return defaultVisionRange_;
		}
	}
}
