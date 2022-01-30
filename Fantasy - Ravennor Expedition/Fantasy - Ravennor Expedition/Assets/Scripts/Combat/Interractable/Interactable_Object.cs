using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Object : RVN_InteractableObject
{
    [SerializeField] private Animator anim;

    protected override bool OnInteract(RuntimeBattleCharacter interactedCharacter)
    {
        anim.Play("UseObject");

        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        PlayerBattleManager.instance.ActivatePlayerBattleController(false);

        return true;
    }

    public void Disable()
    {
        PlayerBattleManager.instance.ActivatePlayerBattleController(true);
        gameObject.SetActive(false);
    }
}
