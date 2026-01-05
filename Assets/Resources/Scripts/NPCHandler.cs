using System.Collections.Generic;
using UnityEngine;

public class NPCHandler : MonoBehaviour {
	private GameObject[] activeColonists;
	private List<Task> availableTasks;

	void Start() {
		availableTasks = new();
	}

	void Update() {
		foreach (Task task in availableTasks) {
			if (task.IsTaskCompleted()) {
				availableTasks.Remove(task);
				break;
			}
		}
	}

	/*
	 * Add a new task to the queue
	 */
	public void AddTaskToQueue(char type, int[] requirements, GameObject targetObject) {
		availableTasks.Add(new(type, requirements, targetObject));
		DistributeTasks();
	}

	/*
	 * This function handles assigning tasks to colonists
	 */
	private void DistributeTasks() {
		activeColonists = GameObject.FindGameObjectsWithTag("Colonist");

		// Only do anything if there are enough colonists, and actually tasks available
		if (activeColonists.Length > 0 && availableTasks.Count > 0) {
			foreach (Task task in availableTasks) {
				GameObject targetColonist = null;
				Colonist colonistScript = null;
				float minDistance = 1e9f;

				// Find the nearest colonist to the task and assign it to them
				foreach (GameObject colonist in activeColonists) {
					colonistScript = colonist.GetComponent<Colonist>();

					// Check the distance to the colonist from the task, but also check if they are the right type of worker
					// No miners going to build tasks, etc.
					if (Vector3.Distance(colonist.transform.position, task.GetLocation()) < minDistance && colonistScript.GetJob().StartsWith(task.GetTaskType())) {
						targetColonist = colonist;
						minDistance = Vector3.Distance(colonist.transform.position, task.GetLocation());
					}
				}

				// If a colonist was found, assign the task to them
				if (targetColonist != null && colonistScript != null) {
					colonistScript.SetTask(task);
				}
			}
		}
	}

	public GameObject[] GetColonistList() {
		activeColonists = GameObject.FindGameObjectsWithTag("Colonist");
		return activeColonists;
	}
}
