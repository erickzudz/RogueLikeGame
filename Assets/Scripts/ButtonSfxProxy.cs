using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonSfxProxy : MonoBehaviour
{
    [Header("Cambio de escena (opcional)")]
    public bool loadScene = false;
    public string sceneToLoad;

    public void OnClick()
    {
        // Reproduce el clic usando el singleton persistente, si existe
        if (UISound.I != null)
            UISound.I.PlayClick();

        if (loadScene && !string.IsNullOrEmpty(sceneToLoad))
            StartCoroutine(LoadAfterDelay());
    }

    IEnumerator LoadAfterDelay()
    {
        float d = (UISound.I != null) ? UISound.I.delayBeforeLoad : 0.12f;
        yield return new WaitForSecondsRealtime(d);
        SceneManager.LoadScene(sceneToLoad);
    }
}
