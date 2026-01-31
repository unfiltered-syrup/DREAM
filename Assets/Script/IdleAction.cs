using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Idle Sway", story: "Agent idles and sways", category: "Action", id: "cb405762410fad8114db4e9d05eec0ec")]
public partial class IdleAction : Action
{
    [SerializeReference] public BlackboardVariable<float> SwaySpeed = new BlackboardVariable<float>(1f);
    [SerializeReference] public BlackboardVariable<float> SwayAmount = new BlackboardVariable<float>(5f);

    [SerializeReference] public BlackboardVariable<float> IdlingTimeFloor = new BlackboardVariable<float>(2f);

    [SerializeReference] public BlackboardVariable<float> IdlingTimeCeiling = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<bool> PlayerInRange = new BlackboardVariable<bool>(false);

    private Quaternion _startRotation;
    private float _randomOffset;
    private float _timeIdling;

    private float _startTime;

    protected override Status OnStart()
    {
        if (GameObject == null)
        {
            return Status.Failure;
        }

        // Cache the rotation
        _startRotation = GameObject.transform.localRotation;
        
        // Generate ma seed
        _randomOffset = UnityEngine.Random.Range(0f, 100f);

        _timeIdling = UnityEngine.Random.Range(IdlingTimeFloor, IdlingTimeCeiling);
        _startTime = Time.time;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (PlayerInRange) return Status.Failure;

        if (Time.time - _startTime > _timeIdling){return Status.Failure;}

        // Nuts sway with perlin noise
        float time = Time.time * SwaySpeed.Value + _randomOffset;
        float noiseX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
        float noiseY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;

        float xSway = noiseX * SwayAmount.Value;
        float ySway = noiseY * SwayAmount.Value;

        Quaternion swayRotation = Quaternion.Euler(xSway, ySway, 0f);
        GameObject.transform.localRotation = Quaternion.Slerp(GameObject.transform.localRotation, _startRotation * swayRotation, Time.deltaTime * 5f);

        return Status.Running;
    }

}