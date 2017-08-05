using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection {
	UP = 0,
	DOWN,
	LEFT,
	RIGHT
};

public class MoveAction : IAction {
	public IUnit unit;
	public MoveDirection direction;

	public MoveAction (IUnit _unit, MoveDirection _direction) {
		unit = _unit;
		direction = _direction;
	}

	~MoveAction () {
	}
}
