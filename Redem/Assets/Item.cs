using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Item : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;
    [field:SerializeField] public SlotType ItemType { get; private set; }
    [field: SerializeField] public string Value { get; private set; }
    void Start()
    {
        label.text = Value;
    }
}