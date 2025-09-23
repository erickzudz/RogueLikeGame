using UnityEngine;
using UnityEngine.XR.Management;
using System.Collections;

public class ARBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }
    void OnDisable()
    {
        XRGeneralSettings.Instance.Manager?.StopSubsystems();
        XRGeneralSettings.Instance.Manager?.DeinitializeLoader();
    }
}
