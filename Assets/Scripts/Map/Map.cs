using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Map : MonoBehaviour {
	public string mapName;
	public Color stoneWallColor;
	public Color stoneFloorColor;
	public Color woodWallColor;
	public Color woodFloorColor;

	private Tile[][] tiles;
	private Texture2D tilesImage;

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
	}

	void Update () {
		
	}

	private bool LoadMap () {
		XmlDocument document = new XmlDocument ();
		document.Load (Application.dataPath + "/Resources/Maps/" + mapName + "/Map.xml");
		XmlNode root = document.DocumentElement;

		XmlNode tilesInfoNode = root ["tiles"];
		if (tilesInfoNode.Attributes ["type"].InnerText != "image") {
			Debug.LogError ("At the moment, only tiles loading from image is supported!");
			return false;
		}

		string imagePath = "Maps/" + mapName + "/" + tilesInfoNode.Attributes ["file"].InnerText;
		tilesImage = Resources.Load (imagePath) as Texture2D;
		Debug.Assert (tilesImage != null);
		return true;
	}

	private void LoadTiles () {
		tiles = new Tile[tilesImage.width][];
		for (int x = 0; x < tilesImage.width; x++) {
			tiles [x] = new Tile[tilesImage.height];

			for (int y = 0; y < tilesImage.height; y++) {
				Color color = tilesImage.GetPixel (x, y);
				Tile tile = new Tile ();

				if (color == stoneWallColor) {
					tile.textureIndex = 2;
					tile.passable = false;
					tile.destructable = false;

				} else if (color == stoneFloorColor) {
					tile.textureIndex = 3;
					tile.passable = true;
					tile.destructable = false;

				} else if (color == woodWallColor) {
					tile.textureIndex = 0;
					tile.passable = false;
					tile.destructable = true;

				} else if (color == woodFloorColor) {
					tile.textureIndex = 1;
					tile.passable = true;
					tile.destructable = false;
				} else {
					Debug.LogError ("Unknown tile (" + x + "; " + y + ") with color: " + color.ToString ());
				}
					
				tiles [x] [y] = tile;
			}
		}
	}

	private void GenerateMesh () {
		Mesh mesh = new Mesh ();
		Vector2 meshOffset = new Vector2 (-tilesImage.width / 2, -tilesImage.height / 2);

		int tilesCount = tilesImage.width * tilesImage.height;
		Vector3[] vertices = new Vector3[4 * tilesCount];
		int[] triangles = new int[6 * tilesCount];
		Vector2[] uv = new Vector2[4 * tilesCount];

		for (int x = 0; x < tilesImage.width; x++) {
			for (int y = 0; y < tilesImage.height; y++) {
				int tileIndex = x * tilesImage.width + y;

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

				Vector2 tileTextureCoord = GetTileTextureCoord (tiles [x] [y].textureIndex);
				uv [tileIndex * 4 + 0] = new Vector2 (tileTextureCoord.x + 0.01f, tileTextureCoord.y + 0.01f);
				uv [tileIndex * 4 + 1] = new Vector2 (tileTextureCoord.x + 0.23f, tileTextureCoord.y + 0.01f);
				uv [tileIndex * 4 + 2] = new Vector2 (tileTextureCoord.x + 0.01f, tileTextureCoord.y + 0.23f);
				uv [tileIndex * 4 + 3] = new Vector2 (tileTextureCoord.x + 0.23f, tileTextureCoord.y + 0.23f);
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		GetComponent <MeshFilter> ().mesh = mesh;
	}
    
    private Vector2 GetTileTextureCoord (int tileTextureIndex) {
		Vector2 coord;
		int textureX = tileTextureIndex / 4;
		int textureY = tileTextureIndex % 4;
		coord.x = textureX * 0.25f;
		coord.y = textureY * 0.25f;
		return coord;
	}
}
