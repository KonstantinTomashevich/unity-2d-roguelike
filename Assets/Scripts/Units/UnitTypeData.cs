using UnityEngine;
using System;

public enum UnitType {
	PLAYER = 0,
	ORC
}

[System.Serializable]
public struct UnitTypeData {
	public UnitType unitType;
	public Sprite sprite;
	public static UnitTypeData EMPTY;
}

