using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 720f;
    public bool allowMouseAim = true;   // PC: mirar con mouse cuando no hay joystick

    CharacterController cc;
    Camera cam;
    PhotonView pv;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cam = Camera.main;
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (pv && !pv.IsMine) return;

        // --- INPUT ---
        Vector2 joy = Vector2.zero;
        if (MobileHUD.I && MobileHUD.I.joystick)
            joy = MobileHUD.I.joystick.Axis;       // (-1..1)

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Si hay joystick activo, sobreescribe teclado
        bool joystickActive = joy.sqrMagnitude > 0.0004f; // ~0.02^2
        if (joystickActive) { h = joy.x; v = joy.y; }

        // --- MOVE ---
        Vector3 move = new Vector3(h, 0f, v);
        if (move.sqrMagnitude > 1f) move.Normalize();
        cc.SimpleMove(move * moveSpeed);

        // --- ROTATE ---
        if (joystickActive)
        {
            // Apunta hacia la dirección del joystick
            Quaternion target = Quaternion.LookRotation(new Vector3(joy.x, 0f, joy.y));
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, rotationSpeed * Time.deltaTime);
        }
        else if (allowMouseAim && cam)
        {
            // PC: mira donde está el mouse
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            if (new Plane(Vector3.up, transform.position).Raycast(r, out float enter))
            {
                Vector3 hit = r.GetPoint(enter);
                Vector3 dir = hit - transform.position; dir.y = 0;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    var t = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, t, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else if (move.sqrMagnitude > 0.0001f)
        {
            // Sin mouse: mira hacia el movimiento (WASD)
            Quaternion t = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, t, rotationSpeed * Time.deltaTime);
        }
    }
}
