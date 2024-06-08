using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Multiplayer : MonoBehaviour
{
    [SerializeField] private InterfaceButton hostButton;
    [SerializeField] private InterfaceButton clientButton;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(hostButton.Selected)
        {
            SceneTransitionHandler.Singleton.InitializeAsHost = true;
            SceneTransitionHandler.Singleton.Initialize();
        }
        if(clientButton.Selected)
        {
            SceneTransitionHandler.Singleton.InitializeAsHost = false;
            SceneTransitionHandler.Singleton.Initialize();
        }
    }
}
