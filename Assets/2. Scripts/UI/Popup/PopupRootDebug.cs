using UnityEngine;

public class PopupRootDebug : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[PopupRoot] Awake: " + gameObject.scene.name, this);
    }

    private void OnEnable()
    {
        Debug.Log("[PopupRoot] OnEnable", this);
    }

    private void Start()
    {
        Debug.Log("[PopupRoot] Start", this);
    }

    private void OnDisable()
    {
        Debug.LogWarning("[PopupRoot] OnDisable", this);
    }

    private void OnDestroy()
    {
        Debug.LogError("[PopupRoot] OnDestroy", this);
    }
}