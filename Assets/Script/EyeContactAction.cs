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
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<Transform> NPCHead;

    // Settings
    [SerializeReference] public BlackboardVariable<float> ContactDistance = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<LayerMask> BlockedLayers = new BlackboardVariable<LayerMask>();

    // Cache
    private PlayerLogic _playerLogic;
    private float _sqrThreshold;

    protected override Status OnStart()
    {
        if (GameObject == null || Player.Value == null || NPCHead.Value == null)
        {
            return Status.Failure;
        }

        _playerLogic = Player.Value.GetComponent<PlayerLogic>();
        if (_playerLogic == null)
        {
            Debug.LogWarning("Target Player does not have PlayerLogic script");
            return Status.Failure;
        }

        _sqrThreshold = ContactDistance.Value * ContactDistance.Value;

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
        Vector3 direction = playerHead.position - NPCHead.Value.position;
        float distance = direction.magnitude;

        if (!Physics.Raycast(NPCHead.Value.position, direction, distance, BlockedLayers.Value))
        {
            _playerLogic.AddEyeContact(Time.deltaTime);
            return Status.Running;
        }

        return Status.Running;
    }
}