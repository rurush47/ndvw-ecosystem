using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class PredatorController : MonoBehaviour {

	public float moveSpeed;

	public Camera cam;
	public NavMeshAgent agent;
	
	Rigidbody myRigidbody;
	Camera viewCamera;
	Vector3 velocity;

	void Start () {
		myRigidbody = GetComponent<Rigidbody> ();
		viewCamera = Camera.main;
	}

	void Update () {
//		// Here we can insert the deplacement mode (now it move by mouse clicks)
//		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
//		transform.LookAt (transform.position + velocity);
//		velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;

		if (Input.GetMouseButtonDown(0))
		{
			// Get mouse position as a ray in the game
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			
			// Get where the ray touches the ground
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				// Move agent
				agent.SetDestination(hit.point);
			}
		}
	}

	void FixedUpdate() {
		myRigidbody.MovePosition (myRigidbody.position + velocity * Time.fixedDeltaTime);
	}
}
