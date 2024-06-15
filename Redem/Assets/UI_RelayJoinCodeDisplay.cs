using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_RelayJoinCodeDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    // Start is called before the first frame update
    void Start()
    {
        displayText.text = SceneTransitionHandler.Singleton.JoinRelayCode;
    }
}
