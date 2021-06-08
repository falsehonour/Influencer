
using UnityEngine;
using UnityEngine.UI;

public class PowerUpButton : FakeButton
{ 
    [SerializeField] private TMPro.TextMeshProUGUI countText;
    [SerializeField] private Image icon;

    public void SetGraphics(PowerUp powerUp)
    {
        string text = powerUp.count <= 1 ? "" : powerUp.count.ToString();
        countText.text = text;

        icon.sprite = PowerUpsProperties.GetIcon(powerUp.type);

    }
}
