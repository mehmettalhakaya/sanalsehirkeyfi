using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        // Oyundaki ana kamerayý bulur
        mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError("Billboard: Ana kamera bulunamadý! Lütfen kameranýn etiketini kontrol et.");
        }
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        // LookAt yerine doðrudan kameranýn baktýðý yönü ve rotasyonu kopyalýyoruz.
        // Bu UI için çok daha kusursuz çalýþýr.
        transform.rotation = mainCam.transform.rotation;
    }
}