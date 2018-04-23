using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinGameMenu : MonoBehaviour {

    public TMP_InputField code;
    public GameManager gameManager;
    public GameObject joinButton;

    private void OnEnable()
    {
        joinButton.SetActive(false);
    }

    public void OnJoinButtonClick()
    {
        Debug.Log("Join button clicked");
        if (code.text != null)
        {
            Debug.Log("Code to join is: " + code.text);
            gameManager.JoinGame(PlayerPrefs.GetString("CurrentGameCode"));
        }
    }
	
}
