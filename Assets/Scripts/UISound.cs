using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UISound : MonoBehaviour
{
    public static UISound I;

    [Header("Audio")]
    public AudioSource sfxSource;     // Asigna el AudioSource del UIAudio
    public AudioClip clickClip;       // Asigna tu clip de clic (WAV/OGG recomendado)
    [Range(0f, 1f)] public float clickVolume = 1f;
    public float delayBeforeLoad = 0.12f; // 120 ms para que se oiga

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);        // no se destruye al cambiar de escena
    }

    public void PlayClick()
    {
        if (sfxSource && clickClip)
            sfxSource.PlayOneShot(clickClip, clickVolume);
    }

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
