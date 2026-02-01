using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detect Contact & Eye Contact", story: "Check contact with [Player] and increment timer", category: "Action", id: "detect_eye_contact_simple")]
public partial class EyeContactAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> PlayerInRange = new BlackboardVariable<bool>(false);
    [SerializeReference] public BlackboardVariable<GameObject> Player = new BlackboardVariable<GameObject>();
    [SerializeReference] public BlackboardVariable<Transform> NPCHead = new BlackboardVariable<Transform>();
    [SerializeReference] public BlackboardVariable<float> ContactDistance = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<float> TurnSpeed = new BlackboardVariable<float>(5.0f);
    [SerializeReference] public BlackboardVariable<bool> ShowDebug = new BlackboardVariable<bool>(true);

    private PlayerLogic _playerLogic;
    private float _sqrThreshold;
    private int _combinedLayerMask;

    protected override Status OnStart()
    {
        if (GameObject == null || Player.Value == null || NPCHead.Value == null)
        {
            return Status.Failure;
        }

        _playerLogic = Player.Value.GetComponent<PlayerLogic>();

        _sqrThreshold = ContactDistance.Value * ContactDistance.Value;

        int blockerMask = LayerMask.GetMask("Blocker");
        int defaultMask = LayerMask.GetMask("Default");
        _combinedLayerMask = blockerMask | defaultMask;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!PlayerInRange.Value) return Status.Failure;
        if (Player.Value == null) return Status.Failure;

        float sqrDist = (GameObject.transform.position - Player.Value.transform.position).sqrMagnitude;
        if (sqrDist > _sqrThreshold)
        {
            return Status.Running; 
        }

        Vector3 lookDirection = Player.Value.transform.position - GameObject.transform.position;
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            GameObject.transform.rotation = Quaternion.Slerp(
                GameObject.transform.rotation, 
                targetRotation, 
                Time.deltaTime * TurnSpeed.Value
            );
        }

        Transform playerHead = _playerLogic.HeadTransform;
        
        Vector3 startPos = NPCHead.Value.position;
        Vector3 direction = playerHead.position - startPos;
        float distance = direction.magnitude;

        RaycastHit hitInfo;
        
        bool isBlocked = Physics.Raycast(startPos, direction, out hitInfo, distance, _combinedLayerMask);

        if (!isBlocked)
        {
            _playerLogic.AddEyeContact(Time.deltaTime);
            if (ShowDebug.Value) Debug.DrawRay(startPos, direction, Color.green);
            return Status.Running;
        }
        else
        {
            if (ShowDebug.Value) Debug.DrawLine(startPos, hitInfo.point, Color.red);
            return Status.Running;
        }
    }
}