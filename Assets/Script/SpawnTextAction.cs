using System;
using Unity.Behavior;
using UnityEngine;
using TMPro;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Spawn Random Text (Internal)", story: "Spawn random internal text above agent", category: "Action", id: "spawn_random_text_internal")]
public partial class SpawnTextAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> TextPrefab;
    [SerializeReference] public BlackboardVariable<Vector3> Offset = new BlackboardVariable<Vector3>(new Vector3(0, 2f, 0));

    protected override Status OnStart()
    {
        string[] textOptions = new string[] 
        { 
            "!", 
            "Sus",
            "67",
            "Hi Plum",
        };

        if (GameObject == null || TextPrefab.Value == null)
        {
            return Status.Failure;
        }

        string selectedText = textOptions[UnityEngine.Random.Range(0, textOptions.Length)];

        Vector3 spawnPos = GameObject.transform.position + Offset.Value;

        GameObject newTextObj = UnityEngine.Object.Instantiate(TextPrefab.Value, spawnPos, Quaternion.identity);

        TMP_Text tmp = newTextObj.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = selectedText;
        }
        else
        {
            TextMeshPro tmPro = newTextObj.GetComponent<TextMeshPro>();
            if (tmPro != null) tmPro.text = selectedText;
        }

        return Status.Success;
    }
}