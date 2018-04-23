using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectWithAlexaMenu : MonoBehaviour {

    public TextMeshProUGUI code;

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("AlexaCode"))
        {
            code.text = PlayerPrefs.GetString("AlexaCode").Replace('_', ' ');
        }
    }

}
