using UnityEngine;

public class House : MonoBehaviour {
	private int maxInhabitants;
	private GameObject[] inhabitants;
	
	void Awake() {
		if (gameObject.name.Contains("Basic House")) {
			maxInhabitants = 3;
		}
		
		inhabitants = new GameObject[maxInhabitants];
	}
	
	public void SetInhabitant(GameObject colonist, int index) {
		if (index >= 0 && index < maxInhabitants) {
			inhabitants[index] = colonist;
			colonist.GetComponent<Colonist>().SetHouse(gameObject);
		}
	}
	
	public void RemoveInhabitant(int index) {
		if (index >= 0 && index < maxInhabitants) {
			inhabitants[index].GetComponent<Colonist>().SetHouse(null);
			inhabitants[index] = null;
		}
	}
}