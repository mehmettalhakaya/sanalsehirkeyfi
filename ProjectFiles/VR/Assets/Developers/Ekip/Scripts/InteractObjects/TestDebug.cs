using UnityEngine;

public class TestDebug : MonoBehaviour, IInteractable
{
    public string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public string GetTitle()
    {
        throw new System.NotImplementedException();
    }

    public void Interact()
    {
        Debug.Log(Random.Range(0, 100));
    }
}