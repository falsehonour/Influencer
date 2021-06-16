﻿public class ThrownFootball : PhysicalProjectile
{
    private const float SPEED = 11f;
    protected override float Speed => SPEED;

    protected override void Hit(PlayerController player)
    {
        base.Hit(player);
        player.OnFootballHit();
    }
}