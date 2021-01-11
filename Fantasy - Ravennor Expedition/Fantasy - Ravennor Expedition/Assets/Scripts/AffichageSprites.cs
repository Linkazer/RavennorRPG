using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffichageSprites : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spr;
    public int offset = 0;
    [SerializeField]
    private SpriteRenderer originSpr;

    private void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        if(spr == null)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (originSpr == null)
        {
            spr.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100) + offset;
        }
        else
        {
            spr.sortingOrder = originSpr.sortingOrder + offset;
        }
    }
}
