using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAction_PlayDialogue : MonoBehaviour, IRoomAction
{
    [SerializeField] private ParcheminScriptable dialog;
    public void PlayAction()
    {
        BattleUiManager.instance.StartDialogue(dialog);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
