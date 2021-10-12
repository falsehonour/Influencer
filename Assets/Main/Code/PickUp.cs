using UnityEngine;
using Mirror;

public class PickUp : NetworkSpawnable
{
    [Server]
    public void Collect()
    {
        Die();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        Hide();
    }
}