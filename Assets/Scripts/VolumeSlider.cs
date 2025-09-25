using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioSource musicSource;   // arrastra aqu� el MainMenuAudio
    public Slider slider;             // arrastra aqu� tu Slider

    void Start()
    {
        if (musicSource != null && slider != null)
        {
            slider.value = musicSource.volume;  // iniciar con el volumen actual
            slider.onValueChanged.AddListener(SetVolume);
        }
    }

    void SetVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;
    }
}
