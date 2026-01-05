using UnityEngine;

public class Warehouse : MonoBehaviour {
	private readonly int[] inventory = { 0, 00 };

	public void AddToWarehouse(int[] items) {
		for (int i = 0; i < inventory.Length; ++i) {
			inventory[i] += items[i];
		}
	}

	public void TakeFromWarehouse(int[] items, int[] targetInventory) {
		for (int i = 0; i < inventory.Length; ++i) {
			if (items[i] <= inventory[i]) {
				targetInventory[i] += inventory[i];
				inventory[i] -= items[i];
			}
		}
	}
}
