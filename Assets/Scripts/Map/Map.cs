using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Map : MonoBehaviour {
	public UnitsManager unitsManager;
	public ItemsManager itemsManager;

	public string mapName;
	public Color stoneWallColor;
	public Color stoneFloorColor;
	public Color woodWallColor;
	public Color woodFloorColor;

	private Tile[][] tiles_;
	private Texture2D tilesImage_;
	private XmlDocument mapXml_;
	private bool isFirstUpdate_;

	public Map () {
		
	}

	~Map () {
		
	}

	void Start () {
		if (!LoadMap ()) {
			return;
		}

		LoadTiles ();
		GenerateMesh ();
		SendLoadUnitsTypes ();
		isFirstUpdate_ = true;
	}

	void Update () {
		if (isFirstUpdate_) {
			Init ();
			isFirstUpdate_ = false;
		}
	}

	public List <Vector2> FindPath (Vector2 startPosition, Vector2 endPosition, bool findPathToAttack = false) {
		if (startPosition == endPosition) {
			List <Vector2> path = new List <Vector2> ();
			path.Add (startPosition);
			return path;
		}

		SortedList <uint, Vector2> frontier = new SortedList <uint, Vector2> ();
		Dictionary <Vector2, Vector2> cameFrom = new Dictionary <Vector2, Vector2> ();
		Dictionary <Vector2, uint> costSoFar = new Dictionary <Vector2, uint> ();

		frontier [0] = startPosition;
		cameFrom [startPosition] = startPosition;
		costSoFar [startPosition] = 0;

		while (frontier.Count > 0) {
			Vector2 next = frontier.Values [0];
			uint costToThisTile = costSoFar [next];
			frontier.RemoveAt (0);

			if (next.Equals (endPosition)) {

				List <Vector2> path = new List <Vector2> ();
				Vector2 scanPosition = next;
				path.Insert (0, scanPosition);

				do {
					scanPosition = cameFrom [scanPosition];
					path.Insert (0, scanPosition);
				} while (scanPosition != startPosition);

				if (findPathToAttack) {
					path.Remove (endPosition);
				}
				return path;

			} else {
				Vector2[] neighbors = { next + Vector2.up, next + Vector2.down,
					next + Vector2.right, next + Vector2.left};

				foreach  (Vector2 neighbor in neighbors ) {
					Tile tile = GetTile (neighbor);
					if (tile != null && (tile.passable || (findPathToAttack && neighbor.Equals (endPosition))) &&
						(unitsManager.GetUnitOnTile (neighbor) == null || (findPathToAttack && neighbor.Equals (endPosition))) &&
					    (!costSoFar.ContainsKey (neighbor) || costSoFar [neighbor] > costToThisTile + 1)) {

						uint heuristicDistance = (uint) Mathf.RoundToInt (1000.0f * HeuristicDistance (neighbor, endPosition));
						while (frontier.ContainsKey (heuristicDistance + (costToThisTile + 1) * 1000)) {
							heuristicDistance++;
						}

						frontier [heuristicDistance + (costToThisTile + 1) * 1000] = neighbor;
						costSoFar [neighbor] = costToThisTile + 1;
						cameFrom [neighbor] = next;
					}
				}
			}
		}

		// Path not found.
		return new List <Vector2> ();
	}

	public Tile GetTile (int x, int y) {
		if (x < 0 || y < 0 || x >= tiles_.Length || y >= tiles_ [0].Length) {
			return null;
		} else {
			return tiles_ [x] [y];
		}
	}

	public int width {
		get {
			return tiles_.Length;
		}
	}

	public int height {
		get {
			return tiles_ [0].Length;
		}
	}

	public Tile GetTile (Vector2 position, bool mapCoords = false) {
		if (!mapCoords) {
			position = RealCoordsToMapCoords (position);
		}
		return GetTile (Mathf.RoundToInt (position.x), Mathf.RoundToInt (position.y));
	}

	public Vector2 RealCoordsToMapCoords (Vector2 position) {
		return new Vector2 (position.x + tiles_.Length / 2, position.y + tiles_ [0].Length / 2);
	}

	public Vector2 MapCoordsToRealCoords (Vector2 position) {
		return new Vector2 (position.x - tiles_.Length / 2, position.y - tiles_ [0].Length / 2);
	}

	public Vector2 GetWorldTransformFromXml (XmlNode xml) {
		Vector2 position = Vector2.zero;
		if (xml.Attributes ["positionX"] != null && xml.Attributes ["positionY"] != null) {
			position.x = int.Parse (xml.Attributes ["positionX"].InnerText);
			position.y = int.Parse (xml.Attributes ["positionY"].InnerText);
		}

		if (xml.Attributes ["invertX"] != null && bool.Parse (xml.Attributes ["invertX"].InnerText)) {
			position.x = tiles_.Length - position.x;
		}

		if (xml.Attributes ["invertY"] != null && bool.Parse (xml.Attributes ["invertY"].InnerText)) {
			position.y = tiles_ [0].Length - position.y;
		}

		if (xml.Attributes ["mapCoords"] != null && bool.Parse (xml.Attributes ["mapCoords"].InnerText)) {
			position = MapCoordsToRealCoords (position);
		}
		return position;
	}

	private bool LoadMap () {
		mapXml_ = new XmlDocument ();
		mapXml_.LoadXml ((Resources.Load ("Maps/" + mapName + "/Map") as TextAsset).text);
		XmlNode root = mapXml_.DocumentElement;

		XmlNode tilesInfoNode = root ["tiles"];
		if (tilesInfoNode.Attributes ["type"].InnerText != "image") {
			Debug.LogError ("At the moment, only tiles loading from image is supported!");
			return false;
		}

		string imagePath = "Maps/" + mapName + "/" + tilesInfoNode.Attributes ["file"].InnerText;
		tilesImage_ = Resources.Load (imagePath) as Texture2D;
		Debug.Assert (tilesImage_ != null);
		return true;
	}

	private void LoadTiles () {
		tiles_ = new Tile[tilesImage_.width][];
		for (int x = 0; x < tiles_.Length; x++) {
			tiles_ [x] = new Tile[tilesImage_.height];

			for (int y = 0; y < tiles_ [x].Length; y++) {
				Color color = tilesImage_.GetPixel (x, y);
				Tile tile = new Tile ();

				if (color == stoneWallColor) {
					tile.textureIndex = 2;
					tile.passable = false;
					tile.destructable = false;
					tile.watchable = false;

				} else if (color == stoneFloorColor) {
					tile.textureIndex = 3;
					tile.passable = true;
					tile.destructable = false;
					tile.watchable = true;

				} else if (color == woodWallColor) {
					tile.textureIndex = 0;
					tile.passable = false;
					tile.destructable = true;
					tile.watchable = false;

				} else if (color == woodFloorColor) {
					tile.textureIndex = 1;
					tile.passable = true;
					tile.destructable = false;
					tile.watchable = true;

				} else {
					Debug.LogError ("Unknown tile (" + x + "; " + y + ") with color: " + color.ToString ());
				}
					
				tiles_ [x] [y] = tile;
			}
		}
	}

	private void GenerateMesh () {
		Mesh mesh = new Mesh ();
		Vector2 meshOffset = new Vector2 (-tilesImage_.width / 2, -tilesImage_.height / 2);

		int tilesCount = tilesImage_.width * tilesImage_.height;
		Vector3[] vertices = new Vector3[4 * tilesCount];
		int[] triangles = new int[6 * tilesCount];
		Vector2[] uv = new Vector2[4 * tilesCount];

		for (int x = 0; x < tiles_.Length; x++) {
			for (int y = 0; y < tiles_ [x].Length; y++) {
				int tileIndex = x * tilesImage_.width + y;

				vertices [tileIndex * 4 + 0] = new Vector3 (-0.5f + x + meshOffset.x, -0.5f + y + meshOffset.y, 0.0f);
				vertices [tileIndex * 4 + 1] = new Vector3 (0.5f + x + meshOffset.x, -0.5f + y + meshOffset.y, 0.0f);
				vertices [tileIndex * 4 + 2] = new Vector3 (-0.5f + x + meshOffset.x, 0.5f + y + meshOffset.y, 0.0f);
				vertices [tileIndex * 4 + 3] = new Vector3 (0.5f + x + meshOffset.x, 0.5f + y + meshOffset.y, 0.0f);

				triangles [tileIndex * 6 + 0] = tileIndex * 4 + 1;
				triangles [tileIndex * 6 + 1] = tileIndex * 4 + 0;
				triangles [tileIndex * 6 + 2] = tileIndex * 4 + 2;
				triangles [tileIndex * 6 + 3] = tileIndex * 4 + 1;
				triangles [tileIndex * 6 + 4] = tileIndex * 4 + 2;
				triangles [tileIndex * 6 + 5] = tileIndex * 4 + 3;

				Vector2 tileTextureCoord = GetTileTextureCoord (tiles_ [x] [y].textureIndex);
				uv [tileIndex * 4 + 0] = new Vector2 (tileTextureCoord.x + 0.005f, tileTextureCoord.y + 0.005f);
				uv [tileIndex * 4 + 1] = new Vector2 (tileTextureCoord.x + 0.24f, tileTextureCoord.y + 0.005f);
				uv [tileIndex * 4 + 2] = new Vector2 (tileTextureCoord.x + 0.005f, tileTextureCoord.y + 0.24f);
				uv [tileIndex * 4 + 3] = new Vector2 (tileTextureCoord.x + 0.24f, tileTextureCoord.y + 0.24f);
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		GetComponent <MeshFilter> ().mesh = mesh;
	}

	private void SendLoadUnitsTypes () {
		XmlNode unitsTypesNode = mapXml_.DocumentElement ["unitsTypes"];
		Debug.Assert (unitsTypesNode != null);
		string type = unitsTypesNode.Attributes ["type"].InnerText;

		XmlNode nodeToLoad = null;
		if (type.Equals ("inner")) {
			nodeToLoad = unitsTypesNode;

		} else if (type.Equals ("file")) {
			string fileName = unitsTypesNode.Attributes ["file"].InnerText;
			XmlDocument unitsTypesDocument = new XmlDocument ();
			unitsTypesDocument.LoadXml ((Resources.Load ("Maps/" + mapName + "/" + fileName) as TextAsset).text);
			nodeToLoad = unitsTypesDocument.DocumentElement;

		} else {
			XmlDocument unitsTypesDocument = new XmlDocument ();
			unitsTypesDocument.LoadXml ((Resources.Load ("DefaultUnits") as TextAsset).text);
			nodeToLoad = unitsTypesDocument.DocumentElement;
		}
		MessageUtils.SendMessageToObjectsWithTag (tag, "LoadUnitsTypes", nodeToLoad);
	}
    
    private Vector2 GetTileTextureCoord (int tileTextureIndex) {
		Vector2 coord;
		int textureX = tileTextureIndex / 4;
		int textureY = tileTextureIndex % 4;
		coord.x = textureX * 0.25f;
		coord.y = textureY * 0.25f;
		return coord;
	}

	private void Init  () {
		MessageUtils.SendMessageToObjectsWithTag (tag, "MapSize", new Vector2 (tiles_.Length, tiles_ [0].Length));
		LoadUnits ();
	}

	private void LoadUnits () {
		foreach (XmlNode xml in mapXml_.DocumentElement ["units"]) {
			if (xml.LocalName == "player") {
				unitsManager.SpawnPlayerFromXml (xml);
			} else if (xml.LocalName == "unit") {
				unitsManager.SpawnAiUnitFromXml (xml);
			} else if (xml.LocalName == "spawner") {
				unitsManager.ProcessXmlSpawner (xml);
			}
		}
		unitsManager.UpdateUnitsSpritesByVisionMap ();
	}

	static public float HeuristicDistance (Vector2 first, Vector2 second) {
		return Mathf.Sqrt (
			(first.x - second.x) * (first.x - second.x) +
			(first.y - second.y) * (first.y - second.y));
	}
}
