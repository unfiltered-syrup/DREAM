using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Walk To Random Point", story: "Agent walks to reachable random point", category: "Action", id: "walk_random_smart")]
public partial class WalkToRandomPointAction : Action
{
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> MoveSpeed = new BlackboardVariable<float>(3.5f);
    [SerializeReference] public BlackboardVariable<GameObject> AgentObject = new BlackboardVariable<GameObject>();
    [SerializeReference] public BlackboardVariable<bool> PlayerInRange = new BlackboardVariable<bool>(false);

    private NavMeshAgent _agent;
    private Animator _animator;
    private int _speedHash;
    
    // Cache the path object to avoid garbage collection allocation every frame
    private NavMeshPath _pathCheck; 

    protected override Status OnStart()
    {
        if (AgentObject.Value == null)
        {
            _agent = GameObject.GetComponent<NavMeshAgent>();
            _animator = GameObject.GetComponent<Animator>();
        }
        else
        {
            _agent = AgentObject.Value.GetComponent<NavMeshAgent>();
            _animator = AgentObject.Value.GetComponent<Animator>();
        }

        if (_agent == null) return Status.Failure;

        if (_animator != null)
        {
            _speedHash = Animator.StringToHash("Speed");
        }

        _agent.speed = MoveSpeed.Value;
        _agent.avoidancePriority = 99; 
        _pathCheck = new NavMeshPath();

        for (int i = 0; i < 10; i++)
        {
            Vector3 candidatePoint = GetRandomNavMeshPoint(GameObject.transform.position, SearchRadius.Value);

            if (candidatePoint == Vector3.positiveInfinity) 
                continue;


            _agent.CalculatePath(candidatePoint, _pathCheck);
            if (_pathCheck.status != NavMeshPathStatus.PathComplete) 
                continue; 
            if (IsCrowded(candidatePoint))
                continue; 

            _agent.SetDestination(candidatePoint);
            return Status.Running;
        }
        
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (PlayerInRange) return Status.Failure;
        if (_agent == null) return Status.Failure;

        if (_animator != null)
        {
            _animator.SetFloat(_speedHash, _agent.velocity.magnitude, 0.1f, Time.deltaTime);
        }

        if (_agent.pathPending) return Status.Running;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (_animator != null) _animator.SetFloat(_speedHash, 0f);
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.ResetPath();
            _agent.avoidancePriority = 50;
        }
        
        if (_animator != null) _animator.SetFloat(_speedHash, 0f);
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 center, float range)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return Vector3.positiveInfinity;
    }

    private bool IsCrowded(Vector3 targetPos)
    {
        // Check for NPCs within 3 units of the target
        Collider[] hits = Physics.OverlapSphere(targetPos, 3.0f);
        int npcCount = 0;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("NPC"))
            {
                npcCount++;
            }
        }

        return npcCount > 1; 
    }
}