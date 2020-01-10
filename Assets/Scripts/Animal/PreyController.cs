using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Prime31.StateKitLite;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum PreyStates
{
	Search,
	Goto,
	DoAction,
	Flee
}

public class PreyController : StateKitLite<PreyStates>
{
	public float moveSpeed;

	public FieldOfView fov;
	public Camera cam;
	public NavMeshAgent agent;
	
	public Rigidbody myRigidbody;
	protected Camera viewCamera;
	protected Vector3 velocity;

	[Header("Refs:")] 
	[SerializeField] private UtilitySystem utilitySystem;

	protected void Start () {
		fov = GetComponent<FieldOfView>();
		myRigidbody = GetComponent<Rigidbody>();
		agent = GetComponent<NavMeshAgent>();
		viewCamera = Camera.main;

		initialState = PreyStates.Search;
	}

	public Vector3 RandomNavmeshLocation(float radius) {
		Vector3 randomDirection = Random.insideUnitSphere * radius;
		randomDirection += transform.position;
		NavMeshHit hit;
		Vector3 finalPosition = Vector3.zero;
		if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
			finalPosition = hit.position;            
		}
		return finalPosition;
	}

	private Vector3 GetMeanVector(System.Collections.Generic.List<Vector3> positions){
		if (positions.Count == 0)
			return Vector3.zero;
		
		float x = 0f;
		float y = 0f;
		float z = 0f;

		foreach (Vector3 pos in positions)
		{
			x += pos.x;
			y += pos.y;
			z += pos.z;
		}
		Vector3 meanVector = new Vector3(x / positions.Count, y / positions.Count, z / positions.Count);
		return meanVector;
	}

	private Vector3 GetOppositeVector(Vector3 position){
		return Vector3.zero - position;
	}

	protected void Update () {
		base.Update();
		// goToPreyFood
		///////////////////////////////////
		// if (fov.visiblePreyFoods.Count==0){
		// 	agent.SetDestination(RandomNavmeshLocation(fov.viewRadius));
		// }
		// else {
		// 		agent.SetDestination(fov.visiblePreyFoods[0].position);
        // }

		// fleePredator
		///////////////////////////////////////////
		// https://answers.unity.com/questions/1187267/average-position-of-a-set-of-gameobjects.html
		
		// if (fov.visiblePredators.Count==0){
		// 	agent.SetDestination(RandomNavmeshLocation(fov.viewRadius));
		// }
		// else {
		// 	System.Collections.Generic.List<Vector3> positions = new System.Collections.Generic.List<Vector3>();
		// 	for (int i = 0; i < fov.visiblePredators.Count; i++) {
		// 		positions.Add (fov.visiblePredators[i].position);
		// 	}
		// 	agent.SetDestination(GetOppositeVector((GetMeanVector(positions))));
        // }

		// die
		//////////////////////////////////
		// if (fov.visiblePredators.Count==0){
		// 	agent.SetDestination(RandomNavmeshLocation(fov.viewRadius));
		// }
		// else {
		// 	for (int i = 0; i < fov.visiblePredators.Count; i++) {
		// 		//if a predator is at less than 1.5 of distance (equivalent of touch the prey)
		// 		if(Vector3.Distance(fov.visiblePredators[i].position, transform.position) <= 1.5){
		// 			Destroy(gameObject);
		// 		}
		// 	}
        // }
	
	}

	protected void FixedUpdate() {
		myRigidbody.MovePosition (myRigidbody.position + velocity * Time.fixedDeltaTime);
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
	[SerializeField] private UrgeFloatDict urgeDistanceDict = new UrgeFloatDict()
	{
		{ Urge.Hunger, 2},
		{ Urge.Thirst, 5},
		{ Urge.Mating, 2},
	};
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
			currentState = PreyStates.Search;
			Search_Enter();
		});
	}

	[SerializeField] private float randomMoveDirectionTime = 5;

	IEnumerator MoveRandomly()
	{
		while (currentState == PreyStates.Search)
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
					currentState = PreyStates.Goto;
				}
				break;
			case Urge.Thirst:
				if (fov.visibleWaterPoints.Count > 0)
				{
					currentState = PreyStates.Goto;
				}
				break;
			case Urge.Mating:
				if (fov.visiblePredators.Count > 0)
				{
					currentState = PreyStates.Goto;
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
			.DelayedCall(gotoTimeout, () => currentState = PreyStates.Search);
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
				target = getElementIfExists(fov.visiblePreys, 0);
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
			currentState = PreyStates.Search;
			return;
		}

		if (Vector3.Distance(transform.position, target.position) < urgeDistanceDict[currentUrge])
		{
			currentState = PreyStates.DoAction;
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
			currentState = PreyStates.Search; 
		});
	}
	void DoAction_Tick() {}

	void DoAction_Exit()
	{
		utilitySystem.ResetUrge(currentUrge);
	}
	
	void Flee_Enter()
	{
		
	}

	void Flee_Tick()
	{
		
	}

	void Flee_Exit()
	{
		
	}
	
	#endregion
}
