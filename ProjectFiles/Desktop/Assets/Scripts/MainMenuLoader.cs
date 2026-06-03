using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuLoader : MonoBehaviour
{
    [SerializeField] GameObject loadingPanel;

    public void StartMuseum()
    {
        loadingPanel.SetActive(true);

        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(0.2f);

        AsyncOperation operation =
            SceneManager.LoadSceneAsync("Outside_Museum");

        while (!operation.isDone)
        {
            yield return null;
        }
    }
}