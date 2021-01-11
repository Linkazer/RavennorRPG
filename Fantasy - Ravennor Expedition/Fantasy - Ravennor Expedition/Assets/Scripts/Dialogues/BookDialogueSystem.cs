using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookDialogueSystem : MonoBehaviour
{
    public void TestReponse(int index)
    {
        switch(index)
        {
            case 0:
                Debug.Log("Yes");
                break;
            case 1:
                Debug.Log("No");
                break;
        }
    }
}
