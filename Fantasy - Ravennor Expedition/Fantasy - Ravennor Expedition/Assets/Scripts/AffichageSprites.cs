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
    [SerializeField] private Canvas canvas;

    private Material mat;

    private Vector3 lastPosition = Vector3.zero;

    private void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        if(spr == null)
        {
            enabled = false;
        }
        mat = gameObject.GetComponent<SpriteRenderer>().material;
        spr.material = Instantiate(mat);
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

        if (canvas != null)
        {
            canvas.sortingOrder = spr.sortingOrder;
        }
    }

    private void OnEnable()
    {
        lastPosition = -transform.position;
    }

    private void OnDisable()
    {
        lastPosition = Vector3.zero;
    }
}
