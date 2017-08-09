public interface IAction {
	void SetupAnimations (string objectsTag, Map map, UnitsManager unitsManager, ItemsManager itemsManager);
	void Commit (Map map, UnitsManager unitsManager, ItemsManager itemsManager);
    float time { get; }
}
