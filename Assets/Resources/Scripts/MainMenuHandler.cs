using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour {
	// Version and update related information
	private readonly string majorVersion = "0";		// Increment for major game updates
	private readonly string minorVersion = "00";	// Increment when significant features added
	private readonly string bugFixVersion = "009";	// Number of Git commits since last update
	private readonly DateTime releaseDate = new(2026, 1, 13);
	public TextMeshProUGUI versionInfo;

	// Options menu components
	public GameObject optionsMenu;
	public TMP_InputField fpsInputField;
	public Toggle vsyncToggle;

	// Loading screen components
	public GameObject loadingScreen;
	public RawImage loadingSymbol;

	void Start() {
		versionInfo.text = $"v{majorVersion}.{minorVersion}.{bugFixVersion} ({releaseDate:dd/MM/yyyy})";

		fpsInputField.characterLimit = 3;

		CloseOptions();

		loadingScreen.transform.position = new(960f, -540f, 0f);
		loadingScreen.GetComponent<Image>().color = new(0f, 0f, 0f, 0f);

		StartCoroutine(RotateLoadingSymbol());
	}

	// Create a new world
	public void NewGame() {
		loadingScreen.transform.position = new(960f, 540f, 0f);
		StartCoroutine(OpenLoadingScreen());
	}

	// Open the "load game" menu, where the player can load/delete saves
	public void LoadGame() {
		// TODO: implement save system first
	}

	// Open the options menu
	public void OpenOptions() {
		optionsMenu.transform.position = new(960f, 540f, 0f);

		fpsInputField.text = $"{Application.targetFrameRate}";
		vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
	}

	// Apply all settings in the options menu
	public void ApplySettings() {
		if (fpsInputField.text.Length > 0) {
			try {
				Application.targetFrameRate = int.Parse(fpsInputField.text);
			} catch (FormatException) {
				Application.targetFrameRate = 60;	// Assuming the player entered non-numeric characters, default to 60
			}
		} else {
			Application.targetFrameRate = 60;		// Assuming the player entered nothing, default to 60
		}

		QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
		QualitySettings.antiAliasing = 0;			// Options are: 0, 2, 4, 8 - THIS IS TEMPORARY
	}

	// Close the options menu (without saving!)
	public void CloseOptions() {
		optionsMenu.transform.position = new(960f, -540f, 0f);
	}

	// Quits out of the game
	public void QuitGame() {
		Application.Quit();
	}

	// Open the loading screen slowly, then transition to the game world scene
	private IEnumerator OpenLoadingScreen() {
		while (loadingScreen.GetComponent<Image>().color.a < 0.99f) {
			float newAlpha = Mathf.Lerp(loadingScreen.GetComponent<Image>().color.a, 1f, 0.1f);
			loadingScreen.GetComponent<Image>().color = new(0f, 0f, 0f, newAlpha);

			yield return null;
		}

		loadingScreen.GetComponent<Image>().color = new(0f, 0f, 0f, 1f);
		SceneManager.LoadScene("World");
		yield return null;
	}

	// Rotate the loading symbol
	private IEnumerator RotateLoadingSymbol() {
		while (true) {
			float zRot = loadingSymbol.transform.rotation.eulerAngles.z;
			loadingSymbol.transform.rotation = Quaternion.Euler(0f, 0f, --zRot);
			yield return null;
		}
	}
}
