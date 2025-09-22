using UnityEngine;

public class NetworkBullet : MonoBehaviour
{
    public float speed = 12f;
    public float life = 3f;
    public float damage = 1f;

    void Start() { Destroy(gameObject, life); }
    void Update() { transform.Translate(Vector3.forward * speed * Time.deltaTime); }

    void OnCollisionEnter(Collision c)
    {
        c.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }
}
