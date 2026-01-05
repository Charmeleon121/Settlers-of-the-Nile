using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour {
	private TextMeshProUGUI fpsDisplay;
	private int fpsTimer;

	private Slider dayNightSlider;
	private TextMeshProUGUI dayLabel;

	public Button housingButton, smallHouseButton;
	private GameObject houseSelectMenu;

	private GameObject colonistPropertyDisplay;
	public TextMeshProUGUI colonistNameLabel, colonistJobLabel;

	private Player playerScript;
	private MapHandler mapHandler;

	void Awake() {
		Application.targetFrameRate = 165;	// Target FPS - for now, set to 165

		QualitySettings.vSyncCount = 1;		// 0 - disable | 1 - enable
		QualitySettings.antiAliasing = 0;	// Options are: 0, 2, 4, 8
	}

	void Start() {
		fpsDisplay = GameObject.Find("FPS Display").GetComponent<TextMeshProUGUI>();
		fpsTimer = 0;

		dayNightSlider = GameObject.Find("Day Night Slider").GetComponent<Slider>();
		dayLabel = GameObject.Find("Day Label").GetComponent<TextMeshProUGUI>();

		housingButton.onClick.AddListener(() => SwapHouseSelectMenuVisibility());
		smallHouseButton.onClick.AddListener(() => mapHandler.BuildMode("Small House"));

		houseSelectMenu = GameObject.Find("House Select Menu");
		houseSelectMenu.transform.position = new(510f, -112.5f);

		colonistPropertyDisplay = GameObject.Find("Colonist Properties");
		colonistPropertyDisplay.transform.position = new(1720f, -280f);

		playerScript = GameObject.Find("Player").GetComponent<Player>();
		mapHandler = GameObject.Find("EventSystem").GetComponent<MapHandler>();
	}

	void Update() {
		UpdateDaySlider();
		UpdateFPSDisplay();
	}

	private void UpdateDaySlider() {
		int day = mapHandler.GetDay();
		float time = mapHandler.GetSunAngle();

		dayNightSlider.value = time / 180f;

		if (time > 0 && time <= 18) {
			dayLabel.text = $"Day {day}: Dawn";
		} else if (time > 18 && time <= 80) {
			dayLabel.text = $"Day {day}: Morning";
		} else if (time > 80 && time <= 100) {
			dayLabel.text = $"Day {day}: Noon";
		} else if (time > 100 && time <= 162) {
			dayLabel.text = $"Day {day}: Afternoon";
		} else if (time > 162 && time <= 180) {
			dayLabel.text = $"Day {day}: Evening";
		} else {
			dayLabel.text = $"Day {day}: Night";
		}
	}

	private void UpdateFPSDisplay() {
		if (fpsTimer == 20) {
			float fps = 1 / Time.deltaTime;
			fpsDisplay.text = $"FPS: {fps:n2}";

			fpsTimer = 0;
		} else {
			++fpsTimer;
		}
	}

	public void SwapHouseSelectMenuVisibility() {
		float xPos = houseSelectMenu.transform.position.x;
		float yPos = houseSelectMenu.transform.position.y;
		houseSelectMenu.transform.position = new(xPos, -yPos);
	}

	public void UpdateNPCDisplay() {
		GameObject colonist = playerScript.GetSelectedColonist();

		if (colonist != null) {
			Colonist colonistScript = colonist.GetComponent<Colonist>();
			colonistNameLabel.text = colonistScript.GetName();
			colonistJobLabel.text = $"Occupation: {colonistScript.GetJob()}";
		}
	}

	public void ShowNPCDisplay() {
		float xPos = colonistPropertyDisplay.transform.position.x;
		colonistPropertyDisplay.transform.position = new(xPos, 280f);
	}

	public void HideNPCDisplay() {
		float xPos = colonistPropertyDisplay.transform.position.x;
		colonistPropertyDisplay.transform.position = new(xPos, -280f);
	}
}
