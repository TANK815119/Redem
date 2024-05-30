using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//height adjuster hard-coded for a 190cm player model
public class HeightAdjuster : MonoBehaviour
{
    [SerializeField] private float playerHeight = 190f;
    [SerializeField] private float modelHeight = 190f;
    [SerializeField] private RigXRTransformer rigTransformer;
    [SerializeField] private RagdollXRTransformer ragdollTransformer;
    [SerializeField] private Transform headCamera;
    [SerializeField] private InterfaceButton incrementButton;
    [SerializeField] private InterfaceButton decrementButton;
    [SerializeField] private TMPro.TMP_Text textMesh;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate() //has to be fixed update for some reason
    {
        //listen for button selections
        if(incrementButton.Selected)
        {
            playerHeight += 1f;
        }
        if(decrementButton.Selected)
        {
            playerHeight -= 1f;
        }

        //set the scale in accordance to the set height
        float scale = playerHeight / modelHeight;
        rigTransformer.Scale = 1f / scale;
        ragdollTransformer.Scale = scale;
        headCamera.localScale = new Vector3(1f / scale, 1f / scale, 1f / scale) * 10f;

        //display the height
        textMesh.text = ((int)playerHeight).ToString() + "cm" +"\n"
            + ((int)(playerHeight * 0.393701f)).ToString() + "in";
    }
}
