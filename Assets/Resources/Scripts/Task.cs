using UnityEngine;

public class Task {
	private readonly char taskType;             // Type of the task (mine, build, etc.)
	private readonly int[] taskRequirements;	// What materials the task requires to be completed
	private readonly GameObject taskObject;     // Where the task is in the world
	private bool taskCompleted;					// Whether the task has been completed or not

	/*
	 * Constructor for creating a new Task object
	 */
	public Task(char type, int[] requirements, GameObject obj) {
		taskType = type;
		taskRequirements = requirements;
		taskObject = obj;
		taskCompleted = false;
	}

	/*
	 * -- Getter methods --
	 */
	public char GetTaskType() {
		return taskType;
	}

	public int[] GetTaskRequirements() {
		return taskRequirements; 
	}

	public GameObject GetTaskObject() {
		return taskObject;
	}

	public Vector3 GetLocation() {
		return taskObject.transform.position;
	}

	public bool IsTaskCompleted() {
		return taskCompleted;
	}

	/*
	 * -- Setter methods --
	 */
	public void SetTaskCompleted() {
		taskCompleted = true;
		taskObject.GetComponent<Buildable>().SetComplete();
	}
}
