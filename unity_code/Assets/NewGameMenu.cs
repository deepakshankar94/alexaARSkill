using UnityEngine;

public class NewGameMenu : MonoBehaviour {

    public GameObject startGameButton;

    private void OnEnable()
    {
        startGameButton.gameObject.SetActive(false);
    }
}
