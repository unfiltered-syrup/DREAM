using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Chair and Sit", story: "Find nearest [Chair], walk to it and sit", category: "Action", id: "find_chair_and_sit_v2")]
public partial class SittingAction : Action
{
    [Header("Settings")]
    [SerializeReference] public BlackboardVariable<string> ChairTag = new BlackboardVariable<string>("Chair");
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(20f);
    
    [Header("Animation")]
    [Tooltip("The boolean parameter in Animator to trigger sitting.")]
    [SerializeReference] public BlackboardVariable<string> SitParamName = new BlackboardVariable<string>("IsSitting");

    private NavMeshAgent _agent;
    private Animator _animator;
    private Transform _targetChair;
    private bool _hasArrived = false;
    private int _sitParamHash;

    protected override Status OnStart()
    {
        if (GameObject == null) return Status.Failure;

        _agent = GameObject.GetComponent<NavMeshAgent>();
        _animator = GameObject.GetComponent<Animator>();

        if (_agent == null || _animator == null)
        {
            Debug.LogWarning("SittingAction: Missing NavMeshAgent or Animator!");
            return Status.Failure;
        }

        _sitParamHash = Animator.StringToHash(SitParamName.Value);

        
        _targetChair = FindNearestChair();

        if (_targetChair == null)
        {
            return Status.Failure;
        }

        _agent.isStopped = false;
        _agent.SetDestination(_targetChair.position);
        _hasArrived = false;

        _animator.SetBool(_sitParamHash, false);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_targetChair == null) return Status.Failure;

        if (!_hasArrived)
        {
            if (!_agent.pathPending && _agent.remainingDistance <= 0.5f)
            {
                _hasArrived = true;
                _agent.isStopped = true;
            }
            return Status.Running;
        }

        if (_hasArrived)
        {
            Quaternion targetRotation = _targetChair.rotation;
            GameObject.transform.rotation = Quaternion.Slerp(GameObject.transform.rotation, targetRotation, Time.deltaTime * 5f);

            if (Quaternion.Angle(GameObject.transform.rotation, targetRotation) < 5f)
            {
                _animator.SetBool(_sitParamHash, true);

                return Status.Running;
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_animator != null)
        {
            _animator.SetBool(_sitParamHash, false);
        }

        if (_agent != null)
        {
            _agent.isStopped = false;
        }
    }

    private Transform FindNearestChair()
    {
        GameObject[] chairs = GameObject.FindGameObjectsWithTag(ChairTag.Value);
        Transform bestTarget = null;
        float closestSqrDist = SearchRadius.Value * SearchRadius.Value;
        Vector3 currentPos = GameObject.transform.position;

        foreach (GameObject chair in chairs)
        {
            Vector3 direction = chair.transform.position - currentPos;
            float dSqr = direction.sqrMagnitude;
            
            if (dSqr < closestSqrDist)
            {
                closestSqrDist = dSqr;
                bestTarget = chair.transform;
            }
        }
        return bestTarget != null ? bestTarget.transform : null;
    }
}