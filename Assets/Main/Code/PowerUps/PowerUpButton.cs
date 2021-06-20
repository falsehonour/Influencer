
using UnityEngine;
using UnityEngine.UI;

public class PowerUpButton : FakeButton
{
    [SerializeField] private GameObject countGraphics;
    [SerializeField] private TMPro.TextMeshProUGUI countText;
    [SerializeField] private Image powerUpIcon;
    [SerializeField] private Vector3 powerUpIconBaseScale;
    [SerializeField] private float powerUpIconSizeMultiplierWhilePressed;
    public void SetGraphics(PowerUp powerUp)
    {
        bool showCount = (powerUp.count > 1);
        if (showCount)
        {
            countGraphics.SetActive(true);
            countText.text = powerUp.count.ToString();
        }
        else
        {
            countGraphics.SetActive(false);
        }
        powerUpIcon.sprite = PowerUpsProperties.GetIcon(powerUp.type);
    }

    public override void Press()
    {
        base.Press();
        powerUpIcon.transform.localScale = powerUpIconBaseScale * powerUpIconSizeMultiplierWhilePressed;
    }

    protected override void Unpress()
    {
        base.Unpress();
        powerUpIcon.transform.localScale = powerUpIconBaseScale;

    }
}
