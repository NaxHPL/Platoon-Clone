using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UiQuitGameButton : MonoBehaviour {

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick() {
        Application.Quit();
    }
}
