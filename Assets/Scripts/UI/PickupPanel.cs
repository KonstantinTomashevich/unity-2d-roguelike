using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupPanel : MonoBehaviour {
	public GUISkin skin;
	public Map map;
	public UnitsManager unitsManager;
	public ItemsManager itemsManager;

	private IUnit playerUnit_;
	private bool isProcessingTurn_;

	void Start () {
		
	}

	void Update () {
		
	}

	void OnGUI () {
		GUI.skin = skin;

		int W = Screen.width;
		int H = Screen.height;
		float hW = W / 2.0f;

		skin.label.fontSize = H / 30;
		skin.button.fontSize = H / 23;
		skin.GetStyle ("title").fontSize = H / 15;

		if (playerUnit_ != null && !isProcessingTurn_) {
			IItem item = GetFirstPickableItem ();

			if (item != null) {
				GUILayout.Window (1, new Rect (hW - H / 2.0f, H - H / 4.0f, H, H / 4.0f), (int id) => {
					GUILayout.BeginHorizontal ();
					GUI.DrawTexture (new Rect (5, 5, H / 4.0f - 10, H / 4.0f - 10), 
						itemsManager.GetItemObject (item).GetComponent <SpriteRenderer> ().sprite.texture);
					GUILayout.Space (H / 4.0f);

					GUILayout.BeginVertical ();
					GUILayout.Label ("There is a " + item.itemType + ".");
					if (GUILayout.Button ("Pickup")) {
						
						if (PickupAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, item)) {
							MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionRequest", new PickupAction (playerUnit_, item));
						}
					}

					GUILayout.EndVertical ();
					GUILayout.EndHorizontal ();
		
				}, "");
			}
		}
		GUI.skin = null;
	}

	void NextTurnRequest () {
		isProcessingTurn_ = true;
	}

	void TurnFinished () {
		isProcessingTurn_ = false;
	}

	void PlayerUnitCreated (PlayerUnit unit) {
		playerUnit_ = unit;
	}

	private IItem GetFirstPickableItem () {
		List <IItem> itemsOnTile = itemsManager.GetItemsOnTile (playerUnit_.position, false);
		IItem firstPickable = null;

		foreach (IItem item in itemsOnTile) {
			if (item.pickable) {
				firstPickable = item;
				break;
			}
		}

		return firstPickable;
	}
}
