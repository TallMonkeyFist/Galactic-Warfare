
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public PlayerMovement Owner { get; private set; } = null;

    public void SetOwner(PlayerMovement owner)
    {
        if(Owner == null)
        {
            Owner = owner;
        }
    }
}
