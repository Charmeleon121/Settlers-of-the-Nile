using UnityEngine;

public class Colonist : MonoBehaviour {
	// Information about the colonist
	private string colonistJob, colonistName;
	private int colonistHealth, colonistHunger, colonistThirst, colonistEnergy;
	private readonly float speed = 5f;

	private Task currentTask;

	private readonly int[] inventory = { 0, 0 }; // Dirt, sandstone,

	/*
	 * On Awake, initialize the colonist as unemployed with random attributes
	 * Also, set the initial target position to the colonist's current location
	 */
	void Awake() {
		colonistJob = "Unemployed";
		colonistName = "Steven";    // Placeholder for now
		
		colonistHealth = 100;	// When this hits 0, the colonist dies
		colonistHunger = 0;		// When this hits 100, they are "starving" and will take damage
		colonistThirst = 0;		// When this hits 100, they are "dehydrated" and will take damage
		colonistEnergy = 100;	// When this hits 0, they will pass out and become useless until rested
	}

	/*
	 * Every frame, reset the current task to null if the colonist has completed
	 * the previous one
	 */
	void Update() {
		if (currentTask != null && currentTask.IsTaskCompleted()) {
			currentTask = null;
		}
	}

	/*
	 * Handle the NPC movement in FixedUpdate
	 */
	void FixedUpdate() {
		if (currentTask != null) {
			if (HasRequiredItems()) {
				Move("Task");
			} else {
				Move("Warehouse");
			}
		}
	}

	// Move the colonist depending on what its destination is set to
	private void Move(string destination) {
		Vector3 targetLocation = destination switch {
			"Task" => currentTask.GetLocation(),
			"Warehouse" => FindWarehouse(),
			_ => Vector3.zero,
		};

		if (Mathf.Abs(Vector3.Distance(targetLocation, transform.position)) > 1.5f) {
			// If the colonist isn't within 1.5 units of its target, keep moving it

			Vector3 distance = targetLocation - transform.position;

			Vector3 movementVector;
			if (distance.x == 0f && distance.z == 0f) {
				movementVector = Vector3.zero;
			} else if (distance.x == 0f && distance.z != 0f) {
				movementVector = new(0f, 0f, distance.z / Mathf.Abs(distance.z));
			} else if (distance.x != 0f && distance.z == 0f) {
				movementVector = new(distance.x / Mathf.Abs(distance.x), 0f, 0f);
			} else {
				movementVector = new(distance.x / Mathf.Abs(distance.x), 0f, distance.z / Mathf.Abs(distance.z));
			}

			float newX = transform.position.x + movementVector.x * speed * Time.fixedDeltaTime;
			float newZ = transform.position.z + movementVector.z * speed * Time.fixedDeltaTime;

			transform.position = new(newX, 0.6f, newZ);
		} else {
			/*
			 * If the colonist IS within 1.5 units of its destination, make sure to round its
			 * position components. Check if the components of the position vector are integers
			 * by checking whether sin(pi * component) = 0, since it should = 0 iff component is
			 * an integer
			 */

			float newX, newZ;
			if (Mathf.Sin(Mathf.PI * transform.position.x) != 0f) {
				newX = Mathf.RoundToInt(transform.position.x);
			} else {
				newX = transform.position.x;
			}

			if (Mathf.Sin(Mathf.PI * transform.position.z) != 0f) {
				newZ = Mathf.RoundToInt(transform.position.z);
			} else {
				newZ = transform.position.z;
			}

			transform.position = new(newX, 0.6f, newZ);

			switch (destination) {
				case "Task":
					Build();
					break;
				case "Warehouse":
					GameObject warehouse = GameObject.FindGameObjectWithTag("Warehouse");

					if (warehouse != null) {
						warehouse.GetComponent<Warehouse>().TakeFromWarehouse(currentTask.GetTaskRequirements(), inventory);
					}

					break;
			}
		}
	}

	// Find the warehouse's location in the world
	private Vector3 FindWarehouse() {
		GameObject warehouse = GameObject.FindGameObjectWithTag("Warehouse");

		if (warehouse != null) {
			return warehouse.transform.position;
		}

		return transform.position;
	}

	// Attempt to build the target build ghost
	private void Build() {
		int[] taskRequirements = currentTask.GetTaskRequirements();
		for (int i = 0; i < inventory.Length; ++i) {
			inventory[i] -= taskRequirements[i];
		}

		currentTask.SetTaskCompleted();
	}

	// Check if the colonist has enough materials to build
	private bool HasRequiredItems() {
		int[] taskRequirements = currentTask.GetTaskRequirements();

		bool result = true;

		for (int i = 0; i < inventory.Length; ++i) {
			if (inventory[i] < taskRequirements[i]) {
				result = false;
				break;
			}
		}

		return result;
	}

	/*
	 * -- Getter methods --
	 */
	public string GetJob() {
		return colonistJob;
	}

	public string GetName() {
		return colonistName;
	}

	/*
	 * -- Setter methods --
	 */
	public void SetJob(string newJob) {
		colonistJob = newJob;
	}

	public void SetTask(Task newTask) {
		currentTask = newTask;
	}
}
