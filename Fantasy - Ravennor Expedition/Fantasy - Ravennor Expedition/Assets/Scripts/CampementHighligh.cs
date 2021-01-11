using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampementHighligh : MonoBehaviour
{
    public Material mat;

    private void OnMouseEnter()
    {
        mat.color = Color.green;
    }

    private void OnMouseExit()
    {
        mat.color = Color.black;
    }
}
