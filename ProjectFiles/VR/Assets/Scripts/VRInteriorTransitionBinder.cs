using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class VRInteriorTransitionBinder
{
    private const string OutsideVrSceneName = "Outside_Museum_VR";
    private static readonly HashSet<int> BoundDoors = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        BindScene(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindScene(scene);
    }

    static void BindScene(Scene scene)
    {
        if (scene.name != OutsideVrSceneName) return;

        LouvreDoor[] doors = Object.FindObjectsByType<LouvreDoor>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (LouvreDoor door in doors)
        {
            BindDoor(door);
        }
    }

    static void BindDoor(LouvreDoor door)
    {
        if (door == null || string.IsNullOrWhiteSpace(door.TargetSceneName)) return;

        int id = door.GetInstanceID();
        if (BoundDoors.Contains(id)) return;

        Collider collider = door.GetComponent<Collider>();
        if (collider == null)
        {
            collider = door.GetComponentInChildren<Collider>();
        }

        if (collider == null)
        {
            Debug.LogWarning($"[{nameof(VRInteriorTransitionBinder)}] '{door.name}' uzerinde collider yok; XR secimi baglanamadi.");
            return;
        }

        XRSimpleInteractable interactable = door.GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = door.gameObject.AddComponent<XRSimpleInteractable>();
        }

        if (!interactable.colliders.Contains(collider))
        {
            interactable.colliders.Add(collider);
        }

        interactable.selectEntered.AddListener(_ => door.Interact());
        BoundDoors.Add(id);

        Debug.Log($"[{nameof(VRInteriorTransitionBinder)}] '{door.name}' XR Select -> '{door.TargetSceneName}' baglandi.");
    }
}
