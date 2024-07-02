using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DisplayHealthOnTexture))]
public class DisplayHealthCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DisplayHealthOnTexture displayHealth = (DisplayHealthOnTexture)target;
        if(GUILayout.Button("Update Texture"))
        {
            displayHealth.UpdateHealthDisplay();
        }
    }
}
