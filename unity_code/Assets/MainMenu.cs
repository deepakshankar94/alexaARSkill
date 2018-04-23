using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public GameObject ConnectWithAlexaButton;
    public GameObject ContinueGameButton;
    public GameObject NewGameButton;
    public GameObject JoinGameButton;

    public GameObject SameAlexaButton;
    public GameObject DifferentAlexaButton;

    private void OnEnable()
    {
        ContinueGameButton.SetActive(PlayerPrefs.HasKey("CurrentGameCode"));
        SameAlexaButton.SetActive(false);
        DifferentAlexaButton.SetActive(false);
    }
}
