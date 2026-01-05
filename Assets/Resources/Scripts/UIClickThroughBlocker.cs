using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickThroughBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	private bool clickBlocked;

	public void OnPointerEnter(PointerEventData eventData) {
		clickBlocked = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		clickBlocked = false;
	}

	public bool IsUIBlocking() {
		return clickBlocked;
	}
}