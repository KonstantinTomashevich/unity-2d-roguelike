using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitBase : IUnit
{
	public const float STANDART_UNIT_MAX_HEALTH = 100.0f;
	public const float STANDART_ATTACK_SPEED = 1.0f;
	public const float STANDART_MOVE_SPEED = 1.0f;
	public const int STANDART_VISION_RANGE = 3;

	public static Color FOG_OF_WAR_COLOR = new Color (0.3f, 0.3f, 0.3f, 1.0f);
	public static Color VISIBLE_COLOR = new Color (1.0f, 1.0f, 1.0f, 1.0f);

	private int id_;
	private Vector2 position_;
	private float health_;
	private float regeneration_;
	private string unitType_;

	private Vector2 attackForce_;
	private float attackSpeed_;
	private float moveSpeed_;
	private float armor_;

	private uint visionRange_;
	private Texture2D visionMap_;
	private Dictionary <Vector2, uint> lastVisionMapUpdateVisibleTiles_;

	public UnitBase (string unitType, float health = STANDART_UNIT_MAX_HEALTH)
	{
		id_ = 0;
		unitType_ = unitType;
		position_ = Vector2.zero;
		health_ = health;
		regeneration_ = 0.0f;

		attackForce_ = Vector2.zero;
		attackSpeed_ = STANDART_ATTACK_SPEED;
		moveSpeed_ = STANDART_MOVE_SPEED;
		armor_ = 0.0f;

		visionRange_ = STANDART_VISION_RANGE;
		visionMap_ = null;
		lastVisionMapUpdateVisibleTiles_ = new Dictionary <Vector2, uint> ();
	}

	~UnitBase ()
	{
	}


	public void ApplyDamage (float damage)
	{
		float unblockedDamage = damage - armor_;
		if (unblockedDamage > 0.0f) {
			health_ -= unblockedDamage;
		}
	}

	public virtual void TurnBegins () {
		health_ += regeneration_;
		if (health_ > STANDART_UNIT_MAX_HEALTH) {
			health_ = STANDART_UNIT_MAX_HEALTH;
		}
	}

	public abstract IAction NextAction (Map map, UnitsManager unitsManager, ItemsManager itemsManager);
	public void InitVisionMap (int mapWidth, int mapHeight)
	{
		visionMap_ = new Texture2D (mapWidth, mapHeight);
		visionMap_.filterMode = FilterMode.Point;
		ClearVisionMap ();
	}

	public void UpdateVisionMap (Map map)
	{
		ClearVisionMap ();
		lastVisionMapUpdateVisibleTiles_ = GetVisibleTiles (map);
		FillVisibleTiles (lastVisionMapUpdateVisibleTiles_, map);
	}

	private void ClearVisionMap ()
	{
		for (int x = 0; x < visionMap_.width; x++) {
			for (int y = 0; y < visionMap_.height; y++) {
				visionMap.SetPixel (x, y, FOG_OF_WAR_COLOR);
			}
		}
		visionMap_.Apply ();
	}

	private Dictionary <Vector2, uint> GetVisibleTiles (Map map)
	{
		Dictionary <Vector2, uint> visibleTiles = new Dictionary <Vector2, uint> ();
		// First value of pair is tile position, second is distance to tile.
		List <KeyValuePair <Vector2, uint> > tilesToCheck = new List <KeyValuePair <Vector2, uint> > ();
		tilesToCheck.Add (new KeyValuePair <Vector2, uint> (position, 0));

		while (tilesToCheck.Count > 0) {
			KeyValuePair <Vector2, uint> infoPair = tilesToCheck [0];
			Vector2 tilePosition = infoPair.Key;
			uint distance = infoPair.Value;
			Tile tile = map.GetTile (infoPair.Key);

			if (tile != null) {
				if (!tile.watchable) {
					distance = visionRange_ + 1;
				}
				bool checkNeighbors = distance < visionRange_;

				if (visibleTiles.ContainsKey (tilePosition)) {
					uint anotherDistance = visibleTiles [tilePosition];

					if (anotherDistance > distance) {
						visibleTiles [tilePosition] = distance;
						checkNeighbors = anotherDistance >= visionRange_;
					}

				} else {
					visibleTiles.Add (tilePosition, distance);
				}

				if (checkNeighbors) {
					uint newDistance = distance + 1;
					Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

					foreach (Vector2 direction in directions) {
						tilesToCheck.Add (new KeyValuePair <Vector2, uint> (tilePosition + direction, newDistance));
					}
				}
			} else if (!visibleTiles.ContainsKey (tilePosition)) {
				visibleTiles.Add (tilePosition, visionRange_ + 1);
			}

			tilesToCheck.RemoveAt (0);
		}
		return visibleTiles;
	}

	private void FillVisibleTiles (Dictionary <Vector2, uint> visibleTiles, Map map)
	{
		foreach (KeyValuePair <Vector2, uint> infoPair in visibleTiles) {
			Vector2 mapCoords = map.RealCoordsToMapCoords (infoPair.Key);
			visionMap_.SetPixel (Mathf.RoundToInt (mapCoords.x), Mathf.RoundToInt (mapCoords.y), VISIBLE_COLOR);
		}
		visionMap_.Apply ();
	}

	public int id { 
		get { 
			return id_;
		}

		set { 
			id_ = value; 
		} 
	}

	public Vector2 position { 
		get { 
			return position_; 
		}

		set { 
			position_ = value; 
		}
	}

	public float health { 
		get { 
			return health_; 
		}

	}

	public float regeneration { 
		get {
			return regeneration_;
		}

		set {
			regeneration_ = value;
		}
	}

	public string unitType { 
		get {
			return unitType_;
		}

		set {
			unitType_ = value;
		}
	}

	public Vector2 attackForce { 
		get {
			return attackForce_;
		}

		set {
			Debug.Assert (value.x >= 0.0f);
			Debug.Assert (value.y >= 0.0f);
			Debug.Assert (value.y >= value.x);
			attackForce_ = value;
		}
	}

	public float attackSpeed { 
		get { 
			return attackSpeed_; 
		}

		set { 
			Debug.Assert (value >= 1.0f); 
			attackSpeed_ = value; 
		}
	}

	public float moveSpeed { 
		get { 
			return moveSpeed_; 
		}

		set {
			Debug.Assert (value >= 1.0f);
			moveSpeed_ = value;
		}
	}

	public float armor {
		get {
			return armor_;
		}

		set {
			Debug.Assert (value >= 0.0f);
			armor_ = value;
		}
	}

	public uint visionRange {
		get {
			return visionRange_;
		}

		set {
			Debug.Assert (value > 0);
			visionRange_ = value;
		}
	}

	public Texture2D visionMap { 
		get {
			return visionMap_;
		}
	}

	protected Dictionary <Vector2, uint> lastVisionMapUpdateVisibleTiles {
		get {
			return lastVisionMapUpdateVisibleTiles_;
		}
	}
}
