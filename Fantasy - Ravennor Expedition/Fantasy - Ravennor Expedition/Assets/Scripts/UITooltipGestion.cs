using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITooltipGestion : MonoBehaviour
{
    public static UITooltipGestion instance;

    [SerializeField]
    private Camera cam;
    [SerializeField]
    private TextMeshProUGUI textBlock;

    private GameObject tooltipUser;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        gameObject.SetActive(false);
    }

    public static void Show(string txt, GameObject sTooltipUser)
    {
        if (instance != null)
        {
            instance.ShowTips(txt, sTooltipUser);
        }
    }

    public static void Hide(GameObject sTooltipUser)
    {
        if (instance != null)
        {
            instance.HideTips(sTooltipUser);
        }
    }

    private void ShowTips(string textToShow, GameObject sTooltipUser)
    {
        tooltipUser = sTooltipUser;
        textBlock.text = textToShow;
        //transform.position = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = tooltipUser.transform.position;
        gameObject.SetActive(true);
    }

    private void HideTips(GameObject sTooltipUser)
    {
        if (sTooltipUser == tooltipUser)
        {
            gameObject.SetActive(false);
            tooltipUser = null;
        }
    }
}
