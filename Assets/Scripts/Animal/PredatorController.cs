using System.Collections;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public enum PredatorStates
{
	Search,
	Goto,
	DoAction
}

public class PredatorController : AnimalController<PredatorStates> 
{
	private new void Start()
	{
		base.Start();
		initialState = PredatorStates.Search;
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
			currentState = PredatorStates.Search;
			Search_Enter();
		});
	}
	
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
		string text = "Going for: " + currentUrge;
		if (Selection.Contains(gameObject))
		{
			Debug.Log(text);
		}
		debugUi.UpdateStateText(text);
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

		if (Vector3.Distance(transform.position, target.position) < urgeDistanceDict[currentUrge])
		{
			gotoTarget = target;
			currentState = PredatorStates.DoAction;
		}
	}
	
	public void Die()
	{
		if (!GameManager.Instance.deathEnabled) return;

		agent.isStopped = true;
		transform.DOScale(Vector3.zero, 1).onComplete += () =>
		{
			Destroy(gameObject);
		};
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
				gotoTarget.GetComponent<PreyController>().Die();
				break;
		}
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
