using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContinueGameMenu : MonoBehaviour {

    public TextMeshProUGUI statusText;
    public TextMeshProUGUI currentGameCodeText;
    public Button ContinueGameButton;

    private void OnEnable()
    {
        currentGameCodeText.text = "Current Game Code: " + PlayerPrefs.GetString("CurrentGameCode").Replace('_', ' ');
    }
}
