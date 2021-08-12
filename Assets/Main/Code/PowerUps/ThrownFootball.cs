
namespace HashtagChampion
{
    public class ThrownFootball : PhysicalProjectile
    {
        private const float SPEED = 11f;
        protected override float Speed => SPEED;

        protected override void HitPlayer(Player player)
        {
            base.HitPlayer(player);
            player.OnFootballHit();
        }
    }
}


