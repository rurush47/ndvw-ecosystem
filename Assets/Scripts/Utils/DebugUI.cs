using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentStateText;

    public void UpdateStateText(string text)
    {
        currentStateText.text = text;
    }
}
