using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

 public enum PreyStates
 {
 	Search,
 	Goto,
 	DoAction,
 	Flee
 }

public class RabbitController : AnimalController<PreyStates>
{
	// NEW //////////////////
	private Animator rabbitAnimator;

	private new void Start()
	{
		base.Start();
		rabbitAnimator = GetComponent<Animator>();
		initialState = PreyStates.Search;
	}
	
	//TODO can generalize
	public Transform GetMatingPartner(List<Transform> list)
	{
		//Returns only other animal of opposite gender that wants to mate
		var target = list.FirstOrDefault(t =>
		{
			if (t == null) return false;

			var component = t.GetComponent<RabbitController>();
			return 
				component != null &&
				component.male != this.male && 
				component.GetCurrentUrge() == Urge.Mating;
		});

		return target;
	}

	public void Die()
	{
		base.Die();
		
		//Death animation
		rabbitAnimator.SetTrigger("dead");
	}

	#region StateMachine
	
	void Search_Enter()
	{
		//animation
		rabbitAnimator.SetBool("walking", true);

		currentUrge = utilitySystem.GetUrgeWithHighestVal();

		agent.isStopped = false;
		//DEBUG
		string text = "Searching for: " + currentUrge;
		if (Selection.Contains(gameObject))
		{
			Debug.Log(text);
		}
		debugUi.UpdateStateText(text);
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
		FleeCheck();

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
				if (GetMatingPartner(fov.visiblePreys) != null)
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
		string text = "Going for: " + currentUrge;
		if (Selection.Contains(gameObject))
		{
			Debug.Log(text);
		}
		debugUi.UpdateStateText(text);
		//=====
		
		//TIMEOUT
		gotoTimeoutTween = DOVirtual
			.DelayedCall(gotoTimeout, () => currentState = PreyStates.Search);
	}
	
	void Goto_Tick()
	{
		FleeCheck();
		
		Transform target = null;
		switch (currentUrge)
		{
			case Urge.Hunger:
				target = GetElementIfExists(fov.visiblePreyFoods, 0);
				break;
			
			case Urge.Mating:
				target = GetMatingPartner(fov.visiblePreys);
				break;
			
			case Urge.Thirst:
				target = GetElementIfExists(fov.visibleWaterPoints, 0);
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
			gotoTarget = target;		
			currentState = PreyStates.DoAction;
		}
	}

	void Goto_Exit()
	{
		gotoTimeoutTween.Kill();
		agent.isStopped = true;
		agent.velocity = Vector3.zero;
	}

	void DoAction_Enter()
	{
		//stop moving animation
		rabbitAnimator.SetBool("walking", false);

		//DEBUG
		string text = "Performing: " + currentUrge;
		if (Selection.Contains(gameObject))
		{
			Debug.Log(text);
		}
		debugUi.UpdateStateText(text);
		//=====
		
		//TODO perform action here (animate etc)
		switch (currentUrge)
		{
			case Urge.Hunger:
				gotoTarget.GetComponent<Plant>().Die();
				DOVirtual.DelayedCall(3, () =>
				{
					currentState = PreyStates.Search; 
				});
				break;
			case Urge.Mating:
				//TIME OUT SEARCH FOR NEW URGE
				DOVirtual
					.DelayedCall(3, () => { currentState = PreyStates.Search; })
					.OnComplete(() =>
					{
						spawnChildAnimal();
					});
				break;
			default:
				//TIME OUT SEARCH FOR NEW URGE
				DOVirtual.DelayedCall(3, () => { currentState = PreyStates.Search; });
				break;
		}
	}
	void DoAction_Tick() {}

	void DoAction_Exit()
	{
		utilitySystem.ResetUrge(currentUrge);
	}

	void FleeCheck()
	{
		fleeTarget = GetElementIfExists(fov.visiblePredators, 0);
		if (fleeTarget != null)
		{
			currentState = PreyStates.Flee;
		}
	}

	private Transform fleeTarget;
	void Flee_Enter()
	{
		currentFleeTime = 0;
		//DEBUG
		//DEBUG
		string text = "Performing: Flee ";
		if (Selection.Contains(gameObject))
		{
			Debug.Log(text);
		}
		debugUi.UpdateStateText(text);
		//=====
		//=====
	}

	[SerializeField] private float minFleeTime = 3;
	private float currentFleeTime = 0;
	private Vector3 lastFleeTargetPosition;
	void Flee_Tick()
	{
		currentFleeTime += Time.deltaTime;
		
		if (fov.visiblePredators.Contains(fleeTarget) && fleeTarget != null)
		{
			lastFleeTargetPosition = fleeTarget.position; 
			agent.velocity = (transform.position - fleeTarget.position).normalized * agent.speed;
		}
		else
		{
			if (currentFleeTime < minFleeTime)
			{
				agent.velocity = (transform.position - lastFleeTargetPosition).normalized * agent.speed;
			}
			else
			{
				fleeTarget = null;
				currentState = PreyStates.Search;	
			}
		}
	}

	void Flee_Exit()
	{
		
	}
	
	#endregion
	
	private void spawnChildAnimal()
	{
		if(male) return;

		var father = gotoTarget.GetComponent<RabbitController>();
		var childGenotype = Genetics.MultipleCrossover(father.genotype, genotype);
		childGenotype = Genetics.Mutation(childGenotype);
		
		var child = Instantiate(childPrefab, transform.position + new Vector3(1, 0, 1), Quaternion.identity);
		child.tag = "Bunny";
		child.transform.localScale = Vector3.zero;
		child.transform.DOScale(childPrefab.transform.localScale, 1);
		
		var childController = child.GetComponent<RabbitController>();
		childController.SetNewGenotype(childGenotype);
		childController.DecodeGenotype();

	}
}
