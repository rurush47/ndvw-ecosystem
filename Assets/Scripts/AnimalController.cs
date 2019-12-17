using UnityEngine;
using UnityEngine.AI;

public class AnimalController : MonoBehaviour {

	public float moveSpeed;

	public FieldOfView fov;
	public Camera cam;
	public NavMeshAgent agent;
	
	protected Rigidbody myRigidbody;
	protected Camera viewCamera;
	protected Vector3 velocity;

	protected void Start () {
		fov = GetComponent<FieldOfView>();
		myRigidbody = GetComponent<Rigidbody> ();
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

	protected void Update () {
		agent.SetDestination(RandomNavmeshLocation(fov.viewRadius));
	}

	protected void FixedUpdate() {
		myRigidbody.MovePosition (myRigidbody.position + velocity * Time.fixedDeltaTime);
	}
}
