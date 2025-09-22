using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth = 10f;
    public UnityEvent onDeath;
    float hp;

    void Awake() { hp = maxHealth; }
    public void TakeDamage(float dmg) { hp -= dmg; if (hp <= 0f) { onDeath?.Invoke(); Destroy(gameObject); } }
}
