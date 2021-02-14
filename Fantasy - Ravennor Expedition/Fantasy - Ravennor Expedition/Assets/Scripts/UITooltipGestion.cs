using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITooltipGestion : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private TextMeshProUGUI textBlock;
    public void ShowTips(string textToShow)
    {
        textBlock.text = textToShow;
        transform.position = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        gameObject.SetActive(true);
    }

    public void HideTips()
    {
        gameObject.SetActive(false);
    }
}
