using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LouvreDoor : MonoBehaviour, IInteractable
{
    [SerializeField] string SceneName;

    [Header("UI Settings")]
    [SerializeField] GameObject loadingPanel;

    private bool isLoading;

    public string TargetSceneName => SceneName;

    public string GetDescription()
    {
        return string.IsNullOrWhiteSpace(SceneName)
            ? "Hedef sahne tanimli degil."
            : $"{SceneName} sahnesine gec.";
    }

    public string GetTitle()
    {
        return "Muzeye Gir";
    }

    public void Interact()
    {
        if (isLoading) return;

        if (string.IsNullOrWhiteSpace(SceneName))
        {
            Debug.LogError($"[{nameof(LouvreDoor)}] SceneName bos. Obje: {gameObject.name}");
            return;
        }

        if (!IsSceneInBuildSettings(SceneName))
        {
            Debug.LogError($"[{nameof(LouvreDoor)}] '{SceneName}' Build Settings/active scene list icinde yok.");
            return;
        }

        StartCoroutine(LoadSceneCoroutine());
    }

    public void LoadTargetScene()
    {
        Interact();
    }

    private IEnumerator LoadSceneCoroutine()
    {
        isLoading = true;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneName);
        if (operation == null)
        {
            Debug.LogError($"[{nameof(LouvreDoor)}] '{SceneName}' yuklenemedi. Sahne adini ve Build Settings listesini kontrol et.");
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
            isLoading = false;
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }
    }

    private static bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}
