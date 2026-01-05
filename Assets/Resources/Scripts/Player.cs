using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
	private Keybinds input;
	private readonly float speed = 5f;

	private bool isPaused;

	private string buildTarget;
	private int buildRotation = 0;

	private GameObject selectedNPC;

	private NPCHandler npcHandler;
	private MapHandler mapHandler;
	private UIHandler uiHandler;
	private UIClickThroughBlocker uiClickBlocker;

	void Awake() {
		DontDestroyOnLoad(gameObject);

		input = new();
		input.Gameplay.Enable();
	}

	void Start() {
		isPaused = false;

		buildTarget = "Nothing";

		selectedNPC = null;

		npcHandler = GameObject.Find("EventSystem").GetComponent<NPCHandler>();
		mapHandler = GameObject.Find("EventSystem").GetComponent<MapHandler>();
		uiHandler = GameObject.Find("EventSystem").GetComponent<UIHandler>();
		uiClickBlocker = GameObject.Find("UI").GetComponent<UIClickThroughBlocker>();
	}

	void Update() {
		if (input.Gameplay.Pause.triggered) {
			if (buildTarget == "Nothing") {
				isPaused = !isPaused;
			} else {
				mapHandler.ExitBuildMode();
			}
		}

		if (!isPaused) {
			if (buildTarget != "Nothing") {
				if (input.Gameplay.Rotate.triggered) {
					if (buildRotation == 270) {
						buildRotation = 0;
					} else {
						buildRotation += 90;
					}
				}

				PlaceObject();
			}

			SelectColonist();
		}
	}

	void FixedUpdate() {
		if (!isPaused) {
			int vertical = Convert.ToInt16(input.Gameplay.Forwards.IsPressed()) - Convert.ToInt16(input.Gameplay.Backwards.IsPressed());
			int horizontal = Convert.ToInt16(input.Gameplay.Right.IsPressed()) - Convert.ToInt16(input.Gameplay.Left.IsPressed());

			float newZ = transform.position.z + vertical * speed * Time.fixedDeltaTime;
			float newX = transform.position.x + horizontal * speed * Time.fixedDeltaTime;

			newZ = Mathf.Clamp(newZ, -22.8f, 15.0f);
			newX = Mathf.Clamp(newX, -16.8f, 15.8f);

			transform.position = new(newX, 10f, newZ);
		}
	}

	private void PlaceObject() {
		if (input.Gameplay.Select.IsPressed() && !uiClickBlocker.IsUIBlocking()) {
			switch (buildTarget) {
				case "Small House":
					mapHandler.Build("Small House");
					break;
			}
		}
	}

	private void SelectColonist() {
		if (input.Gameplay.Select.IsPressed() && !uiClickBlocker.IsUIBlocking()) {
			GameObject[] colonistList = npcHandler.GetColonistList();

			foreach (GameObject colonist in colonistList) {
				if (Vector3.Distance(colonist.transform.position, GetCursorWorldPos()) <= 1f) {
					selectedNPC = colonist;
					uiHandler.UpdateNPCDisplay();
					uiHandler.ShowNPCDisplay();
					break;
				} else {
					if (!uiClickBlocker.IsUIBlocking()) {
						selectedNPC = null;
						uiHandler.HideNPCDisplay();
					}
				}
			}
		}
	}

	public Vector3 GetCursorWorldPos() {
		Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

		if (Physics.Raycast(ray, out RaycastHit hit)) {
			return new(Mathf.RoundToInt(hit.point.x), 0.6f, Mathf.RoundToInt(hit.point.z));
		}

		return Vector3.zero;
	}

	public void SetBuildTarget(string newTarget) {
		buildTarget = newTarget;
	}

	public GameObject GetSelectedColonist() {
		return selectedNPC;
	}

	public int GetBuildRotation() {
		return buildRotation;
	}
}
