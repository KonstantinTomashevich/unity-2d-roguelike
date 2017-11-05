using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour {
	public GUISkin skin;
	public Map map;
	public UnitsManager unitsManager;
	public ItemsManager itemsManager;

	private Vector2 lastScrollPosition_;
	private IUnit playerUnit_;
	private IItem selectedItem_;

	void Start () {
		lastScrollPosition_ = Vector2.zero;
		playerUnit_ = null;
		selectedItem_ = null;
	}

	void Update () {
	}

	void OnGUI () {
		GUI.skin = skin;

		int W = Screen.width;
		int H = Screen.height;

		skin.label.fontSize = H / 30;
		skin.button.fontSize = H / 23;
		skin.window.fontSize = H / 23;
		skin.GetStyle ("title").fontSize = H / 15;

		if (playerUnit_ != null) {

			GUILayout.Window (2, new Rect (W - H / 2.5f, 0, H / 2.5f, H), (int id) => {
				ProcessItemsGUI (W, H);
				ProcessSelectedItemGUI (W, H);
			}, "Inventory");
		}
		GUI.skin = null;
	}

	void PlayerUnitCreated (IUnit unit) {
		playerUnit_ = unit;
	}

	void UnitDie (IUnit unit) {
		if (playerUnit_ == unit) {
			playerUnit_ = null;
		}
	}

	private void ProcessItemsGUI (int W, int H) {
		lastScrollPosition_ = GUILayout.BeginScrollView (lastScrollPosition_);

		List <IItem> itemsCopy = playerUnit_.GetItemsInInventory ();
		int currentlyPlacedInARow = 0;
		int rowIndex = 0;
		GUILayout.BeginHorizontal ();

		while (itemsCopy.Count > 0) {
			IItem item = itemsCopy [0];
			int itemsOfThisTypeCount = RemoveItemsOfType (itemsCopy, item.itemType);

			if (GUI.Button (new Rect (H / 90.0f + currentlyPlacedInARow * H / 9.5f, rowIndex * H / 9.5f, H / 10.0f, H / 10.0f), "")) {
				selectedItem_ = item;
			}

			GUI.DrawTexture (new Rect (H / 90.0f + currentlyPlacedInARow * H / 9.5f, rowIndex * H / 9.5f, H / 10.0f, H / 10.0f), 
				itemsManager.GetItemObject (item).GetComponent <SpriteRenderer> ().sprite.texture);

			GUILayout.Label ("  " + itemsOfThisTypeCount,
				GUILayout.Width (H / 10.0f), GUILayout.Height (H / 10.0f));

			currentlyPlacedInARow++;
			if (currentlyPlacedInARow >= 3) {

				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();

				currentlyPlacedInARow = 0;
				rowIndex++;
			}
		}

		GUILayout.EndHorizontal ();
		GUILayout.EndScrollView ();
	}

	private void ProcessSelectedItemGUI (int W, int H) {
		if (selectedItem_ != null) {
			GUILayout.BeginHorizontal ();
			GUI.DrawTexture (new Rect (5, H - H / 5.0f, H / 5.0f, H / 5.0f), 
				itemsManager.GetItemObject (selectedItem_).GetComponent <SpriteRenderer> ().sprite.texture);
			
			GUILayout.Space (H / 5.0f);
			GUILayout.BeginVertical ();
			GUILayout.Label ("It's " + selectedItem_.itemType + ".");

			if (GUILayout.Button ("Drop")) {
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionRequest", new DropAction (playerUnit_, selectedItem_));
				selectedItem_ = null;
			}

			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
		}
	}

	private int RemoveItemsOfType (List <IItem> items, string itemType) {
		int count = 0;
		int index = 0;

		while (index < items.Count) {
			if (items [index].itemType == itemType) {
				items.RemoveAt (index);
				count++;

			} else {
				index++;
			}
		}
		return count;
	}
}
