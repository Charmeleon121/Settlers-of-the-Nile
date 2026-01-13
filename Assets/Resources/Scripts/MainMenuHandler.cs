using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour {
	public void NewGame() {
		SceneManager.LoadScene("World");
	}

	public void LoadGame() {
		// TODO
	}

	public void Options() {
		// TODO
	}

	public void QuitGame() {
		Application.Quit();
	}
}
