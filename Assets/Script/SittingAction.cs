using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Chair and Sit (Smart)", story: "Find [Chair], checking availability during transit", category: "Action", id: "find_chair_sit_smart")]
public partial class SittingAction : Action
{
    [Header("Settings")]
    [SerializeReference] public BlackboardVariable<string> ChairTag = new BlackboardVariable<string>("Chair");
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(20f);
    
    [Header("Timing")]
    [Tooltip("How long to stay seated.")]
    [SerializeReference] public BlackboardVariable<float> SittingDuration = new BlackboardVariable<float>(15.0f);
    
    [Header("Animation")]
    [Tooltip("The EXACT name of the Bool parameter in your Animator Controller.")]
    [SerializeReference] public BlackboardVariable<string> SitParamName = new BlackboardVariable<string>("IsSitting");

    private NavMeshAgent _agent;
    private Animator _animator;
    private Rigidbody _rb; 
    private Transform _targetChair;
    private ChairScript _targetScript;
    
    private bool _hasArrived = false;
    private bool _isSitting = false;
    private float _timer = 0f;
    private int _sitParamHash;

    protected override Status OnStart()
    {
        if (GameObject == null) return Status.Failure;

        _agent = GameObject.GetComponent<NavMeshAgent>();
        _animator = GameObject.GetComponent<Animator>();
        _rb = GameObject.GetComponent<Rigidbody>(); 

        if (_agent == null || _animator == null)
        {
            Debug.LogWarning($"SittingAction: Object '{GameObject.name}' is missing NavMeshAgent or Animator!");
            return Status.Failure;
        }

        bool paramFound = false;
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.name == SitParamName.Value)
            {
                paramFound = true;
                if (param.type != AnimatorControllerParameterType.Bool)
                {
                    Debug.LogError($"[SittingAction] Parameter '{SitParamName.Value}' exists but is NOT a Bool!");
                    return Status.Failure;
                }
                break;
            }
        }

        if (!paramFound)
        {
            Debug.LogError($"[SittingAction] Parameter '{SitParamName.Value}' NOT found on Animator!");
            return Status.Failure;
        }

        _sitParamHash = Animator.StringToHash(SitParamName.Value);

        _targetChair = FindNearestChair();
        
        if (_targetChair == null)
        {
            return Status.Failure; 
        }

        _targetScript = _targetChair.GetComponent<ChairScript>();

        _agent.isStopped = false;
        _agent.updatePosition = true; 
        _agent.SetDestination(_targetChair.position);
        
        if (_rb != null) _rb.isKinematic = false;
        _hasArrived = false;
        _isSitting = false;
        _timer = 0f;
        _animator.SetBool(_sitParamHash, false);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_targetChair == null) return Status.Failure;

        if (_isSitting)
        {
            _timer += Time.deltaTime;

            if (_timer >= SittingDuration.Value)
            {
                _animator.SetBool(_sitParamHash, false);
                return Status.Success; 
            }
            return Status.Running;
        }

        if (!_hasArrived)
        {
            if (!_agent.pathPending)
            {
                if (_targetScript != null && _targetScript.Occupied)
                {
                    if (_agent.remainingDistance > 1.0f)
                    {
                        Debug.Log($"[SittingAction] Chair occupied while traveling, Aborting.");
                        _agent.ResetPath();
                        return Status.Failure;
                    }
                }

                if (_agent.remainingDistance <= 0.5f)
                {
                    _hasArrived = true;
                    _agent.isStopped = true;
                }
            }
            return Status.Running;
        }

        if (_hasArrived)
        {
            Quaternion targetRotation = _targetChair.rotation;
            GameObject.transform.rotation = Quaternion.Slerp(GameObject.transform.rotation, targetRotation, Time.deltaTime * 5f);

            if (Quaternion.Angle(GameObject.transform.rotation, targetRotation) < 5f)
            {
                if (_rb != null) _rb.isKinematic = true; 
                _agent.isStopped = true;
                _agent.updatePosition = false; 
                
                _animator.SetBool(_sitParamHash, true);
                _isSitting = true;
                _timer = 0f;
                return Status.Running;
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_animator != null) _animator.SetBool(_sitParamHash, false);

        if (_agent != null)
        {
            _agent.updatePosition = true;
            _agent.isStopped = false;
        }

        if (_rb != null) _rb.isKinematic = false; 
    }

    private Transform FindNearestChair()
    {
        GameObject[] chairs = GameObject.FindGameObjectsWithTag(ChairTag.Value);
        Transform bestTarget = null;
        float closestSqrDist = SearchRadius.Value * SearchRadius.Value;
        Vector3 currentPos = GameObject.transform.position;

        foreach (GameObject chair in chairs)
        {
            if (chair.TryGetComponent(out ChairScript script))
            {
                if (script.Occupied) continue; 
            }

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