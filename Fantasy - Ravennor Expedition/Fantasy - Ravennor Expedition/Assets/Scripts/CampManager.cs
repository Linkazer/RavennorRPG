using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampManager : MonoBehaviour
{
    [SerializeField]
    private List<Image> characterSprites;

    [SerializeField]
    private Color nonSelectedColor;

    public void GoToNextScene()
    {
        RavenorGameManager.instance.LoadBattle();
    }

    public void OpenCharacterSheet(int index)
    {
        characterSprites[index].color = Color.white;
        for (int i = 0; i < characterSprites.Count; i++)
        {
            if(i!=index)
            {
                characterSprites[i].color = nonSelectedColor;
            }
        }
    }

    public void OpenCharacterSkills(int index)
    {

    }

    public void ClosePannel()
    {
        for (int i = 0; i < characterSprites.Count; i++)
        {
            characterSprites[i].color = Color.white;
        }
    }

    public void OpenInventory()
    {

    }
}
