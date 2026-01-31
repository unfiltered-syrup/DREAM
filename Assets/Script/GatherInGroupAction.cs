using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Host Group Gathering", story: "Host calls nearby agents", category: "Action", id: "a7b8c9d0-1234-5678-9101-112131415161")]
public partial class GatherInGroupAction : Action
{
    [Header("Gathering Settings")]
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(15f);
    [SerializeReference] public BlackboardVariable<int> MaxGuests = new BlackboardVariable<int>(3);
    [SerializeReference] public BlackboardVariable<float> StopDistance = new BlackboardVariable<float>(2.0f);

    [Header("Shaking Settings")]
    [SerializeReference] public BlackboardVariable<bool> EnableShake = new BlackboardVariable<bool>(true);
    [SerializeReference] public BlackboardVariable<float> ShakeStrength = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> ShakeSpeed = new BlackboardVariable<float>(5f);

    [Header("Duration Settings")]
    [SerializeReference] public BlackboardVariable<float> MinDuration = new BlackboardVariable<float>(2f);
    [SerializeReference] public BlackboardVariable<float> MaxDuration = new BlackboardVariable<float>(8f);

    [SerializeReference] private List<NavMeshAgent> _guests = new List<NavMeshAgent>();
    [SerializeReference] private Dictionary<Transform, Quaternion> _baseRotations = new Dictionary<Transform, Quaternion>();
    
    [NonSerialized] private float _randomSeed;
    [NonSerialized] private bool _isActive;
    [NonSerialized] private float _endTime; 

    [SerializeReference] public BlackboardVariable<bool> PlayerInRange = new BlackboardVariable<bool>(false);

    protected override Status OnStart()
    {
        if (GameObject == null) return Status.Failure;

        float randomDuration = UnityEngine.Random.Range(MinDuration.Value, MaxDuration.Value);
        _endTime = Time.time + randomDuration;

        if (_guests == null) _guests = new List<NavMeshAgent>();
        _guests.Clear();
        
        if (_baseRotations == null) _baseRotations = new Dictionary<Transform, Quaternion>();
        _baseRotations.Clear();

        _randomSeed = UnityEngine.Random.Range(0f, 100f);
        _isActive = true;

        // Save Host rotation
        _baseRotations[GameObject.transform] = GameObject.transform.localRotation;

        FindGuests();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.time >= _endTime)
        {
            return Status.Success;
        }

        if (PlayerInRange) return Status.Failure;
        if (!_isActive || GameObject == null) return Status.Failure;

        // Host Shake
        if (_baseRotations.ContainsKey(GameObject.transform))
        {
            ApplyShake(GameObject.transform, _baseRotations[GameObject.transform], _randomSeed);
        }

        for (int i = _guests.Count - 1; i >= 0; i--)
        {
            NavMeshAgent guest = _guests[i];

            if (guest == null || guest.gameObject == null)
            {
                _guests.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(guest.transform.position, GameObject.transform.position);

            if (dist > StopDistance.Value)
            {
                if (guest.isOnNavMesh)
                {
                    guest.isStopped = false;
                    guest.SetDestination(GameObject.transform.position);
                    _baseRotations[guest.transform] = guest.transform.localRotation;
                }
            }
            else
            {
                if (guest.isOnNavMesh) guest.isStopped = true;

                // Look at Host
                Vector3 lookDir = (GameObject.transform.position - guest.transform.position).normalized;
                lookDir.y = 0;
                
                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    if (!_baseRotations.ContainsKey(guest.transform)) 
                        _baseRotations[guest.transform] = guest.transform.localRotation;

                    _baseRotations[guest.transform] = Quaternion.Slerp(_baseRotations[guest.transform], targetRot, Time.deltaTime * 5f);
                }

                // Shake Guest
                ApplyShake(guest.transform, _baseRotations[guest.transform], _randomSeed + (i + 1) * 50f);
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_guests != null)
        {
            foreach (var guest in _guests)
            {
                if (guest != null && guest.isOnNavMesh)
                {
                    guest.isStopped = false;
                    guest.ResetPath();
                    
                    // Reset to flat rotation
                    Vector3 euler = guest.transform.localEulerAngles;
                    guest.transform.localRotation = Quaternion.Euler(0, euler.y, 0);
                }
            }
            _guests.Clear();
        }
        
        // Reset Host
        if (GameObject != null)
        {
            Vector3 euler = GameObject.transform.localEulerAngles;
            GameObject.transform.localRotation = Quaternion.Euler(0, euler.y, 0);
        }

        _isActive = false;
    }

    private void FindGuests()
    {
        Collider[] hits = Physics.OverlapSphere(GameObject.transform.position, SearchRadius.Value);
        var candidates = new List<NavMeshAgent>();

        foreach (var hit in hits)
        {
            if (hit.gameObject == GameObject) continue;
            if (!hit.CompareTag("NPC") && !hit.CompareTag("NPC")) continue; 

            if (hit.TryGetComponent(out NavMeshAgent agent))
            {
                candidates.Add(agent);
            }
        }

        // Safe Sort
        candidates.Sort((a, b) => {
            float distA = (a.transform.position - GameObject.transform.position).sqrMagnitude;
            float distB = (b.transform.position - GameObject.transform.position).sqrMagnitude;
            return distA.CompareTo(distB);
        });

        int count = Mathf.Min(candidates.Count, MaxGuests.Value);
        for (int i = 0; i < count; i++)
        {
            NavMeshAgent g = candidates[i];
            _guests.Add(g);
            _baseRotations[g.transform] = g.transform.localRotation;
        }
    }

    private void ApplyShake(Transform t, Quaternion baseRot, float seed)
    {
        if (!EnableShake.Value) return;

        float time = Time.time * ShakeSpeed.Value + seed;
        float x = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;
        float z = (Mathf.PerlinNoise(time, time) - 0.5f) * 2f;

        Quaternion wobble = Quaternion.Euler(
            x * ShakeStrength.Value, 
            y * ShakeStrength.Value, 
            z * ShakeStrength.Value
        );

        t.localRotation = baseRot * wobble;
    }
}