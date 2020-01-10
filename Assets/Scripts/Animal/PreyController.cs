using System.Collections;
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

public class PreyController : AnimalController<PreyStates>
{
	private new void Start()
	{
		base.Start();
		initialState = PreyStates.Search;
	}
	
	#region StateMachine

	void Search_Enter()
	{
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
				if (fov.visiblePreys.Count > 0)
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
			gotoTarget = target;		
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
				break;
		}
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

	void FleeCheck()
	{
		fleeTarget = getElementIfExists(fov.visiblePredators, 0);
		if (fleeTarget != null)
		{
			currentState = PreyStates.Flee;
		}
	}

	private Transform fleeTarget;
	void Flee_Enter()
	{
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

	void Flee_Tick()
	{
		if (fov.visiblePredators.Contains(fleeTarget))
		{
			agent.velocity = (transform.position - fleeTarget.position).normalized * agent.speed;
		}
		else
		{
			fleeTarget = null;
			currentState = PreyStates.Search;
		}
	}

	void Flee_Exit()
	{
		
	}
	
	#endregion
}
