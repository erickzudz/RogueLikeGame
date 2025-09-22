using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 720f;
    public bool faceMouse = true;

    CharacterController cc;
    Camera cam;
    PhotonView pv;

    void Awake() { cc = GetComponent<CharacterController>(); cam = Camera.main; pv = GetComponent<PhotonView>(); }

    void Update()
    {
        if (pv && !pv.IsMine) return; // solo dueño procesa input

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;
        cc.SimpleMove(input * moveSpeed);

        if (faceMouse && cam)
        {
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            if (new Plane(Vector3.up, Vector3.zero).Raycast(r, out float enter))
            {
                Vector3 hit = r.GetPoint(enter);
                Vector3 dir = hit - transform.position; dir.y = 0;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion t = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, t, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else if (input.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(input), rotationSpeed * Time.deltaTime);
        }
    }
}
