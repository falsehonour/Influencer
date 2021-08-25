
using UnityEngine;

namespace HashtagChampion
{
    public class ThrownFootball : PhysicalProjectile
    {
        private const float SPEED = 11f;
        protected override float Speed => SPEED;

        protected override void OnSpawn(Vector3 position, Quaternion rotation, uint callerNetId)
        {
            base.OnSpawn(position, rotation, callerNetId);
            if (isClient)
            {
                SoundManager.PlayOneShotSound(SoundNames.FootballKicked, position);
            }

        }

        protected override void Stop()
        {
            base.Stop();
            SoundManager.PlayOneShotSound(SoundNames.FootballHit, myTransform.position);
        }

        protected override void HitPlayer(Player player)
        {
            base.HitPlayer(player);
            player.OnFootballHit();
        }
    }
}


