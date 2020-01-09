using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FieldOfView : MonoBehaviour {

	public float viewRadius;
	[Range(0,360)]
	public float viewAngle;
    //the angle in unity begin at the north with 0

	//public LayerMask targetMask;
	//public LayerMask obstacleMask;

	public LayerMask preyMask;
	public LayerMask predatorMask;
	public LayerMask preyFoodMask;
	public LayerMask treeMask;
	public LayerMask waterMask;


	// List of visible targets at each time
	[HideInInspector]

	//public List<Transform> visibleTargets = new List<Transform>();

	public List<Transform> visiblePreys = new List<Transform>();
	public List<Transform> visiblePredators = new List<Transform>();
	public List<Transform> visiblePreyFoods = new List<Transform>();
	public List<Transform> visibleWaterPoints = new List<Transform>();

	void Start() {
		StartCoroutine ("FindTargetsWithDelay", .2f);
	}

	// Delay specifies the number of seconds we need to wait before uptading the visible target list
	IEnumerator FindTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVisibleTargets ();
		}
	}

	void FindVisibleTargets() {
		//visibleTargets.Clear ();

		visiblePreys.Clear();
	    visiblePredators.Clear();
		visiblePreyFoods.Clear();

		//Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

		Collider[] preysInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, preyMask);
		Collider[] predatorsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, predatorMask);
		Collider[] preyFoodsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, preyFoodMask);
		Collider[] waterInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, waterMask);


		// for (int i = 0; i < targetsInViewRadius.Length; i++) {
		// 	Transform target = targetsInViewRadius [i].transform;
		// 	Vector3 dirToTarget = (target.position - transform.position).normalized;
		// 	// the following line check if we have a target in our field of view
		// 	if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) {
		// 		float dstToTarget = Vector3.Distance (transform.position, target.position);
		// 		// the following line check if we have no obstacle between the target and us
		// 		if (!Physics.Raycast (transform.position, dirToTarget, dstToTarget, TreesMask)) {
		// 			visibleTargets.Add (target);
		// 		}
		// 	}
		// }
	

		for (int i = 0; i < preysInViewRadius.Length; i++) {
			Transform target = preysInViewRadius[i].transform;
			if(target == transform) continue;
			Vector3 dirToTarget = (target.position - transform.position).normalized;
			// the following line check if we have a target in our field of view
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
				float dstToTarget = Vector3.Distance(transform.position, target.position);
				// the following line check if we have no obstacle between the target and us
				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, treeMask)) {
					visiblePreys.Add(target);
					visiblePreys = visiblePreys
						.OrderBy(x => Vector3.Distance(transform.position, x.position))
						.ToList();
				}
			}
		}

		for (int i = 0; i < predatorsInViewRadius.Length; i++) {
			Transform target = predatorsInViewRadius[i].transform;
			if(target == transform) continue;
			Vector3 dirToTarget = (target.position - transform.position).normalized;
			// the following line check if we have a target in our field of view
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
				float dstToTarget = Vector3.Distance(transform.position, target.position);
				// the following line check if we have no obstacle between the target and us
				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, treeMask)) {
					visiblePredators.Add(target);
					visiblePredators = visiblePredators
						.OrderBy(x => Vector3.Distance(transform.position, x.position))
						.ToList();
				}
			}
		}

		for (int i = 0; i < preyFoodsInViewRadius.Length; i++) {
			Transform target = preyFoodsInViewRadius[i].transform;
			if(target == transform) continue;
			Vector3 dirToTarget = (target.position - transform.position).normalized;
			// the following line check if we have a target in our field of view
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
				float dstToTarget = Vector3.Distance(transform.position, target.position);
				// the following line check if we have no obstacle between the target and us
				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, treeMask)) {
					visiblePreyFoods.Add(target);
					visiblePreyFoods = visiblePreyFoods
						.OrderBy(x => Vector3.Distance(transform.position, x.position))
						.ToList();
				}
			}
		}
		
		for (int i = 0; i < waterInViewRadius.Length; i++) {
			Transform target = waterInViewRadius[i].transform;
			if(target == transform) continue;
			Vector3 dirToTarget = (target.position - transform.position).normalized;
			// the following line check if we have a target in our field of view
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
				float dstToTarget = Vector3.Distance(transform.position, target.position);
				// the following line check if we have no obstacle between the target and us
				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, treeMask)) {
					visibleWaterPoints.Add(target);
					visibleWaterPoints = visibleWaterPoints
						.OrderBy(x => Vector3.Distance(transform.position, x.position))
						.ToList();
				}
			}
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
		if (!angleIsGlobal) {
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}
}

