using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitBase : IUnit
{
	public const float STANDART_UNIT_MAX_HEALTH = 100.0f;
	public const float STANDART_ATTACK_SPEED = 1.0f;
	public const float STANDART_MOVE_SPEED = 1.0f;

	private int id_;
	private Vector2 position_;
	private float health_;
	private string unitType_;

	private Vector2 attackForce_;
	private float attackSpeed_;
	private float moveSpeed_;
	private float armor_;

	public UnitBase () {
		id_ = 0;
		position_ = Vector2.zero;
		health_ = STANDART_UNIT_MAX_HEALTH;

		attackForce_ = Vector2.zero;
		attackSpeed_ = STANDART_ATTACK_SPEED;
		moveSpeed_ = STANDART_MOVE_SPEED;
		armor_ = 0.0f;
	}

	~UnitBase () {
	}


	public void ApplyDamage (float damage) {
		float unblockedDamage = damage - armor_;
		if (unblockedDamage > 0.0f) {
			health_ -= unblockedDamage;
		}
	}

	public abstract IAction[] MakeTurn (Map map, UnitsManager unitsManager, ItemsManager itemsManager);

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
}
