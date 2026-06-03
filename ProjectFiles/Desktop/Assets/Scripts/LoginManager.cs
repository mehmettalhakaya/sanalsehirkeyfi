using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Mevcut Menu Butonlari")]
    public Button startButton;
    public Button quitButton;

    [Header("Auth Choice Panel")]
    public GameObject authChoicePanel;
    public TMP_Text authWarningText;
    public Button openLoginButton;
    public Button openRegisterButton;
    public Button authChoiceCloseButton;

    [Header("Login Panel")]
    public GameObject loginPanel;
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginMessageText;
    public Button loginSubmitButton;
    public Button loginBackButton;

    [Header("Register Panel")]
    public GameObject registerPanel;
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerPasswordAgainInput;
    public TMP_Text registerMessageText;
    public Button registerSubmitButton;
    public Button registerBackButton;

    [Header("Ayarlar")]
    public string explorationSceneName = "Outside_Museum";
    public GameObject loadingPanel;
    public string registerUrl = "https://mtkaya.me/api/register";
    public string loginUrl = "https://mtkaya.me/api/login";

    [Tooltip("Mevcut Ayril butonu zaten calisiyorsa kapali kalsin.")]
    public bool overrideQuitButtonClick = false;

    private const string PREF_USERNAME = "AuthUsername";
    private const string PREF_TOKEN = "AuthToken";

    private bool requestRunning;
    private bool loggedInThisSession;
    private bool explorationStarting;

    void Awake()
    {
        SetPanel(authChoicePanel, false);
        SetPanel(loginPanel, false);
        SetPanel(registerPanel, false);

        if (loginPasswordInput != null) loginPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (registerPasswordInput != null) registerPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (registerPasswordAgainInput != null) registerPasswordAgainInput.contentType = TMP_InputField.ContentType.Password;
    }

    void Start()
    {
        loggedInThisSession = false;
        PlayerPrefs.DeleteKey("AuthLoggedIn");

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (quitButton != null && overrideQuitButtonClick)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        WireButton(openLoginButton, ShowLoginPanel);
        WireButton(openRegisterButton, ShowRegisterPanel);
        WireButton(authChoiceCloseButton, CloseAuthPanels);
        WireButton(loginSubmitButton, Login);
        WireButton(loginBackButton, BackToAuthChoice);
        WireButton(registerSubmitButton, Register);
        WireButton(registerBackButton, BackToAuthChoice);

        StartCoroutine(KeepStartButtonOnAuthFlow());
    }

    public void OnStartButtonClicked()
    {
        if (loggedInThisSession)
            StartExploration();
        else
            ShowAuthChoice();
    }

    public void ShowAuthChoice()
    {
        SetMenuButtonsInteractable(false);
        SetMenuButtonsVisible(false);
        SetPanel(loginPanel, false);
        SetPanel(registerPanel, false);
        SetPanel(authChoicePanel, true);
        BringToFront(authChoicePanel);
        SetText(authWarningText, "Keşfe başlamak için önce giriş yapmanız gerekiyor.");
    }

    public void ShowLoginPanel()
    {
        requestRunning = false;
        SetMenuButtonsInteractable(false);
        SetMenuButtonsVisible(false);
        SetPanel(authChoicePanel, false);
        SetPanel(registerPanel, false);
        SetPanel(loginPanel, true);
        BringToFront(loginPanel);
        SetButtonInteractable(loginSubmitButton, true);
        SetMessage(loginMessageText, "", false);
    }

    public void ShowRegisterPanel()
    {
        requestRunning = false;
        SetMenuButtonsInteractable(false);
        SetMenuButtonsVisible(false);
        SetPanel(authChoicePanel, false);
        SetPanel(loginPanel, false);
        SetPanel(registerPanel, true);
        BringToFront(registerPanel);
        SetButtonInteractable(registerSubmitButton, true);
        SetMessage(registerMessageText, "", false);
    }

    public void BackToAuthChoice()
    {
        requestRunning = false;
        SetMenuButtonsInteractable(false);
        SetMenuButtonsVisible(false);
        SetButtonInteractable(loginSubmitButton, true);
        SetButtonInteractable(registerSubmitButton, true);
        SetPanel(loginPanel, false);
        SetPanel(registerPanel, false);
        SetPanel(authChoicePanel, true);
        BringToFront(authChoicePanel);
        SetText(authWarningText, "Keşfe başlamak için önce giriş yapmanız gerekiyor.");
    }

    public void Login()
    {
        if (requestRunning || loginUsernameInput == null || loginPasswordInput == null) return;

        string username = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetMessage(loginMessageText, "Kullanıcı adı/e-posta ve şifre boş olamaz.", true);
            return;
        }

        StartCoroutine(LoginRequest(username, password));
    }

    public void Register()
    {
        if (requestRunning || registerUsernameInput == null ||
            registerPasswordInput == null || registerPasswordAgainInput == null) return;

        string username = registerUsernameInput.text.Trim();
        string email = registerEmailInput != null ? registerEmailInput.text.Trim() : "";
        string password = registerPasswordInput.text;
        string passwordAgain = registerPasswordAgainInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetMessage(registerMessageText, "Kullanıcı adı, e-posta ve şifre boş olamaz.", true);
            return;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            SetMessage(registerMessageText, "Geçerli bir e-posta girin.", true);
            return;
        }

        if (password != passwordAgain)
        {
            SetMessage(registerMessageText, "Şifreler aynı değil.", true);
            return;
        }

        if (password.Length < 8)
        {
            SetMessage(registerMessageText, "Şifre en az 8 karakter olmalı.", true);
            return;
        }

        StartCoroutine(RegisterRequest(username, email, password));
    }

    public void StartExploration()
    {
        explorationStarting = true;
        SetPanel(authChoicePanel, false);
        SetPanel(loginPanel, false);
        SetPanel(registerPanel, false);
        SetMenuButtonsInteractable(false);
        SetMenuButtonsVisible(false);

        if (loadingPanel == null)
            loadingPanel = FindMenuObject("LoadingPanel");

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Ana buton auth sirasinda gizli oldugu icin sahne yuklemeyi aktif LoginManager baslatir.
        StartCoroutine(LoadSceneAsync());
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator LoginRequest(string username, string password)
    {
        requestRunning = true;
        SetButtonInteractable(loginSubmitButton, false);
        SetMessage(loginMessageText, "Giriş yapılıyor...", false);

        AuthResponse response = null;
        string json = JsonUtility.ToJson(new SiteLoginRequest { login = username, password = password });
        yield return PostJson(loginUrl, json, loginMessageText, r => response = r);

        SetButtonInteractable(loginSubmitButton, true);
        requestRunning = false;

        if (response == null) yield break;

        if (!response.success)
        {
            SetMessage(loginMessageText,
                string.IsNullOrEmpty(response.message) ? "Kullanıcı adı veya şifre hatalı." : response.message,
                true);
            yield break;
        }

        string savedUsername = response.user != null && !string.IsNullOrEmpty(response.user.username)
            ? response.user.username
            : username;

        loggedInThisSession = true;
        PlayerPrefs.SetString(PREF_USERNAME, savedUsername);
        if (!string.IsNullOrEmpty(response.token))
            PlayerPrefs.SetString(PREF_TOKEN, response.token);
        PlayerPrefs.Save();

        SetMessage(loginMessageText, string.IsNullOrEmpty(response.message) ? "Giriş başarılı." : response.message, false);
        yield return new WaitForSeconds(0.4f);
        StartExploration();
    }

    IEnumerator KeepStartButtonOnAuthFlow()
    {
        for (int i = 0; i < 8; i++)
        {
            yield return null;
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
        }
    }

    IEnumerator RegisterRequest(string username, string email, string password)
    {
        requestRunning = true;
        SetButtonInteractable(registerSubmitButton, false);
        SetMessage(registerMessageText, "Kayıt yapılıyor...", false);

        AuthResponse response = null;
        string json = JsonUtility.ToJson(new SiteRegisterRequest { username = username, email = email, password = password });
        yield return PostJson(registerUrl, json, registerMessageText, r => response = r);

        SetButtonInteractable(registerSubmitButton, true);
        requestRunning = false;

        if (response == null) yield break;

        if (!response.success)
        {
            SetMessage(registerMessageText,
                string.IsNullOrEmpty(response.message) ? "Kayıt başarısız." : response.message,
                true);
            yield break;
        }

        SetMessage(registerMessageText, "Kayıt başarılı, şimdi giriş yapabilirsiniz.", false);
        yield return new WaitForSeconds(0.8f);

        if (loginUsernameInput != null) loginUsernameInput.text = username;
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        ShowLoginPanel();
        SetMessage(loginMessageText, "Kayıt başarılı, şimdi giriş yapabilirsiniz.", false);
    }

    IEnumerator PostJson(string url, string json, TMP_Text messageText, System.Action<AuthResponse> done)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                SetMessage(messageText, "Bağlantı hatası: " + request.error, true);
                done(null);
                yield break;
            }

            AuthResponse response = ParseResponse(request.downloadHandler.text);
            if (request.result == UnityWebRequest.Result.ProtocolError)
                response.success = false;

            done(response);
        }
    }

    IEnumerator LoadSceneAsync()
    {
        yield return new WaitForSeconds(0.2f);
        AsyncOperation operation = SceneManager.LoadSceneAsync(explorationSceneName);
        while (operation != null && !operation.isDone)
            yield return null;
    }

    AuthResponse ParseResponse(string json)
    {
        try
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(json.Trim());
            response.Normalize();
            return response;
        }
        catch
        {
            return new AuthResponse { success = false, message = "Sunucu cevabı okunamadı." };
        }
    }

    void CloseAuthPanels()
    {
        explorationStarting = false;
        SetPanel(authChoicePanel, false);
        SetPanel(loginPanel, false);
        SetPanel(registerPanel, false);
        SetMenuButtonsVisible(true);
        SetMenuButtonsInteractable(true);
    }

    void BringToFront(GameObject panel)
    {
        if (panel != null)
            panel.transform.SetAsLastSibling();
    }

    void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    void SetText(TMP_Text label, string text)
    {
        if (label != null)
            label.text = text;
    }

    void SetMessage(TMP_Text label, string text, bool isError)
    {
        if (label == null) return;
        label.text = text;
        label.color = isError ? new Color(1f, 0.45f, 0.45f) : new Color(0.55f, 1f, 0.55f);
    }

    void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    void SetMenuButtonsInteractable(bool interactable)
    {
        SetButtonInteractable(startButton, interactable);
        SetButtonInteractable(quitButton, interactable);
    }

    void SetMenuButtonsVisible(bool visible)
    {
        if (startButton != null)
            startButton.gameObject.SetActive(visible);

        if (quitButton != null)
            quitButton.gameObject.SetActive(visible);
    }

    GameObject FindMenuObject(string objectName)
    {
        Transform canvas = null;

        if (startButton != null)
        {
            canvas = startButton.transform;
            while (canvas != null && canvas.GetComponent<Canvas>() == null)
                canvas = canvas.parent;
        }

        if (canvas != null)
        {
            var found = canvas.Find(objectName);
            if (found != null)
                return found.gameObject;
        }

        return GameObject.Find(objectName);
    }

    public bool IsAuthPanelOpen()
    {
        return explorationStarting || IsPanelActive(authChoicePanel) || IsPanelActive(loginPanel) || IsPanelActive(registerPanel);
    }

    bool IsPanelActive(GameObject panel)
    {
        return panel != null && panel.activeInHierarchy;
    }

    public bool IsLoggedIn()
    {
        return loggedInThisSession;
    }

    public static void Logout()
    {
        PlayerPrefs.DeleteKey("AuthLoggedIn");
        PlayerPrefs.DeleteKey(PREF_USERNAME);
        PlayerPrefs.DeleteKey(PREF_TOKEN);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    class SiteLoginRequest
    {
        public string login;
        public string password;
    }

    [System.Serializable]
    class SiteRegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public bool success;
        public string message;
        public string error;
        public string username;
        public string token;
        public AuthUser user;

        public void Normalize()
        {
            if (!string.IsNullOrEmpty(error) && string.IsNullOrEmpty(message))
                message = error;

            if (!success)
                success = !string.IsNullOrEmpty(token) || user != null;

            if (string.IsNullOrEmpty(username) && user != null)
                username = user.username;
        }
    }

    [System.Serializable]
    public class AuthUser
    {
        public int id;
        public string username;
        public string email;
        public string role;
    }
}
