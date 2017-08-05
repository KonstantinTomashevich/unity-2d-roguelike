using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeleeAttackDirection {
	UP = 0,
	DOWN,
	LEFT,
	RIGHT
};

public class MeleeAttackAction : IAction {
	public IUnit unit;
	public MeleeAttackDirection direction;

	public MeleeAttackAction (IUnit _unit, MeleeAttackDirection _direction) {
		unit = _unit;
		direction = _direction;
	}

	~MeleeAttackAction () {
	}
}
