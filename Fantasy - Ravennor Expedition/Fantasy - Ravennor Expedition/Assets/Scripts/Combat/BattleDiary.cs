using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleDiary : MonoBehaviour
{
    public static BattleDiary instance;

    [SerializeField]
    private TextMeshProUGUI diary;

    private void Awake()
    {
        instance = this;
    }

    public void AddText(string txt)
    {
        diary.text = diary.text + "\n" + txt;
    }
}
