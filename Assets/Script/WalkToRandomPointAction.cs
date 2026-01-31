using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI; // Required for NavMesh
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Walk To Random Point", story: "Agent walks to random point", category: "Action", id: "18563d8e164a7802bd67c18bd5cb2aac")]
public partial class WalkToRandomPointAction : Action
{
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> MoveSpeed = new BlackboardVariable<float>(3.5f);
    [SerializeReference] public BlackboardVariable<GameObject> AgentObject = new BlackboardVariable<GameObject>();
    [SerializeReference] public BlackboardVariable<bool> PlayerInRange = new BlackboardVariable<bool>(false);

    private NavMeshAgent _agent;

    protected override Status OnStart()
    {

        _agent = AgentObject.Value.GetComponent<NavMeshAgent>();
        _agent.speed = MoveSpeed.Value;

        Vector3 randomPoint = GetRandomPoint(GameObject.transform.position, SearchRadius.Value);
        
        if (randomPoint != Vector3.positiveInfinity)
        {
            _agent.SetDestination(randomPoint);
            return Status.Running;
        }
        else
        {
            return Status.Failure;
        }
    }

    protected override Status OnUpdate()
    {
        if(PlayerInRange){return Status.Failure;}

        if (_agent == null) return Status.Failure;

        if (_agent.pathPending)
        {
            return Status.Running;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            return Status.Success;
        }

        return Status.Running;
    }


    private Vector3 GetRandomPoint(Vector3 center, float range)
    {
        for (int i = 0; i < 30; i++)
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
}