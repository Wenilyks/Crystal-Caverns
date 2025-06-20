using UnityEngine;

public class Entity : MonoBehaviour
{
    public virtual void GetDamage()
    {

    }

    public virtual void Die()
    {
        Destroy(this.gameObject);
    }
    
}
