using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detect Contact & Eye Contact", story: "Check contact with [Player] and increment timer", category: "Action", id: "detect_eye_contact_simple")]
public partial class EyeContactAction : Action
{
    // Prerequisites
    [SerializeReference] public BlackboardVariable<bool> PlayerInRange = new BlackboardVariable<bool>(false);
    [SerializeReference] public BlackboardVariable<GameObject> Player = new BlackboardVariable<GameObject>();
    [SerializeReference] public BlackboardVariable<Transform> NPCHead = new BlackboardVariable<Transform>();

    // Settings
    [SerializeReference] public BlackboardVariable<float> ContactDistance = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<bool> ShowDebug = new BlackboardVariable<bool>(true);

    private PlayerLogic _playerLogic;
    private float _sqrThreshold;
    private int _layerMaskBlocker;
    private int _layerMaskDefault;
    protected override Status OnStart()
    {
        if (GameObject == null || Player.Value == null || NPCHead.Value == null)
        {
            return Status.Failure;
        }

        _playerLogic = Player.Value.GetComponent<PlayerLogic>();
        if (_playerLogic == null)
        {
            if (ShowDebug.Value) Debug.LogWarning("Target Player does not have PlayerLogic script!");
            return Status.Failure;
        }

        _sqrThreshold = ContactDistance.Value * ContactDistance.Value;

        _layerMaskBlocker = LayerMask.GetMask("Blocker");
        _layerMaskDefault = LayerMask.GetMask("Default");


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

        Transform playerHead = _playerLogic.HeadTransform;
        
        Vector3 startPos = NPCHead.Value.position;
        Vector3 direction = playerHead.position - startPos;
        float distance = direction.magnitude;

        RaycastHit hitInfo;
        
        bool isBlocked = Physics.Raycast(startPos, direction, out hitInfo, distance, _layerMaskBlocker) || Physics.Raycast(startPos, direction, out hitInfo, distance, _layerMaskDefault);

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