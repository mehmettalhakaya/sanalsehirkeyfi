#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class AllowHttpForUnityAuth
{
    static AllowHttpForUnityAuth()
    {
        if (PlayerSettings.insecureHttpOption == InsecureHttpOption.AlwaysAllowed)
            return;

        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
        AssetDatabase.SaveAssets();
    }
}
#endif
