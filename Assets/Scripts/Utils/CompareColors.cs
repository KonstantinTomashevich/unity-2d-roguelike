using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareColors {
	public static bool Compare (Color first, Color second, float multiplier = 100.0f) {
		return (Mathf.Floor (first.r * multiplier) == Mathf.Floor (second.r * multiplier) &&
			Mathf.Floor (first.g * multiplier) == Mathf.Floor (second.g * multiplier) &&
			Mathf.Floor (first.b * multiplier) == Mathf.Floor (second.b * multiplier) &&
			Mathf.Floor (first.a * multiplier) == Mathf.Floor (second.a * multiplier));
	}
}