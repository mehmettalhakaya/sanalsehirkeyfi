using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuLoader : MonoBehaviour
{
    const string MuseumSceneName = "Outside_Museum_VR";

    [SerializeField] GameObject loadingPanel;

    public void StartMuseum()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(0.2f);

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(MuseumSceneName);

        if (operation == null)
        {
            Debug.LogError($"Scene '{MuseumSceneName}' could not be loaded. Check Build Settings.");
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
