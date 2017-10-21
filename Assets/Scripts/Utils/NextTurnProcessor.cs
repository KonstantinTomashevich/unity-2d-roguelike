using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextTurnProcessor : MonoBehaviour {
	public Map map;
	public UnitsManager unitsManager;
	public ItemsManager itemsManager;

	private bool isProcessingTurn_;
	private List <IAction> immediateActionsQueue_;
	private float immediateActionsElapsedTime_;

	private int currentProcessingUnitIndex_;
	private IAction currentProcessingAction_;
	private float currentProcessingElapsedTime_;
	private bool shouldStartNextTurnProcessing_;

	void Start () {
		isProcessingTurn_ = false;
		immediateActionsQueue_ = new List <IAction> ();
		immediateActionsElapsedTime_ = 0.0f;

		currentProcessingAction_ = null;
		currentProcessingUnitIndex_ = -1;
		currentProcessingElapsedTime_ = 0.0f;
		shouldStartNextTurnProcessing_ = false;
	}

	void Update () {
		if (shouldStartNextTurnProcessing_) {
			StartNextTurnProcessing ();
			shouldStartNextTurnProcessing_ = false;
		}
	}

	void StartNextTurnProcessing () {
		currentProcessingUnitIndex_ = 0;
		currentProcessingAction_ = null;
		currentProcessingElapsedTime_ = 0.0f;

		isProcessingTurn_ = true;
		immediateActionsQueue_.Clear ();
		immediateActionsElapsedTime_ = 0.0f;
		ProcessNextUnitTurn ();
	}

	void NextTurnRequest () {
		if (!isProcessingTurn_) {
			shouldStartNextTurnProcessing_ = true;
		}
	}

	void AllAnimationsFinished () {
		if (isProcessingTurn_) {
			ProcessCurrentActionAndStartNext ();
		} else {
			ProcessImmediateAction ();
		}
		unitsManager.UpdateUnitsSpritesByVisionMap ();
	}

	void ImmediateActionRequest (IAction action) {
		if (!isProcessingTurn_ && immediateActionsElapsedTime_ <= 1.0f) {
			immediateActionsQueue_.Add (action);
			if (immediateActionsQueue_.Count == 1) {
				SetupNextImmediateAction ();
			}
		}
	}

	private void ProcessNextUnitTurn () {
		CorrectPreviousUnitSpritePosition ();
		IUnit unit = unitsManager.GetUnitByIndex (currentProcessingUnitIndex_);

		if (unit != null) {
			currentProcessingElapsedTime_ = 0.0f;
			unit.TurnBegins ();
			SetupNextAction ();

		} else {
			UpdateItems ();
			isProcessingTurn_ = false;
			MessageUtils.SendMessageToObjectsWithTag (tag, "TurnFinished", null);
		}
	}

	private void ProcessCurrentActionAndStartNext () {
		currentProcessingAction_.Commit (map, unitsManager, itemsManager);
		IUnit unit = unitsManager.GetUnitByIndex (currentProcessingUnitIndex_);
		if (unit != null) {
			if (unit.health <= 0.0f) {

				if (unitsManager.RemoveUnit (unit.id) <= currentProcessingUnitIndex_) {
					currentProcessingUnitIndex_--;
				}
				ProcessNextUnitTurn ();

			} else {
				SetupNextAction ();
			}
		}
	}

	private void SetupNextAction () {
		IUnit unit = unitsManager.GetUnitByIndex (currentProcessingUnitIndex_);
		currentProcessingAction_ = null;

		do {
			currentProcessingAction_ = unit.NextAction (map, unitsManager, itemsManager);
		} while (currentProcessingAction_ != null && !currentProcessingAction_.IsValid (map, unitsManager, itemsManager));

		if (currentProcessingAction_ != null) {
			currentProcessingElapsedTime_ += currentProcessingAction_.time;
		}

		if (currentProcessingAction_ == null || currentProcessingElapsedTime_ > 1.0f) {
			currentProcessingUnitIndex_++;
			ProcessNextUnitTurn ();
		} else {
			currentProcessingAction_.SetupAnimations (tag, map, unitsManager, itemsManager);
		}
	}

	private void CorrectPreviousUnitSpritePosition () {
		if (currentProcessingUnitIndex_ > 0) {
			IUnit unit = unitsManager.GetUnitByIndex (currentProcessingUnitIndex_ - 1);
			unitsManager.GetUnitObject (unit).transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
		}
	}

	private void ProcessImmediateAction () {
		IAction action = immediateActionsQueue_ [0];
		action.Commit (map, unitsManager, itemsManager);
		if (action is IUnitAction) {
			IUnit unit = (action as IUnitAction).unit;
			unitsManager.GetUnitObject (unit).transform.position = new Vector3 (unit.position.x, unit.position.y, 0.0f);
		}

		immediateActionsQueue_.RemoveAt (0);
		MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionFinished", action);
		SetupNextImmediateAction ();
	}

	private void SetupNextImmediateAction () {
		if (immediateActionsQueue_.Count > 0) {
			IAction action = immediateActionsQueue_ [0];
			immediateActionsElapsedTime_ += action.time;

			if (immediateActionsElapsedTime_ > 1.0f) {
				immediateActionsQueue_.Clear ();
				MessageUtils.SendMessageToObjectsWithTag (tag, "AllImmediateActionsFinished", null);
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionsMaxTimeReached", null);

			} else {
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionStart", action);
				action.SetupAnimations (tag, map, unitsManager, itemsManager);
			}
		} else {
			MessageUtils.SendMessageToObjectsWithTag (tag, "AllImmediateActionsFinished", null);
			if (immediateActionsElapsedTime_ >= 1.0f) {
				MessageUtils.SendMessageToObjectsWithTag (tag, "ImmediateActionsMaxTimeReached", null);
			}
		}
	}

	private void UpdateItems () {
		for (int index = 0; index < itemsManager.GetItemsCount (); index++) {
			itemsManager.GetItemByIndex (index).ProcessTurn (map, unitsManager, itemsManager);
		}
		itemsManager.UpdateItemsSpritesByVisionMap (unitsManager.visionMapProvider);
	}
}
