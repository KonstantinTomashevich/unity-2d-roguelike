using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimation {
	void Init (GameObject hostObject);
	void Process ();
	bool isFinished { get; }
}
