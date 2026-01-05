using UnityEngine;

public class Buildable : MonoBehaviour {
	private bool isComplete;
	private string buildType;

	void Awake() {
		isComplete = false;
	}

	/*
	 * -- Getter methods --
	 */
	public bool IsComplete() {
		return isComplete;
	}

	public string GetBuildType() {
		return buildType;
	}

	/*
	 * -- Setter methods --
	 */
	public void SetComplete() {
		isComplete = true;
	}

	public void SetBuildType(string newBuildType) {
		buildType = newBuildType;
	}
}
