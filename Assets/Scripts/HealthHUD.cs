using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;

public class HealthHUD : MonoBehaviour
{
    [Header("Refs UI")]
    public Slider hpSlider;      // 0..1
    public TMP_Text hpText;      // opcional "80 / 100"

    Health localHealth;

    void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    IEnumerator BindWhenReady()
    {
        // Espera hasta que exista el player local instanciado por Photon
        while (localHealth == null)
        {
            foreach (var h in FindObjectsOfType<Health>())
            {
                if (h.TryGetComponent(out PhotonView pv) && pv.IsMine)
                {
                    Bind(h);
                    break;
                }
            }
            if (localHealth == null) yield return null; // siguiente frame
        }
    }

    void Bind(Health h)
    {
        if (localHealth != null)
            localHealth.onHealthChanged.RemoveListener(OnHealthChanged);

        localHealth = h;
        localHealth.onHealthChanged.AddListener(OnHealthChanged);
        OnHealthChanged(localHealth.CurrentHP, localHealth.maxHP); // inicial
    }

    void OnDestroy()
    {
        if (localHealth != null)
            localHealth.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    void OnHealthChanged(float hp, float max)
    {
        if (hpSlider) hpSlider.value = max > 0 ? hp / max : 0f;
        if (hpText) hpText.text = $"{Mathf.CeilToInt(hp)}";
    }
}
