using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DG.Tweening;
using Prime31.StateKitLite;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AnimalController<TEnum> : StateKitLite<TEnum> where TEnum : struct, IConvertible, IComparable, IFormattable
{
    public Genotype genotype;
    public float moveSpeed;

    public FieldOfView fov;
    public Camera cam;
    public NavMeshAgent agent;

    public Rigidbody myRigidbody;
    protected Camera viewCamera;
    protected Vector3 velocity;

    [Header("Variables")] public bool male;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 1.5f;

    [Header("Refs:")] [SerializeField] protected UtilitySystem utilitySystem;
    [SerializeField] protected DebugUI debugUi;
    [SerializeField] protected GameObject childPrefab;

    protected void Start()
    {
        genotype = new Genotype(Random.Range(-1, Int32.MaxValue));
        
        fov = GetComponent<FieldOfView>();
        myRigidbody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        viewCamera = Camera.main;

        if (GameManager.Instance.deathEnabled)
        {
            utilitySystem.SubscribeOnUrgeExceedLimit((() =>
            {
                if (gameObject != null)
                {
                    Die();
                }
            }));
        }

        agent.speed = Random.Range(minSpeed, maxSpeed);
        male = (Random.value > 0.5f);

        DecodeGenotype();
    }

    protected void DecodeGenotype()
    {
        float minAngle = 120.0f;
        float maxAngle = 360.0f;
        float minRadius = 3.0f;
        float maxRadius = 8.0f;
        var geneDict = Genetics.Decode(genotype);

        fov.viewAngle = geneDict["fovAngle"] / 64.0f * (maxAngle - minAngle) + minAngle;
        fov.viewRadius = geneDict["fovRadius"] / 64.0f * (maxRadius - minRadius) + minRadius;
        agent.speed = geneDict["speed"] / 32.0f * (maxSpeed - minSpeed) + minSpeed;
        male = geneDict["sex"] != 0;
    }

    protected void SetNewGenotype(Genotype g)
    {
        genotype = g;
    }

    public TEnum GetCurrentState()
    {
        return currentState;
    }

    public Urge GetCurrentUrge()
    {
        return currentUrge;
    }

    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        return finalPosition;
    }

    protected Vector3 GetMeanVector(System.Collections.Generic.List<Vector3> positions)
    {
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

    protected Vector3 GetOppositeVector(Vector3 position)
    {
        return Vector3.zero - position;
    }

    public void Die()
    {
        if (!GameManager.Instance.deathEnabled) return;

        agent.isStopped = true;
        transform.DOScale(Vector3.zero, 1).onComplete += () => { Destroy(gameObject); };
    }

//	protected void FixedUpdate() {
//		myRigidbody.MovePosition (myRigidbody.position + velocity * Time.fixedDeltaTime);
//	}

    public Transform GetElementIfExists(List<Transform> list, int index)
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

    [Header("State machine:")] [SerializeField]
    protected float gotoDistance;

    [SerializeField] protected float gotoTimeout = 8;
    [SerializeField] protected float searchTimeout = 5;
    [SerializeField] protected float randomMoveDirectionTime = 5;

    [SerializeField] protected UrgeFloatDict urgeDistanceDict = new UrgeFloatDict()
    {
        {Urge.Hunger, 2},
        {Urge.Thirst, 5},
        {Urge.Mating, 2},
    };

    protected Urge currentUrge;
    protected Coroutine wanderCoroutine;
    protected Tween searchTimeoutTween;
    protected Tween gotoTimeoutTween;
    protected Transform gotoTarget;

    #endregion
}