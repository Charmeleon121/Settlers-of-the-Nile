using UnityEngine;

public class MapHandler : MonoBehaviour {
	// Prefabs and materials
	private GameObject groundBlockPrefab, waterBlockPrefab, colonistPrefab, smallHousePrefab;
	private GameObject ghostSmallHousePrefab;
	private Material ghostMaterial, ghostErrorMaterial, sandMaterial;
	private Shader grassShader;

	// All ground blocks and water blocks
	private GameObject[] groundBlocks, waterBlocks, allBlocks;

	// All ghost blocks
	private GameObject[] ghostObjects;

	// All colonists, all placed buildables, and buildings
	private GameObject[] colonists, placedBuildables, houses;

	// Whether the player can place a block or not
	private bool canBuild;

	// The sun and it's rotation
	private GameObject sun;
	private Light sunlight;
	private readonly Color earlyColor = new(0.84f, 0.37f, 0.03f);
	private readonly Color dayColor = new(0.99f, 0.91f, 0.69f);
	private readonly float daySpeed = 0.01f;
	private float rotX = 0f;
	private int day = 1;

	// Parameters for the ground culling with the camera
	private readonly int topWidth = 28;
	private readonly int bottomWidth = 18;
	private readonly int height = 15;
	
	// The previous position of the camera
	private Vector3 prevCameraPos;

	// Other scripts
	private Player playerScript;
	private NPCHandler npcHandler;

	/*
	 * On Awake, initialize all prefab variables, material/shader variables, external
	 * object variables, and external scripts
	 */
	void Awake() {
		groundBlockPrefab = Resources.Load<GameObject>("Blocks/Ground");
		waterBlockPrefab = Resources.Load<GameObject>("Blocks/Water");
		colonistPrefab = Resources.Load<GameObject>("NPCs/Colonist");
		smallHousePrefab = Resources.Load<GameObject>("Models/Prefabs/Basic House");

		ghostSmallHousePrefab = Resources.Load<GameObject>("Models/Prefabs/Basic House Ghost");

		ghostMaterial = Resources.Load<Material>("Materials/Ghost Material");
		ghostErrorMaterial = Resources.Load<Material>("Materials/Ghost Error Material");
		sandMaterial = Resources.Load<Material>("Materials/Sand Material");
		grassShader = Resources.Load<Shader>("Materials/Grass Shader");

		sun = GameObject.Find("Sun");
		sunlight = sun.GetComponent<Light>();
		sun.transform.rotation = Quaternion.Euler(rotX, 0f, 0f);
		
		prevCameraPos = Camera.main.transform.position;

		playerScript = GameObject.Find("Player").GetComponent<Player>();
		npcHandler = GameObject.Find("EventSystem").GetComponent<NPCHandler>();
	}

	/*
	 * On Start, generate a map of a given size
	 */
	void Start() {
		GenerateMap(100, 100);
	}

	/*
	 * Every frame, handle the camera culling so out-of-view ground blocks are de-rendered,
	 * progress the day and angle of the sun, update any build ghosts, and build any objects
	 * which have an associated task marked as complete
	 */
	void Update() {
		// Only update the culling if the camera moved
		if (Camera.main.transform.position != prevCameraPos) {
			CameraCulling();
		}
		
		DayProgression();
		UpdateBuildGhostPos();
		BuildCompleteBuildables();
		
		// Update the previous camera position
		prevCameraPos = Camera.main.transform.position;
	}

	// A sinusoidal function to determine the shape/length of the river
	private int GetRiverTilePos(float zPos) {
		return Mathf.RoundToInt(2f * Mathf.Sin(zPos / 8f) + (Mathf.Cos(10f * zPos) / 20f));
	}

	// The map is generated in this function
	private void GenerateMap(int width, int length) {
		width = width % 2 == 0 ? width : width + 1;
		length = length % 2 == 0 ? length : length + 1;

		GameObject newBlock;
		for (int z = -length / 2; z < length / 2; ++z) {
			int riverXPos = GetRiverTilePos(z);

			for (int x = -width / 2; x < width / 2; ++x) {
				if (x >= riverXPos - (width / 30) && x <= riverXPos + (width / 30)) {
					newBlock = Instantiate(waterBlockPrefab, new(x, -0.2f, z), Quaternion.Euler(0f, 0f, 0f));
					newBlock.tag = "Water";
				} else {
					newBlock = Instantiate(groundBlockPrefab, new(x, 0f, z), Quaternion.Euler(0f, 0f, 0f));
					newBlock.tag = "Ground";
					newBlock.GetComponent<MeshRenderer>().material = sandMaterial;
				}
			}
		}

		PlaceGrassTiles(width / 25);

		// Start with 10 initial colonists
		string[] jobs = { "Unemployed", "Builder", "Farmer", "Miner" };

		GameObject newColonist;
		for (int i = 0; i < 10; ++i) {
			int centreX = -6;
			int centreZ = 12;

			int xPos = centreX + Random.Range(-3, 3);
			int zPos = centreZ + Random.Range(-7, 8);

			newColonist = Instantiate(colonistPrefab, new(xPos, 0.6f, zPos), Quaternion.Euler(0f, 0f, 0f));
			newColonist.tag = "Colonist";

			int index = Random.Range(0, jobs.Length);

			newColonist.GetComponent<Colonist>().SetJob(jobs[index]);
		}
	}

	// This function determines where grass tiles should be around the river edge
	private void PlaceGrassTiles(float riverHalfWidth) {
		groundBlocks = GameObject.FindGameObjectsWithTag("Ground");
		waterBlocks = GameObject.FindGameObjectsWithTag("Water");

		int grassWidth = Mathf.RoundToInt(2 * riverHalfWidth);
		
		Vector3 originPoint;
		Material mat;
		foreach (GameObject ground in groundBlocks) {
			originPoint = GetNearestWaterTilePos(ground);
			if (Mathf.Abs(ground.transform.position.x - originPoint.x) <= grassWidth && ground.transform.position.z == originPoint.z) {
				if (ground.transform.position.x < 0f) {
					originPoint.x -= 1;
				} else {
					originPoint.x += 1;
				}

				/*
				 * Creating a custom material here as part of the terrain blending from
				 * grass to sand depending on the distance of the block to the nearest
				 * river bank
				 */
				mat = new(grassShader);
				mat.SetFloat("_Divisor", 8);
				mat.SetVector("_Target_Position", originPoint);

				ground.GetComponent<Renderer>().material = mat;
				break;
			}
		}
	}

	// Get the position of the nearest water tile to a given ground block
	private Vector3 GetNearestWaterTilePos(GameObject groundBlock) {
		float distance = Mathf.Infinity;
		GameObject closest = null;
		Vector3 waterPosition;
		foreach (GameObject water in waterBlocks) {
			waterPosition = new(water.transform.position.x, 0f, water.transform.position.z);

			if (Mathf.Abs(waterPosition.z - groundBlock.transform.position.z) <= Mathf.Sqrt(2)) {
				if (Vector3.Distance(groundBlock.transform.position, waterPosition) < distance) {
					distance = Mathf.Abs(groundBlock.transform.position.x - water.transform.position.x);
					closest = water;
				}
			}
		}

		return closest.transform.position;
	}

	// This function hides any ground/water block that is not currently within the camera's view
	private void CameraCulling() {
		groundBlocks = GameObject.FindGameObjectsWithTag("Ground");
		waterBlocks = GameObject.FindGameObjectsWithTag("Water");
		
		allBlocks = new GameObject[groundBlocks.Length + waterBlocks.Length];
		groundBlocks.CopyTo(allBlocks, 0);
		waterBlocks.CopyTo(allBlocks, groundBlocks.Length);

		int layerMask = 1 << 3; // This makes sure the raycast only looks at layer 3 (terrain) by bit-shifting the index of the layer
		if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask)) {
			Vector3 centre = hit.collider.gameObject.transform.position;

			Vector3 relativePosition;
			float verticalFactor, halfWidthAtZ;
			foreach (GameObject block in allBlocks) {
				relativePosition = block.transform.position - centre;
				verticalFactor = (relativePosition.z + height * 0.5f) / height;
				halfWidthAtZ = Mathf.Lerp(bottomWidth * 0.5f, topWidth * 0.5f, verticalFactor);

				if (relativePosition.z >= -height * 0.5f && relativePosition.z <= height * 0.5f && relativePosition.x >= -halfWidthAtZ && relativePosition.x <= halfWidthAtZ) {
					block.GetComponent<Renderer>().enabled = true;
				} else {
					block.GetComponent<Renderer>().enabled = false;
				}
			}
		}
	}

	// Progress the day, as well as adjusting the sun's intensity and color accordingly
	private void DayProgression() {
		if (rotX >= 360f) {
			rotX = 0f;
			++day;
		} else {
			rotX += daySpeed;
		}

		if (rotX <= 18f || rotX >= 162f) {
			sunlight.color = earlyColor;
		} else if (rotX > 18f && rotX <= 40f) {
			sunlight.color = Color.Lerp(earlyColor, dayColor, (rotX - 18f) / 22f);
		} else if (rotX >= 120f && rotX < 162f) {
			sunlight.color = Color.Lerp(dayColor, earlyColor, (rotX - 120f) / 42f);
		} else {
			sunlight.color = dayColor;
		}

		if (((rotX - 360 >= -5f || rotX > 0f) && rotX < 18f) || (rotX > 162f && rotX < 185f)) {
			sunlight.intensity = 0.5f;
		} else if (rotX > 18f && rotX < 90f) {
			sunlight.intensity = Mathf.Lerp(0.5f, 1f, (rotX - 18f) / 72f);
		} else if (rotX > 90f && rotX < 162f) {
			sunlight.intensity = Mathf.Lerp(1f, 0.5f, (rotX - 90f) / 72f);
		} else {
			sunlight.intensity = 0f;
		}

		sun.transform.rotation = Quaternion.Euler(rotX, 90f, 90f);
	}

	// Ensure the active build ghost follows the cursor so the player can place it
	private void UpdateBuildGhostPos() {
		ghostObjects = GameObject.FindGameObjectsWithTag("Ghost");

		if (ghostObjects.Length > 0) {
			colonists = GameObject.FindGameObjectsWithTag("Colonist");
			placedBuildables = GameObject.FindGameObjectsWithTag("Buildable");
			houses = GameObject.FindGameObjectsWithTag("House");

			Vector3 cursorPos = playerScript.GetCursorWorldPos();

			if (!SpaceOccupied(colonists, cursorPos) && !SpaceOccupied(placedBuildables, cursorPos) && !SpaceOccupied(houses, cursorPos)) {
				SetMaterialInChildren(ghostObjects[0], ghostMaterial);
				canBuild = true;
			} else {
				SetMaterialInChildren(ghostObjects[0], ghostErrorMaterial);
				canBuild = false;
			}

			ghostObjects[0].transform.position = cursorPos;
			int rotation = playerScript.GetBuildRotation();
			ghostObjects[0].transform.rotation = Quaternion.Euler(0f, rotation, 0f);
		}
	}

	// Remove the build ghost and reset the player's build target to "exit" build mode
	public void ExitBuildMode() {
		Destroy(ghostObjects[0]);
		playerScript.SetBuildTarget("Nothing");
	}

	// Determine the build ghost to show the player, or exit build mode accordingly
	public void BuildMode(string target) {
		ghostObjects = GameObject.FindGameObjectsWithTag("Ghost");

		if (ghostObjects.Length > 0) {
			// There is already a ghost on the player's cursor, exit out of placement mode
			ExitBuildMode();
		} else {
			// Enter into placement mode
			if (target == "Small House") {
				GameObject ghostHouse = Instantiate(ghostSmallHousePrefab, new(0f, 0.6f, 0f), Quaternion.Euler(0f, 0f, 0f));
				ghostHouse.tag = "Ghost";
				playerScript.SetBuildTarget("Small House");
			}
		}
	}

	// In effect, "place" the build ghost in the world for colonists to actually build
	public void Build(string target) {
		if (canBuild) {
			int rotation = playerScript.GetBuildRotation();

			switch (target) {
				case "Small House":
					Vector3 cursorPos = playerScript.GetCursorWorldPos();
					GameObject builtObject = Instantiate(ghostSmallHousePrefab, cursorPos, Quaternion.Euler(0f, rotation, 0f));
					builtObject.tag = "Buildable";
					builtObject.GetComponent<Buildable>().SetBuildType("Small House");

					int[] requirements = new int[] { 0, 10 };

					npcHandler.AddTaskToQueue('B', requirements, builtObject);
					break;
			}
		}
	}

	// If any placed build ghost is ready to be built, actually build it
	private void BuildCompleteBuildables() {
		placedBuildables = GameObject.FindGameObjectsWithTag("Buildable");

		if (placedBuildables.Length > 0) {
			Buildable buildScript;
			foreach (GameObject buildable in placedBuildables) {
				buildScript = buildable.GetComponent<Buildable>();

				if (buildScript.IsComplete()) {
					if (buildScript.GetBuildType() == "Small House") {
						Instantiate(smallHousePrefab, buildable.transform.position, buildable.transform.rotation);
						Destroy(buildable);
						break;
					}
				}
			}
		}
	}

	// Determine if the place the player is trying to put an object is blocked by another object
	private bool SpaceOccupied(GameObject[] array, Vector3 targetPos) {
		bool found = false;

		// TODO: Modify this so that the player can't place things (other than bridges) on water
		foreach (GameObject obj in array) {
			if (Vector3.Distance(obj.transform.position, targetPos) <= 1f) {
				found = true;
				break;
			}
		}

		return found;
	}

	// Set the material of child objects - for when an object is not a single mesh
	private void SetMaterialInChildren(GameObject parent, Material mat) {
		int childCount = parent.transform.childCount;

		GameObject childObject;
		for (int i = 0; i < childCount; ++i) {
			childObject = parent.transform.GetChild(i).gameObject;
			childObject.GetComponent<Renderer>().material = mat;
		}
	}

	/*
	 * -- Getter methods --
	 */
	public float GetSunAngle() {
		return rotX;
	}

	public int GetDay() {
		return day;
	}
}
