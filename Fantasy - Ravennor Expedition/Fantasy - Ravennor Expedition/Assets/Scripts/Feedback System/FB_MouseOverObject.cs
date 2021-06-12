using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FB_MouseOverObject : MonoBehaviour
{
    [SerializeField]
    private UnityEvent overEvt, exitEvt;

    private void OnMouseEnter()
    {
        overEvt.Invoke();
    }

    private void OnMouseExit()
    {
        exitEvt.Invoke();
    }
}
