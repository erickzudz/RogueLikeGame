using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ARController : MonoBehaviour
{
    [Header("Refs")]
    public ARSession arSession;               // arrastra el AR Session de la escena
    public ARCameraManager arCamera;          // arrastra la Main Camera (hija del XR Origin)
    public ARPlaneManager planeMgr;           // arrastra el AR Plane Manager (XR Origin)
    public ARCameraBackground background;     // arrastra el ARCameraBackground (mismo objeto que arCamera)
    public TextMeshProUGUI hud;               // opcional: tu label de debug

    bool running = false;
    bool gotFrame = false;

    void Awake()
    {
        if (!background && arCamera) background = arCamera.GetComponent<ARCameraBackground>();
    }

    void OnEnable()
    {
        if (arCamera) arCamera.frameReceived += OnFrame;
    }

    void OnDisable()
    {
        if (arCamera) arCamera.frameReceived -= OnFrame;
    }

    void OnFrame(ARCameraFrameEventArgs _)
    {
        gotFrame = true;
    }

    public void StartARButton() => StartCoroutine(StartAR());

    IEnumerator StartAR()
    {
        Log("StartAR ► solicitando permiso cámara…");
        // 0) Permiso de cámara
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                yield return null;
        }

        // 1) Inicializar XR
        var xr = XRGeneralSettings.Instance.Manager;
        if (xr.activeLoader == null)
        {
            Log("Inicializando XR loader…");
            yield return xr.InitializeLoader();
            if (xr.activeLoader == null) { Log("XR loader FAIL"); yield break; }
        }
        xr.StartSubsystems();
        Log("XR subsystems ON");

        // 2) Chequear/instalar ARCore
        Log("Chequeando disponibilidad AR…");
        yield return ARSession.CheckAvailability();
        if (ARSession.state == ARSessionState.NeedsInstall)
            yield return ARSession.Install();

        Log("ARSession.state = " + ARSession.state);
        if (ARSession.state == ARSessionState.Unsupported)
        {
            Log("AR UNSUPPORTED (dispositivo/servicios).");
            yield break;
        }

        // 3) Encender AR Session
        arSession.enabled = true;
        if (planeMgr) planeMgr.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        if (background) background.enabled = true;

        // 4) Esperar permiso efectivo + primer frame de cámara
        float t = 0f; gotFrame = false;
        while (arCamera && !arCamera.permissionGranted && t < 3f) { t += Time.deltaTime; yield return null; }
        t = 0f;
        while (!gotFrame && t < 3f) { t += Time.deltaTime; yield return null; }

        Log($"permGranted={(arCamera ? arCamera.permissionGranted : false)}, gotFrame={gotFrame}, state={ARSession.state}");

        running = arCamera && arCamera.permissionGranted && gotFrame;
        if (!running)
            Log("Sin video aún. Si usas **URP**, agrega la 'AR Background Renderer Feature' al Renderer.");
        else
            Log("AR RUNNING ✓ toca el piso para colocar el mapa.");
    }

    void Log(string m)
    {
        Debug.Log(m);
        if (hud) hud.text = m + "\n" + hud.text;
    }
}
