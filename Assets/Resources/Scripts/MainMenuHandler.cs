using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour {
	private readonly string majorVersion = "0";		// Increment for major game updates
	private readonly string minorVersion = "00";	// Increment when significant features added
	private readonly string bugFixVersion = "008";	// Number of Git commits since last update
	private readonly DateTime releaseDate = new(2026, 1, 13);

	public TextMeshProUGUI versionInfo;

	public GameObject optionsMenu;
	public TMP_InputField fpsInputField;
	public Toggle vsyncToggle;

	void Start() {
		versionInfo.text = $"v{majorVersion}.{minorVersion}.{bugFixVersion} ({releaseDate:dd/MM/yyyy})";

		fpsInputField.characterLimit = 3;

		CloseOptions();
	}

	public void NewGame() {
		SceneManager.LoadScene("World");
	}

	public void LoadGame() {
		// TODO
	}

	public void OpenOptions() {
		optionsMenu.transform.position = new(960f, 540f, 0f);

		fpsInputField.text = $"{Application.targetFrameRate}";
		vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
	}

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

	public void CloseOptions() {
		optionsMenu.transform.position = new(960f, -540f, 0f);
	}

	public void QuitGame() {
		Application.Quit();
	}
}
