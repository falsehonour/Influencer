public class PowerUpPickUp : PickUp
{
    [UnityEngine.SerializeField] private PowerUp powerUp;
    public PowerUp GetPowerUp() => powerUp;
}
