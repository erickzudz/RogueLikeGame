using UnityEngine;

public class UISound : MonoBehaviour
{
    private static UISound instance;
    private AudioSource audioSource;

    [Header("Clips")]
    public AudioClip clickClip;   // arrastra tu "menuclic" aquí

    void Awake()
    {
        // Singleton para usarlo desde cualquier botón
        if (instance == null)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método estático que llaman los botones
    public static void PlayClick()
    {
        if (instance != null && instance.clickClip != null)
        {
            instance.instanceAudio();
        }
    }

    private void instanceAudio()
    {
        audioSource.PlayOneShot(clickClip);
    }
}
