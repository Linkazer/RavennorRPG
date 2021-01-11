using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTest : MonoBehaviour
{
    public GameObject toMove;

    public float speed = 1;
    [HideInInspector]
    public Vector3[] path;
    private Vector3 lastPathPosition;
    int targetIndex = -1;

    Vector2 posUnit, posTarget, direction;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //PathRequestManager.RequestPath(toMove.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), OnPathFound);
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
        else
        {
            Debug.Log(gameObject.name + "No Path");
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];

            while (true)
            {
                posUnit = new Vector2(toMove.transform.position.x, toMove.transform.position.y);
                posTarget = new Vector2(currentWaypoint.x, currentWaypoint.y);
                if (Vector2.Distance(posUnit, posTarget) < (0.01f * speed))
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                    lastPathPosition = currentWaypoint;
                }
                direction = new Vector2(currentWaypoint.x - toMove.transform.position.x, currentWaypoint.y - toMove.transform.position.y).normalized;

                toMove.transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;
                //	Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
                //testFollowPath = false;
                yield return null;
            }
        }

    }

    public void OnDrawGizmos()
    {
        if (path != null && targetIndex >= 0)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one * 0.1f);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
