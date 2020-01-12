using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

// public enum PredatorStates
// {
// 	Search,
// 	Goto,
// 	DoAction
// }

public class TestWolfController : AnimalController<PredatorStates> 
{
	// NEW //////////////////
	private Animator wolfAnimator;

	private void Start()
	{
		base.Start();
		wolfAnimator = GetComponent<Animator>();
		initialState = PredatorStates.Search;
	}
	
	//TODO can generalize
	public Transform GetMatingPartner(List<Transform> list)
	{
		//Returns only other animal of opposite gender that wants to mate
		var target = list.FirstOrDefault(t =>
		{
			if (t == null) return false;
			var component = t.GetComponent<PredatorController>();
			return 
				component != null &&
				component.male != this.male &&
				component.GetCurrentUrge() == Urge.Mating;
		});

		return target;
	}

	void Update()
	{
		base.Update();
		wolfAnimator.SetFloat("Vertical", agent.velocity.magnitude);
	}
	
	//HACK
	IEnumerator SetModeOneFrame(string state, int val)
	{
		wolfAnimator.SetInteger(state, val);
		yield return new WaitForSeconds(0.1f);
		wolfAnimator.SetInteger(state, 0);
	}

	#region StateMachine
	
	// NEW //////////////////
	//animation 
	void anim()
	{
		bool walking = false;
		if (currentState == PredatorStates.Goto)
		{
			walking = true;
		}
		if (currentState == PredatorStates.DoAction)
		{
			walking = false;
		}
		if (currentState == PredatorStates.Search)
		{
			walking = true;
		}
		wolfAnimator.SetBool("walking",walking);
	}

	
	void Search_Enter()
	{
		//animation
		wolfAnimator.SetInteger("State", 1);
		//=========
		
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
				if (GetMatingPartner(fov.visiblePredators) != null)
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
				target = GetElementIfExists(fov.visiblePreyFoods, 0);
				break;
			
			case Urge.Mating:
				target = GetMatingPartner(fov.visiblePredators);
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

		//Death animation
		wolfAnimator.SetInteger("State", 10);

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
			case Urge.Thirst:
				//Drinking animation
				StartCoroutine(SetModeOneFrame("Mode", 4007));
				//TIME OUT SEARCH FOR NEW URGE
				DOVirtual.DelayedCall(3, () =>
				{
					currentState = PredatorStates.Search; 
				});
				break;
			case Urge.Hunger:
				gotoTarget.GetComponent<PreyController>().Die();
				
				//DEATH ANIMATION
				StartCoroutine(SetModeOneFrame("Mode", 4001));

				//TIME OUT SEARCH FOR NEW URGE
				DOVirtual.DelayedCall(3, () =>
				{
					currentState = PredatorStates.Search; 
				});
				break;
			case Urge.Mating:
				//Mating animation
				StartCoroutine(SetModeOneFrame("Mode", 4020));
				//TIME OUT SEARCH FOR NEW URGE
				DOVirtual
					.DelayedCall(3, () => { currentState = PredatorStates.Search; })
					.OnComplete(() =>
					{
						if(male) return;
						
						var child = Instantiate(childPrefab, transform.position + new Vector3(1, 0, 1), Quaternion.identity);
						child.tag = "Fox";
						child.transform.localScale = Vector3.zero;
						child.transform.DOScale(childPrefab.transform.localScale, 1);
					});
				break;
			default:
				//TIME OUT SEARCH FOR NEW URGE
				DOVirtual.DelayedCall(3, () =>
				{
					currentState = PredatorStates.Search; 
				});
				break;
		}
		//
	}
	void DoAction_Tick() {}

	void DoAction_Exit()
	{
		utilitySystem.ResetUrge(currentUrge);
	}
	
	#endregion
}

