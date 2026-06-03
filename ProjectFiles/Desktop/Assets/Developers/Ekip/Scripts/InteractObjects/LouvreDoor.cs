using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Coroutine kullanabilmek için bu kütüphane şart
public class LouvreDoor : MonoBehaviour, IInteractable
{
    [SerializeField] string SceneName;

    [Header("UI Settings")]
    [SerializeField] GameObject loadingPanel; // Hazırladığın loading panelini buraya sürükleyeceksin

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
        // Etkileşime girildiğinde Coroutine'i başlatıyoruz
        StartCoroutine(LoadSceneCoroutine());
    }
    private IEnumerator LoadSceneCoroutine()
    {
        // 1. Loading Panelini aktif et
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        // 2. Sahneyi arka planda (asenkron) yüklemeye başla
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneName);
        // 3. Yükleme işlemi bitene kadar bekle
        while (!operation.isDone)
        {
            // Eğer ilerleme çubuğu (progress bar) eklemek istersen, 
            // operation.progress değerini (0 ile 1 arasıdır) kullanabilirsin.

            yield return null; // Bir sonraki frame'e geç
        }
    }
}