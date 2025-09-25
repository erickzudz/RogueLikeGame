using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;

public class ARStartup : MonoBehaviour
{
    public ARSession arSession;
    public ARCameraManager cameraManager;

    IEnumerator Start()
    {
        // 1) Pedir permiso de cámara (si no se otorgó)
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            // Espera a que el usuario responda (siguiente frame ya refleja el cambio)
            yield return null;
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.LogWarning("Camera permission denied.");
                yield break;
            }
        }

        // 2) Comprobar disponibilidad / instalación de ARCore
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            yield return ARSession.CheckAvailability();

        if (ARSession.state == ARSessionState.NeedsInstall)
            yield return ARSession.Install(); // Mostrará prompt de "Google Play Services for AR"

        if (ARSession.state != ARSessionState.Ready)
        {
            Debug.LogWarning("AR not supported or ARCore not installed/updated.");
            yield break;
        }

        // 3) Habilitar AR
        arSession.enabled = true;

        // (Opcional) Verifica permiso efectivo del ARCameraManager
        if (cameraManager && !cameraManager.permissionGranted)
            Debug.LogWarning("ARCameraManager: camera permission still not granted.");
    }
}
