using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Rekabsen
{
    public class ButtonSceneTransition : NetworkBehaviour, ButtonActionInterface
    {
        [SerializeField] private string sceneName;
        private NetworkVariable<bool> transitionCalled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Update is called once per frame
        void Update()
        {
            if (transitionCalled.Value)
            {
                SceneTransition();
            }
        }

        public void Play()
        {
            transitionCalled.Value = true;
        }

        private void SceneTransition()
        {
            SceneTransitionHandler.Singleton.SetSceneName(sceneName);
            if (IsHost)
            {
                SceneTransitionHandler.Singleton.InitializeAsHost = true;
                SceneTransitionHandler.Singleton.InitializeAsMultiplayer = true;
                //SceneTransitionHandler.Singleton.JoinRelayCode = keyboard.GetText(); -- not neccessary
                SceneTransitionHandler.Singleton.Initialize();
            }
            if (!IsHost)
            {
                SceneTransitionHandler.Singleton.InitializeAsHost = false;
                SceneTransitionHandler.Singleton.InitializeAsMultiplayer = true;
                //SceneTransitionHandler.Singleton.JoinRelayCode = keyboard.GetText(); should be same as innitial join
                SceneTransitionHandler.Singleton.Initialize();
            }
        }
    }
}