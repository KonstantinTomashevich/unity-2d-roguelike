using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageUtils {
	public static void SendMessageToObjectsWithTag (string tag, string message, object argument) {
		GameObject[] objects = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject gameObject in objects) {
			gameObject.SendMessage (message, argument, SendMessageOptions.DontRequireReceiver);
		}
	}
}
