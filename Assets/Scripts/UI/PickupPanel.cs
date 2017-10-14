using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupPanel : MonoBehaviour {
	public GameObject pickupPanelObject;
	public Image itemImage;
	public Text itemText;

	public Map map;
	public UnitsManager unitsManager;
	public ItemsManager itemsManager;

	private IUnit playerUnit_;
	private bool isProcessingTurn_;

	void Start () {
		SetPanelEnabled (false);
	}

	void Update () {
		
	}

	public void OnPickupPressed () {
		IItem firstPickable = GetFirstPickableItem ();
		if (PickupAction.StaticValidation (map, unitsManager, itemsManager, playerUnit_, firstPickable)) {
			MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionRequest", new PickupAction (playerUnit_, firstPickable));
		}
	}

	void NextTurnRequest () {
		isProcessingTurn_ = true;
		SetPanelEnabled (false);
	}

	void TurnFinished () {
		isProcessingTurn_ = false;
		UpdatePanel ();
	}

	void PlayerUnitCreated (PlayerUnit unit) {
		playerUnit_ = unit;
		UpdatePanel ();
	}

	void AllAnimationsFinished () {
		UpdatePanel ();
	}

	private void UpdatePanel () {
		if (playerUnit_ != null && !isProcessingTurn_) {
			IItem firstPickable = GetFirstPickableItem ();

			if (firstPickable != null) {
				SetPanelEnabled (true);
				InitForItem (firstPickable);

			} else {
				SetPanelEnabled (false);
			}
		}
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

	private void InitForItem (IItem item) {
		itemImage.sprite = itemsManager.GetItemObject (item).GetComponent <SpriteRenderer> ().sprite;
		itemText.text = "There is a " + item.itemType + ".";
	}

	private void SetPanelEnabled (bool enabled) {
		pickupPanelObject.SetActive (enabled);
		foreach (Transform child in pickupPanelObject.transform) {
			child.gameObject.SetActive (enabled);
		}
	}
}
