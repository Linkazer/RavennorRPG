using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimationManager : MonoBehaviour
{
    public static BattleAnimationManager instance;

    [SerializeField] private SpellObject prefab;
    [SerializeField] private Transform prefabParent;
    [SerializeField] private int prefabCount;

    public List<SpellObject> spellsObjects = new List<SpellObject>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        for(int i = 0; i < prefabCount; i++)
        {
            spellsObjects.Add(Instantiate(prefab.gameObject, prefabParent).GetComponent<SpellObject>());
        }
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

    public void PlayOnNode(List<Vector2> position, Sprite spriteToPut, Sprite caseSprite, float timeToShow, AudioClip soundToPlay)
    {
        List<SpellObject> toShow = new List<SpellObject>();
        foreach(Vector2 pos in position)
        {
            toShow.Add(GetSpellsObject());
            toShow[toShow.Count-1].SetObject(pos);
            toShow[toShow.Count - 1].SetSprite(spriteToPut, caseSprite, 5);
        }

        StartCoroutine(SpriteShowed(toShow, timeToShow, soundToPlay));
        if (timeToShow >= 0)
        {
            BattleManager.instance.EndCurrentActionWithDelay(timeToShow);
        }
    }

    public void PlayOnNode(Vector2 position, Sprite spriteToPut, Sprite caseSprite, float timeToShow, AudioClip soundToPlay)
    {
        List<SpellObject> toShow = new List<SpellObject>();
        toShow.Add(GetSpellsObject());
        toShow[toShow.Count - 1].SetObject(position);
        toShow[toShow.Count - 1].SetSprite(spriteToPut, caseSprite, 1);

        StartCoroutine(SpriteShowed(toShow, timeToShow, soundToPlay));
        if (timeToShow >= 0)
        {
            BattleManager.instance.EndCurrentActionWithDelay(timeToShow);
        }
    }

    public void PlayProjectile(Vector2 startPos, Vector2 endPos, Sprite projectileSprite, float speed)
    {
        SpellObject toShow = GetSpellsObject();
        toShow.SetObject(startPos);
        toShow.SetSprite(projectileSprite, null, 10);
        toShow.SetMovableObject(endPos, speed, ()=> BattleManager.instance.DoCurrentAction(endPos));
    }

    public void AddZoneEffect(Vector2 position, Sprite zoneSprite, RuntimeBattleCharacter caster, int turnNeeded, RuntimeSpellEffect newEffet)
    {
        List<SpellObject> toShow = new List<SpellObject>();
        toShow.Add(GetSpellsObject());
        toShow[toShow.Count - 1].SetObject(position);
        toShow[toShow.Count - 1].SetSprite(zoneSprite, null, -999);
        toShow[toShow.Count - 1].SetCaster(caster, turnNeeded, newEffet);
    }

    IEnumerator SpriteShowed(List<SpellObject> usedObj, float time, AudioClip soundToPlay)
    {
        usedObj[0].SetSound(soundToPlay);

        if(time < 0)
        {
            time = 0.5f;
        }
        yield return new WaitForSeconds(time);
        foreach(SpellObject obj in usedObj)
        {
            obj.ResetObject();
        }
    }

}
