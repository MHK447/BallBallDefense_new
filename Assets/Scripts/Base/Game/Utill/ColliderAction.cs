using UnityEngine;

public class ColliderAction : MonoBehaviour
{
    [HideInInspector]
    public System.Action<Collider2D> TriggerEnterAction = null;

    [HideInInspector]
    public System.Action<Collision2D> CollisionEnterAction = null;

    public System.Action AttackAction = null;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerEnterAction?.Invoke(collision);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEnterAction?.Invoke(collision);
    }


    public void AttackEvent()
    {
        AttackAction?.Invoke();
    }
}
