using UnityEngine;
using System;
using System.Xml;

[System.Serializable]
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
		defaultArmor_ = float.Parse (xml.Attributes ["armor"].InnerText);
		defaultRegeneration_ = float.Parse (xml.Attributes ["regeneration"].InnerText);

		defaultAttackForce_ = new Vector2 (
			float.Parse (xml.Attributes ["minAttack"].InnerText),
			float.Parse (xml.Attributes ["maxAttack"].InnerText));

		defaultMoveSpeed_ = float.Parse (xml.Attributes ["moveSpeed"].InnerText);
		defaultAttackSpeed_ = float.Parse (xml.Attributes ["attackSpeed"].InnerText);
		defaultVisionRange_ = uint.Parse (xml.Attributes ["visionRange"].InnerText);
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
