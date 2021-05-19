using UnityEngine;
using Mirror;

public class PickUp : Spawnable
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