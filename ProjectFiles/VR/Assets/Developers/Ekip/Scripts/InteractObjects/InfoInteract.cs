using UnityEngine;

public class InfoInteract : MonoBehaviour, IInteractable
{
    [Header("İçerik")]
    [SerializeField] private string objectTitle = "Nesne Başlığı";
    [SerializeField, TextArea(3, 6)] private string description = "Buraya açıklama yaz...";

    public string GetTitle() => objectTitle;
    public string GetDescription() => description;

    // InfoPanel tarafından çağrılır, değişikliği kaydeder
    public void SetDescription(string newDescription)
    {
        description = newDescription;
    }

    public void Interact()
    {
        // Panel açma işi Interactor üzerinden yapılıyor
        // Buraya ekstra mantık ekleyebilirsin (ses çal, animasyon vb.)
        Debug.Log($"{objectTitle} ile etkileşime girildi.");
    }
}