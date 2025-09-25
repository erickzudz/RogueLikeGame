using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UISound : MonoBehaviour
{
    public static UISound I;

    [Header("Refs")]
    public AudioSource sfxSource;           // arrastra aquí el AudioSource de UIAudio
    public AudioClip clickClip;             // arrastra tu menuclic
    [Range(0f, 1f)] public float clickVolume = 1f;
    [Tooltip("Retraso para que se oiga el clic antes de cambiar de escena")]
    public float delayBeforeLoad = 0.12f;   // 120 ms suele bastar

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject); // el audio no se corta al cambiar de escena
    }

    public void PlayClick()
    {
        if (sfxSource && clickClip) sfxSource.PlayOneShot(clickClip, clickVolume);
    }

    // Úsalo desde el botón para sonar y luego cambiar de escena
    public void PlayClickThenLoad(string sceneName)
    {
        StartCoroutine(PlayAndLoad(sceneName));
    }

    IEnumerator PlayAndLoad(string sceneName)
    {
        PlayClick();
        yield return new WaitForSecondsRealtime(delayBeforeLoad);
        SceneManager.LoadScene(sceneName);
    }
}