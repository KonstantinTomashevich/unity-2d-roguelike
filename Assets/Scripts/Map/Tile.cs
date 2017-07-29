using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {
	public int textureIndex;
	public bool passable;
	public bool destructable;

	public Tile () {
	}

	~Tile () {
	}

	public string ToPrettyString () {
		return "Tile {texture: " + textureIndex + "; " + (passable ? "passable; " : "impassable; ") +
			(destructable ? "destructable}" : "indestructable}");
	}
}