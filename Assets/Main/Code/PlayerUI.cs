using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    /*[SerializeField] private GameObject onScreenParent;
    [SerializeField] private GameObject offScreenParent;
    private bool isOnScreen;*/
    //TODO: Too many fields for such a small object....
    //TODO: Feels like these values should not be stored in memory for each PlayerUI object
    [SerializeField] private RectTransform bounds;
    private float halfWidth;
    private float halfHeight;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    private char proceedingCharacter;
    private string playerDisplayName;
    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private TMPro.TextMeshProUGUI powerUpCountText;
    [SerializeField] private Image powerUpIcon;
    [SerializeField] private bool roundScreenPosition;
    private Camera camera;
    private Transform anchor;
    private Transform myTransform;
    private Canvas canvas;

    public void SetHealthBarFill(sbyte health)
    {
        float fill = ((float)health / (float)PlayerController.PlayerServerData.MAX_HEALTH);
        healthBarFill.fillAmount = fill;
        healthText.text = health.ToString();
    }

    public void SetProceedingCharacter(char character)
    {
        this.proceedingCharacter = character;
        UpdateNameText();
    }

    public void SetPlayerName(string playerName)
    {
        playerDisplayName = playerName;
        UpdateNameText();
    }

    private void UpdateNameText()
    {
        //HARDCODED colours and sizes
        string value = string.Empty;
        if(proceedingCharacter != ' ')
        {
            value += "<size=100%>" + "<color=\"red\">" + proceedingCharacter;
        }
        value += "<size=70%>" + "<color=\"white\">" + playerDisplayName;//character.ToString();

        nameText.text = value;
    }

    public void SetPowerUp(PowerUp powerUp)
    {
        //NOTE: This is pretty much identical to what we have at PowerUpButton's script
        bool showCount = (powerUp.count > 1);
        powerUpCountText.text = showCount ? powerUp.count.ToString() : "";
        powerUpIcon.sprite = PowerUpsProperties.GetIcon(powerUp.type);
    }

    public void Initialise(Transform anchor, Camera camera)
    {
        canvas = GetComponentInParent<Canvas>();
        this.camera = camera;
        this.anchor = anchor;
        myTransform = transform;

        halfWidth = bounds.rect.width / 2f;
        halfHeight = bounds.rect.height / 2f;

    }

    private void Update()
    {
        //TODO: Feels like some stuff can be precalculated/cached
        //bool wasOnScreen = isOnScreen;
        Vector2 newPosition = camera.WorldToScreenPoint(anchor.position);
        newPosition.y += halfHeight;
        // Debug.Log(bounds.rect.width);
        float scaleFactor = canvas.scaleFactor;
        float xBoundry = halfWidth * scaleFactor;
        float yBoundry = halfHeight * scaleFactor;

        newPosition.x = Mathf.Clamp(newPosition.x, xBoundry, Screen.width - xBoundry);
        newPosition.y = Mathf.Clamp(newPosition.y, yBoundry, Screen.height - yBoundry);

        if (roundScreenPosition)
        {
            newPosition = Vector2Int.RoundToInt(newPosition);
            /* newPosition.x = (int)(newPosition.x);
            newPosition.y = (int)(newPosition.y);*/
            /* newPosition.x = newPosition.x - (newPosition.x % 1f);
            newPosition.y = newPosition.y - (newPosition.y % 1f);*/
        }
       /* newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width);
        newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height);*/
        myTransform.position = newPosition;

        /*isOnScreen = true;
        if (newPosition.x >= halfWidth  && newPosition.x < Screen.width - halfWidth && 
            newPosition.y >= halfHeight && newPosition.y < Screen.height)
        {
            isOnScreen = true;
        }
        else
        {
            isOnScreen = false;
            newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height);
        }*/
       // myTransform.position = newPosition;

        /*if(isOnScreen != wasOnScreen)
        {
            onScreenParent.SetActive(isOnScreen);
            offScreenParent.SetActive(!isOnScreen);
        }*/
    }
}
