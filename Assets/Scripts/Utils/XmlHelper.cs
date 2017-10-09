using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class XmlHelper {
	public static bool HasAttribute (XmlNode xml, string attributeName) {
		return xml.Attributes [attributeName] != null;
	}

	public static float GetFloatAttribute (XmlNode xml, string attributeName) {
		return float.Parse (xml.Attributes [attributeName].InnerText);
	}

	public static int GetIntAttribute (XmlNode xml, string attributeName) {
		return int.Parse (xml.Attributes [attributeName].InnerText);
	}

	public static uint GetUIntAttribute (XmlNode xml, string attributeName) {
		return uint.Parse (xml.Attributes [attributeName].InnerText);
	}

	public static bool GetBoolAttribute (XmlNode xml, string attributeName) {
		return bool.Parse (xml.Attributes [attributeName].InnerText);
	}

	public static Vector2 GetVector2Attribute (XmlNode xml, string attributeNameX, string attributeNameY) {
		return new Vector2 (GetFloatAttribute (xml, attributeNameX), GetFloatAttribute (xml, attributeNameY));
	}

	public static Rect GetRectAttribute (XmlNode xml, string attributeName) {
		return Rect.MinMaxRect (GetFloatAttribute (xml, attributeName + "X0"), GetFloatAttribute (xml, attributeName + "Y0"),
								GetFloatAttribute (xml, attributeName + "X1"), GetFloatAttribute (xml, attributeName + "Y1"));
	}
}
