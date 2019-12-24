using UnityEngine;
using UnityEngine.AI;


public class PreyController : MonoBehaviour {

	public float moveSpeed;

	public FieldOfView fov;
	public Camera cam;
	public NavMeshAgent agent;
	
	public Rigidbody myRigidbody;
	protected Camera viewCamera;
	protected Vector3 velocity;

	protected void Start () {
		fov = GetComponent<FieldOfView>();
		myRigidbody = GetComponent<Rigidbody>();
		agent = GetComponent<NavMeshAgent>();
		viewCamera = Camera.main;
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
}
