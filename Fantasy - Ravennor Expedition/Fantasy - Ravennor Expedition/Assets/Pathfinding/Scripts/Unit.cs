using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {


	public Transform target;
	float speed = 1;
	Vector3[] path;
	int targetIndex;

	void Start() {
        if (target != null)
        {
            //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        }
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}
	Vector2 posUnit, posTarget, direction;
	IEnumerator FollowPath() {
		Vector3 currentWaypoint = path[0];
		while (true) {
			posUnit = new Vector2(transform.position.x,transform.position.y);
			posTarget = new Vector2(currentWaypoint.x, currentWaypoint.y);
			if (Vector2.Distance(posUnit,posTarget)<(0.05f*speed)) {
				targetIndex ++;
				if (targetIndex >= path.Length) {
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}
			Debug.Log(speed);
			direction = new Vector2(currentWaypoint.x - transform.position.x, currentWaypoint.y - transform.position.y).normalized;

			transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;
			//	Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;

		}
	}

	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one*0.1f);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}
}
