using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimationManager : MonoBehaviour
{
    public static BattleAnimationManager instance;

    public List<SpellObject> spellsObjects;

    private void Awake()
    {
        instance = this;
    }

    private SpellObject GetSpellsObject()
    {
        foreach(SpellObject obj in spellsObjects)
        {
            if(!obj.isUsed)
            {
                return obj;
            }
        }

        return null;
    }

    public void PlayOnNode(List<Vector2> positiony, AnimationClip anim)
    {

    }

    public void PlayOnNode(Vector2 position, AnimationClip anim)
    {

    }

    public void PlayOnNode(List<Vector2> position, Sprite spriteToPut, Sprite caseSprite, float timeToShow)
    {
        List<SpellObject> toShow = new List<SpellObject>();
        foreach(Vector2 pos in position)
        {
            toShow.Add(GetSpellsObject());
            toShow[toShow.Count-1].SetObject(pos);
            toShow[toShow.Count - 1].SetSprite(spriteToPut, caseSprite, 5);
        }

        StartCoroutine(SpriteShowed(toShow, timeToShow));
        BattleManager.instance.EndCurrentActionWithDelay(timeToShow);
    }

    public void PlayOnNode(Vector2 position, Sprite spriteToPut, Sprite caseSprite, float timeToShow)
    {
        List<SpellObject> toShow = new List<SpellObject>();
        toShow.Add(GetSpellsObject());
        toShow[toShow.Count - 1].SetObject(position);
        toShow[toShow.Count - 1].SetSprite(spriteToPut, caseSprite, 1);

        StartCoroutine(SpriteShowed(toShow, timeToShow));
        BattleManager.instance.EndCurrentActionWithDelay(timeToShow);
    }

    public void PlayProjectile(Vector2 startPos, Vector2 endPos, Sprite projectileSprite, float speed)
    {
        List<SpellObject> toShow = new List<SpellObject>();
        toShow.Add(GetSpellsObject());
        toShow[toShow.Count - 1].SetObject(startPos);
        toShow[toShow.Count - 1].SetSprite(projectileSprite, null, 10);

        StartCoroutine(ProjectileMovement(toShow[0], endPos, speed));
    }

    public void AddZoneEffect(Vector2 position, Sprite zoneSprite, RuntimeBattleCharacter caster, int turnNeeded, SpellEffectCommon newEffet)
    {
        List<SpellObject> toShow = new List<SpellObject>();
        toShow.Add(GetSpellsObject());
        toShow[toShow.Count - 1].SetObject(position);
        toShow[toShow.Count - 1].SetSprite(zoneSprite, null, -999);
        toShow[toShow.Count - 1].SetCaster(caster, turnNeeded, newEffet);
    }

    IEnumerator ProjectileMovement(SpellObject toMove, Vector2 targetPos, float speed)
    {
        Vector3 direction = new Vector3(targetPos.x - toMove.transform.position.x, targetPos.y - toMove.transform.position.y, 0);

        toMove.transform.right = direction;

        while (Vector2.Distance(toMove.transform.position,targetPos)>0.1f)
        {
            toMove.AddPositionMovement(direction * speed * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        BattleManager.instance.DoCurrentAction(targetPos);
        toMove.ResetObject();
    }

    IEnumerator SpriteShowed(List<SpellObject> usedObj, float time)
    {
        yield return new WaitForSeconds(time);
        foreach(SpellObject obj in usedObj)
        {
            obj.ResetObject();
        }
    }

    /*public void PlayAnimationOnNodes(string animName, List<Node> nodesToPlay)
    {
        if (animName != "")
        {
            foreach(Node n in nodesToPlay)
            {
                n.nodeAnim.PlayAnimation(animName);
            }
            BattleManager.instance.EndCurrentActionWithDelay(nodesToPlay[0].nodeAnim.PlayAnimation(animName));
        }
        else
        {
            BattleManager.instance.EndCurrentActionWithDelay(0);
        }
    }

    public void PlayAnimationSingleNode(string animName, Node toPlay)
    {
        if (animName != "")
        {
            toPlay.nodeAnim.PlayAnimation(animName);
            BattleManager.instance.EndCurrentActionWithDelay(toPlay.nodeAnim.PlayAnimation(animName));
        }
        else
        {
            BattleManager.instance.EndCurrentActionWithDelay(0);
        }
    }*/

    //Utiliser un GameObjet pré créé pour les projectiles
    //
}
