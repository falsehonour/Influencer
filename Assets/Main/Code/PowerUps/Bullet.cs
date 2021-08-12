using UnityEngine;

namespace HashtagChampion
{
    public class Bullet : PhysicalProjectile
    {
        private const float SPEED = 11f;
        protected override float Speed => SPEED;

        protected override void OnSpawn(Vector3 position, Quaternion rotation, uint callerNetId)
        {
            base.OnSpawn(position, rotation, callerNetId);
            if (isClient)
            {
                SoundManager.PlayOneShotSound(SoundNames.GunShot, position);
            }

        }

        protected override void HitPlayer(Player player)
        {
            base.HitPlayer(player);
            player.OnBulletHit();
        }

        protected override void Stop()
        {
            base.Stop();
            SoundManager.PlayOneShotSound(SoundNames.BulletHit, myTransform.position);

        }
    }
}

