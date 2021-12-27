using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Vector2 startPos, currentPos;
    private Vector2 startWorldPos;

    [SerializeField]
    private float mouseSpeed, keySpeed;
    [SerializeField]
    private float lerpCoef;
    private float baselerp = 0;

    public bool followChara;
    [SerializeField]
    private Transform toFollow;
    Vector2 followingPos => toFollow.position;
    Vector2 currentPosition => transform.position;

    private void Start()
    {
        baselerp = lerpCoef;
        BattleManager.characterTurnBegin += OnNewTurn;
    }

    // Update is called once per frame
    void Update()
    {
        if (followChara && toFollow != null && Vector2.Distance(toFollow.position, transform.position) > 0)
        {
            Vector2 lerpedVector = transform.position + (new Vector3(toFollow.position.x, toFollow.position.y, -10) - transform.position).normalized * lerpCoef * Time.deltaTime;

            if (Vector2.Distance(toFollow.position, transform.position) < (lerpCoef * Time.deltaTime))
            {
                lerpCoef = baselerp;
                SetCameraPosition(toFollow.position);
            }
            else
            {
                SetCameraPosition(lerpedVector);
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            followChara = false;
            startPos = Input.mousePosition;
            startWorldPos = transform.position;
        }
        else if (Input.GetMouseButton(2))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 wantedPos = (startPos - mousePos) * mouseSpeed * 0.00005f;

            SetCameraPosition(wantedPos + startWorldPos);
        }
        else
        {
            SetCameraPosition(currentPosition + GetKeyDirection() * keySpeed * Time.deltaTime);
        }
    }

    private Vector2 GetKeyDirection()
    {
        Vector2 toReturn = Vector2.zero;
        toReturn = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        return toReturn.normalized;
    }

    public void SetCameraPosition(Vector2 newPos)
    {
        Vector2 possibleDirection = BattleManager.instance.PossibleCameraDirection(newPos);
        if (possibleDirection.x == 0)
        {
            newPos = new Vector2(transform.position.x, newPos.y);
        }
        if (possibleDirection.y == 0)
        {
            newPos = new Vector2(newPos.x, transform.position.y);
        }
        transform.position = new Vector3(newPos.x, newPos.y, -10);
    }

    public void OnNewTurn(RuntimeBattleCharacter turnChara)
    {
        SetNextChara(BattleManager.instance.GetCurrentTurnChara().transform);
    }

    public void SetNextChara(Transform newToFollow)
    {
        if(toFollow == null)
        {
            SetCameraPosition(RoomManager.GetCamStartPosition());
        }
        lerpCoef = Vector2.Distance(newToFollow.position, transform.position) * baselerp;
        if (lerpCoef < baselerp)
            lerpCoef = baselerp;
        toFollow = newToFollow;
        followChara = true;
    }
}
