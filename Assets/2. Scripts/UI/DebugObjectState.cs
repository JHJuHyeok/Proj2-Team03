using UnityEngine;

public class DebugObjectState : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[Skill BTN Bar] Awake", this);
    }

    private void OnEnable()
    {
        Debug.Log("[Skill BTN Bar] OnEnable", this);
    }

    private void Start()
    {
        Debug.Log("[Skill BTN Bar] Start", this);
    }

    private void OnDisable()
    {
        Debug.LogWarning("[Skill BTN Bar] OnDisable", this);
    }

    private void OnDestroy()
    {
        Debug.LogError("[Skill BTN Bar] OnDestroy", this);
    }
}