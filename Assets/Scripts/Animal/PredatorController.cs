using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Prime31.StateKitLite;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum PredatorStates
{
	Search,
	Goto,
	DoAction
}

public class PredatorController : StateKitLite<PredatorStates> {

	public float moveSpeed;

	public FieldOfView fov;
	public Camera cam;
	public NavMeshAgent agent;
	
	public Rigidbody myRigidbody;
	protected Camera viewCamera;
	protected Vector3 velocity;

	[Header("References:")] 
	[SerializeField] private UtilitySystem utilitySystem;

	protected void Start () {
		fov = GetComponent<FieldOfView>();
		myRigidbody = GetComponent<Rigidbody>();
		agent = GetComponent<NavMeshAgent>();
		viewCamera = Camera.main;

		initialState = PredatorStates.Search;
	}

	public Vector3 RandomNavmeshLocation(float radius) 
	{
		Vector3 randomDirection = Random.insideUnitSphere * radius;
		randomDirection += transform.position;
		NavMeshHit hit;
		Vector3 finalPosition = Vector3.zero;
		if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
			finalPosition = hit.position;            
		}
		return finalPosition;
	}

	protected void FixedUpdate() {
		myRigidbody.MovePosition (myRigidbody.position + moveSpeed * Time.fixedDeltaTime * velocity.normalized);
	}

	public Transform getElementIfExists(List<Transform> list, int index)
	{
		if (list.Count >= index + 1)
		{
			return list[index];
		}
		else
		{
			return null;
		}
	}

	#region StateMachine

	[Header("State machine:")] 
	[SerializeField] private float gotoDistance;
	[SerializeField] private float gotoTimeout = 8;
	[SerializeField] private float searchTimeout = 5;
	private Urge currentUrge;
	private Coroutine wanderCoroutine;
	private Tween searchTimeoutTween;
	private Tween gotoTimeoutTween;

	void Search_Enter()
	{
		currentUrge = utilitySystem.GetUrgeWithHighestVal();

		agent.isStopped = false;
		//DEBUG
		if (Selection.Contains(gameObject))
		{
			Debug.Log("Searching for: " + currentUrge);
		}
		//=====
		
		//Start random wander
		wanderCoroutine = StartCoroutine(MoveRandomly());
		
		//TIMEOUT - search for another urge if its greater than current
		searchTimeoutTween = DOVirtual.DelayedCall(searchTimeout, () =>
		{
			currentState = PredatorStates.Search;
			Search_Enter();
		});
	}

	[SerializeField] private float randomMoveDirectionTime = 5;

	IEnumerator MoveRandomly()
	{
		while (currentState == PredatorStates.Search)
		{
			agent.SetDestination(RandomNavmeshLocation(fov.viewRadius));
			yield return new WaitForSeconds(randomMoveDirectionTime);
		}
	}
	
	void Search_Tick()
	{
		switch (currentUrge)
		{
			case Urge.Hunger:
				if (fov.visiblePreyFoods.Count > 0)
				{
					currentState = PredatorStates.Goto;
				}
				break;
			case Urge.Thirst:
				if (fov.visibleWaterPoints.Count > 0)
				{
					currentState = PredatorStates.Goto;
				}
				break;
			case Urge.Mating:
				if (fov.visiblePredators.Count > 0)
				{
					currentState = PredatorStates.Goto;
				}
				break;
		}
	}

	void Search_Exit()
	{
		searchTimeoutTween.Kill();
		StopCoroutine(wanderCoroutine);
	}

	void Goto_Enter()
	{
		//DEBUG
		if (Selection.Contains(gameObject))
		{
			Debug.Log("Going for: " + currentUrge);
		}
		//=====
		
		//TIMEOUT
		gotoTimeoutTween = DOVirtual
			.DelayedCall(gotoTimeout, () => currentState = PredatorStates.Search);
	}

	
	void Goto_Tick()
	{
		Transform target = null;
		switch (currentUrge)
		{
			case Urge.Hunger:
				target = getElementIfExists(fov.visiblePreyFoods, 0);
				break;
			
			case Urge.Mating:
				//TODO - should check if another wolf wants to mate!!!
				target = getElementIfExists(fov.visiblePredators, 0);
				break;
			
			case Urge.Thirst:
				target = getElementIfExists(fov.visibleWaterPoints, 0);
				break;
		}

		//no target found go to search again
		if (target != null)
		{
			agent.SetDestination(target.position);
		}
		else
		{
			currentState = PredatorStates.Search;
			return;
		}

		if (Vector3.Distance(transform.position, target.position) < gotoDistance)
		{
			currentState = PredatorStates.DoAction;
		}
	}

	void Goto_Exit()
	{
		gotoTimeoutTween.Kill();
		agent.isStopped = true;
	}

	void DoAction_Enter()
	{
		//DEBUG
		if (Selection.Contains(gameObject))
		{
			Debug.Log("Performing: " + currentUrge);
		}
		//=====

		//TODO perform action here (animate etc)
		
		//
		
		//TIME OUT SEARCH FOR NEW URGE
		DOVirtual.DelayedCall(3, () =>
		{
			currentState = PredatorStates.Search; 
		});
	}
	void DoAction_Tick() {}

	void DoAction_Exit()
	{
		utilitySystem.ResetUrge(currentUrge);
	}
	
	#endregion
}
