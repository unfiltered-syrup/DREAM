using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SetAnimatorSpeed : MonoBehaviour
{
    [Header("Settings")]
    public string speedParameter = "Speed";
    public float animationSmoothTime = 0.1f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private int _speedHash;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _speedHash = Animator.StringToHash(speedParameter);
    }

    void Update()
    {
        float currentSpeed = _agent.velocity.magnitude;
        Debug.Log("Curspeed" + currentSpeed);
        _animator.SetFloat(_speedHash, currentSpeed, animationSmoothTime, Time.deltaTime);
    }
}