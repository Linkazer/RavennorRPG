using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Vector3 startPos, currentPos;
    private Vector3 startWorldPos;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float lerpCoef;

    public bool followChara;
    [SerializeField]
    private Transform toFollow;

    private void Start()
    {
        BattleManager.TurnBeginEvent.AddListener(OnNewTurn);
    }

    // Update is called once per frame
    void Update()
    {
        if(followChara && toFollow != null && Vector2.Distance(toFollow.position, transform.position) > 0)
        {
            //Vector3.Lerp(transform.position, new Vector3(toFollow.position.x, toFollow.position.y, -10), lerpCoef)
            if (Vector2.Distance(toFollow.position, transform.position) < lerpCoef)
            {
                SetCameraPosition(toFollow.position);
            }
            else
            {
                SetCameraPosition(transform.position + (new Vector3(toFollow.position.x, toFollow.position.y, -10) - transform.position).normalized * lerpCoef);
            }
        }

        if(Input.GetMouseButtonDown(2))
        {
            followChara = false;
            startPos = Input.mousePosition;
            startWorldPos = transform.position;
        }
        else if(Input.GetMouseButton(2))
        {
            Vector3 wantedPos = (startPos - Input.mousePosition)*speed*0.00005f;

            if (BattleManager.instance.CanCameraGoNextDestination(wantedPos + startWorldPos))
            {
                SetCameraPosition(wantedPos + startWorldPos);
            }
            else
            {
                startPos = Input.mousePosition;
                startWorldPos = transform.position;
            }
        }
    }

    public void SetCameraPosition(Vector2 newPos)
    {

        transform.position = new Vector3(newPos.x, newPos.y, -10);

    }

    public void OnNewTurn()
    {
        SetNextChara(BattleManager.instance.GetCurrentTurnChara().transform);
    }

    public void SetNextChara(Transform newToFollow)
    {
        toFollow = newToFollow;
        followChara = true;
    }
}
