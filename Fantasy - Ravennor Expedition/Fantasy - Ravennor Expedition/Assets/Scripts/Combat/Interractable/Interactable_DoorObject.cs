using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_DoorObject : RVN_InteractableObject
{
    [SerializeField]
    private List<int> indexs;

    [SerializeField]
    private Animator anim;

    public void DestroyDoor()
    {
        Grid.instance.CreateGrid();
        PlayerBattleManager.instance.ActivatePlayerBattleController(true);
        PlayerBattleManager.instance.ShowDeplacement();
        gameObject.SetActive(false);
    }

    protected override bool OnInteract(RuntimeBattleCharacter interactedCharacter)
    {
        anim.Play("UseObject");

        foreach (int r in indexs)
        {
            BattleManager.instance.OpenRoom(r);
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        PlayerBattleManager.instance.ActivatePlayerBattleController(false);

        return true;
    }
}
