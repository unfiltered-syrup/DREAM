using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Host Group Gathering (Body Shake + Head Squash)", story: "Host calls agents and gathers around", category: "Action", id: "host_group_gathering_combo")]
public partial class GatherInGroupAction : Action
{
    [Header("Gathering Settings")]
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(15f);
    [SerializeReference] public BlackboardVariable<int> MaxGuests = new BlackboardVariable<int>(3);
    [SerializeReference] public BlackboardVariable<float> StopDistance = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<float> FaceAngleThreshold = new BlackboardVariable<float>(10f);

    [Header("Filtering Logic")]
    [SerializeReference] public BlackboardVariable<string> SitParamName = new BlackboardVariable<string>("IsSitting");
    [SerializeReference] public BlackboardVariable<string> LastActionVarName = new BlackboardVariable<string>("LastAction");
    [SerializeReference] public BlackboardVariable<string> SitStringValue = new BlackboardVariable<string>("Sit");
    [SerializeReference] public BlackboardVariable<string> IsGatheringVarName = new BlackboardVariable<string>("IsGathering");
    [SerializeReference] public BlackboardVariable<string> PlayerInRangeVarName = new BlackboardVariable<string>("PlayerInRange");

    [Header("Head Squash")]
    [SerializeReference] public BlackboardVariable<bool> EnableTalking = new BlackboardVariable<bool>(true);
    [SerializeReference] public BlackboardVariable<string> HeadBoneName = new BlackboardVariable<string>("DEF-head");
    [SerializeReference] public BlackboardVariable<float> TalkStretchAmount = new BlackboardVariable<float>(0.1f);
    [SerializeReference] public BlackboardVariable<float> TalkSpeed = new BlackboardVariable<float>(18f);

    [Header("Body Shake")]
    [SerializeReference] public BlackboardVariable<bool> EnableShake = new BlackboardVariable<bool>(true);
    [Tooltip("Degrees of rotation for the body shake.")]
    [SerializeReference] public BlackboardVariable<float> ShakeStrength = new BlackboardVariable<float>(2.0f); 
    [SerializeReference] public BlackboardVariable<float> ShakeSpeed = new BlackboardVariable<float>(5f);

    [Header("Duration Settings")]
    [SerializeReference] public BlackboardVariable<float> MinDuration = new BlackboardVariable<float>(2f);
    [SerializeReference] public BlackboardVariable<float> MaxDuration = new BlackboardVariable<float>(18f);

    [SerializeReference] private List<NavMeshAgent> _guests = new List<NavMeshAgent>();
    
    [SerializeReference] private Dictionary<Transform, Quaternion> _bodyBaseRotations = new Dictionary<Transform, Quaternion>();

    [SerializeReference] private Dictionary<Transform, Transform> _agentToHeadMap = new Dictionary<Transform, Transform>();
    [SerializeReference] private Dictionary<Transform, Vector3> _headBaseScales = new Dictionary<Transform, Vector3>();
    
    [NonSerialized] private float _randomSeed;
    [NonSerialized] private bool _isActive;
    [NonSerialized] private float _endTime; 
    [NonSerialized] private bool _hostStartedTalking = false; 

    [SerializeReference] public BlackboardVariable<bool> PlayerInRange = new BlackboardVariable<bool>(false);

    protected override Status OnStart()
    {
        if (GameObject == null) return Status.Failure;

        float randomDuration = UnityEngine.Random.Range(MinDuration.Value, MaxDuration.Value);
        _endTime = Time.time + randomDuration;

        // Initialize Lists
        if (_guests == null) _guests = new List<NavMeshAgent>();
        _guests.Clear();
        
        if (_bodyBaseRotations == null) _bodyBaseRotations = new Dictionary<Transform, Quaternion>();
        _bodyBaseRotations.Clear();

        if (_agentToHeadMap == null) _agentToHeadMap = new Dictionary<Transform, Transform>();
        _agentToHeadMap.Clear();

        if (_headBaseScales == null) _headBaseScales = new Dictionary<Transform, Vector3>();
        _headBaseScales.Clear();

        _randomSeed = UnityEngine.Random.Range(0f, 100f);
        _isActive = true;
        _hostStartedTalking = false; 

        _bodyBaseRotations[GameObject.transform] = GameObject.transform.localRotation;

        Transform hostHead = FindDeepChild(GameObject.transform, HeadBoneName.Value);
        hostHead.localScale = Vector3.one;
        if (hostHead != null)
        {
            _agentToHeadMap[GameObject.transform] = hostHead;
            _headBaseScales[hostHead] = hostHead.localScale;
        }

        FindGuests();

        if (_guests.Count == 0)
        {
            _isActive = false;
            return Status.Failure; 
        }

        SetGuestsGatheringState(true);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.time >= _endTime) return Status.Success;
        if (PlayerInRange) return Status.Failure;
        if (!_isActive || GameObject == null) return Status.Failure;

        if (!_hostStartedTalking && _guests.Count > 0)
        {
            foreach (var guest in _guests)
            {
                if (guest == null) continue;
                if (Vector3.Distance(guest.transform.position, GameObject.transform.position) < 3.0f)
                {
                    _hostStartedTalking = true;
                    break;
                }
            }
        }

        if (_hostStartedTalking)
        {
            if (_bodyBaseRotations.ContainsKey(GameObject.transform))
                ApplyBodyShake(GameObject.transform, _bodyBaseRotations[GameObject.transform], _randomSeed);

            if (_agentToHeadMap.ContainsKey(GameObject.transform))
            {
                Transform head = _agentToHeadMap[GameObject.transform];
                ApplyHeadSquash(head, _headBaseScales[head], _randomSeed);
            }
        }

        for (int i = _guests.Count - 1; i >= 0; i--)
        {
            NavMeshAgent guest = _guests[i];
            if (guest == null || guest.gameObject == null) { _guests.RemoveAt(i); continue; }

            float dist = Vector3.Distance(guest.transform.position, GameObject.transform.position);

            if (dist > StopDistance.Value)
            {
                // MOVING
                if (guest.isOnNavMesh)
                {
                    Vector3 dirToHost = (GameObject.transform.position - guest.transform.position).normalized;
                    dirToHost.y = 0; 
                    float angleToHost = Vector3.Angle(guest.transform.forward, dirToHost);

                    if (angleToHost > FaceAngleThreshold.Value)
                    {
                        guest.isStopped = true;
                        guest.velocity = Vector3.zero; 
                        if (dirToHost != Vector3.zero)
                        {
                            Quaternion targetRot = Quaternion.LookRotation(dirToHost);
                            guest.transform.rotation = Quaternion.Slerp(guest.transform.rotation, targetRot, Time.deltaTime * 5f);
                        }
                    }
                    else
                    {
                        guest.isStopped = false;
                        guest.SetDestination(GameObject.transform.position);
                    }
                    
                    _bodyBaseRotations[guest.transform] = guest.transform.localRotation;
                }
            }
            else
            {
                // ARRIVED
                if (guest.isOnNavMesh) guest.isStopped = true;

                // Face da host
                Vector3 lookDir = (GameObject.transform.position - guest.transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    // Smooth turn
                    Quaternion newRot = Quaternion.Slerp(guest.transform.rotation, targetRot, Time.deltaTime * 5f);
                    guest.transform.rotation = newRot;
                    
                    _bodyBaseRotations[guest.transform] = guest.transform.localRotation;
                }

                // Shake Body
                ApplyBodyShake(guest.transform, _bodyBaseRotations[guest.transform], _randomSeed + (i + 1) * 50f);

                //  Squash Head
                if (_agentToHeadMap.ContainsKey(guest.transform))
                {
                    Transform head = _agentToHeadMap[guest.transform];
                    ApplyHeadSquash(head, _headBaseScales[head], _randomSeed + (i + 1) * 50f);
                }
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        SetGuestsGatheringState(false);

        // Reset
        if (_guests != null)
        {
            foreach (var guest in _guests)
            {
                if (guest != null && guest.isOnNavMesh)
                {
                    guest.isStopped = false;
                    guest.ResetPath();
                    guest.avoidancePriority = 50; 
                    
                    // Reset Body Rotation (Flatten)
                    Vector3 euler = guest.transform.localEulerAngles;
                    guest.transform.localRotation = Quaternion.Euler(0, euler.y, 0);
                }
            }
            _guests.Clear();
        }


        if (GameObject != null)
        {
            Vector3 euler = GameObject.transform.localEulerAngles;
            GameObject.transform.localRotation = Quaternion.Euler(0, euler.y, 0);
        }

        foreach (var kvp in _agentToHeadMap)
        {
            Transform head = kvp.Value;
            if (head != null && _headBaseScales.ContainsKey(head))
            {
                head.localScale = _headBaseScales[head];
            }
        }

        _agentToHeadMap.Clear();
        _headBaseScales.Clear();
        _bodyBaseRotations.Clear();
        _isActive = false;
    }


    private void ApplyBodyShake(Transform t, Quaternion baseRot, float seed)
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

    private void ApplyHeadSquash(Transform head, Vector3 baseScale, float seed)
    {
        if (!EnableTalking.Value) return;

        float noise = Mathf.PerlinNoise(Time.time * TalkSpeed.Value, seed);
        float stretch = noise * TalkStretchAmount.Value;

        // Y stretches up
        float yScale = baseScale.y * (1f + stretch);
        
        // X/Z squash down
        float xzFactor = 1f - (stretch * 0.5f);
        float xScale = baseScale.x * xzFactor;
        float zScale = baseScale.z * xzFactor;

        head.localScale = new Vector3(xScale, yScale, zScale);
    }

    private void SetGuestsGatheringState(bool state)
    {
        if (_guests == null) return;
        foreach (var guest in _guests)
        {
            if (guest == null) continue;
            if (guest.TryGetComponent(out BehaviorGraphAgent bgAgent))
            {
                if (bgAgent.BlackboardReference != null)
                {
                    if (bgAgent.BlackboardReference.GetVariable(IsGatheringVarName.Value, out var variable))
                    {
                        if (variable is BlackboardVariable<bool> boolVar) boolVar.Value = state;
                    }
                }
            }
        }
    }

    private void FindGuests()
    {
        Collider[] hits = Physics.OverlapSphere(GameObject.transform.position, SearchRadius.Value);
        var candidates = new List<NavMeshAgent>();

        foreach (var hit in hits)
        {
            if (hit.gameObject == GameObject) continue;
            if (!hit.CompareTag("NPC")) continue; 
            if (!hit.TryGetComponent(out NavMeshAgent agent)) continue;

            bool isSittingAnim = false;
            if (hit.TryGetComponent(out Animator anim))
            {
                foreach (var param in anim.parameters)
                {
                    if (param.name == SitParamName.Value && param.type == AnimatorControllerParameterType.Bool)
                    {
                        if (anim.GetBool(SitParamName.Value)) isSittingAnim = true;
                        break;
                    }
                }
            }
            if (isSittingAnim) continue;

            bool shouldSkip = false;
            if (hit.TryGetComponent(out BehaviorGraphAgent behaviorAgent))
            {
                if (behaviorAgent.BlackboardReference != null)
                {
                    if (behaviorAgent.BlackboardReference.GetVariable(LastActionVarName.Value, out var laVar))
                        if (laVar is BlackboardVariable<string> strVar && strVar.Value == SitStringValue.Value) shouldSkip = true;

                    if (behaviorAgent.BlackboardReference.GetVariable(IsGatheringVarName.Value, out var igVar))
                         if (igVar is BlackboardVariable<bool> boolVar && boolVar.Value == true) shouldSkip = true;

                    if (behaviorAgent.BlackboardReference.GetVariable(PlayerInRangeVarName.Value, out var playerVar))
                         if (playerVar is BlackboardVariable<bool> boolVar && boolVar.Value == true) shouldSkip = true;
                }
            }
            if (shouldSkip) continue;

            candidates.Add(agent);
        }

        candidates.Sort((a, b) => {
            float distA = (a.transform.position - GameObject.transform.position).sqrMagnitude;
            float distB = (b.transform.position - GameObject.transform.position).sqrMagnitude;
            return distA.CompareTo(distB);
        });

        int count = Mathf.Min(candidates.Count, MaxGuests.Value);
        for (int i = 0; i < count; i++)
        {
            NavMeshAgent g = candidates[i];
            
            g.avoidancePriority = 20; 
            _guests.Add(g);

            _bodyBaseRotations[g.transform] = g.transform.localRotation;

            Transform head = FindDeepChild(g.transform, HeadBoneName.Value);
            if (head != null)
            {
                _agentToHeadMap[g.transform] = head;
                _headBaseScales[head] = head.localScale;
            }
        }
    }

    private Transform FindDeepChild(Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null) return result;
        foreach (Transform child in aParent)
        {
            result = FindDeepChild(child, aName);
            if (result != null) return result;
        }
        return null;
    }
}