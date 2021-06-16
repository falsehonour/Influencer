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
    [SerializeField] private TMPro.TextMeshProUGUI text;
    private string playerDisplayName;
    private char proceedingCharacter;
    //[SerializeField] private TMPro.TextMeshProUGUI playerName;
    private Camera camera;
    private Transform anchor;
    private Transform myTransform;
    private Canvas canvas;

    public void SetHealthBarFill(float fill)
    {
        healthBarFill.fillAmount = fill;
    }

    public void SetProceedingCharacter(char character)
    {
        this.proceedingCharacter = character;
        UpdateText();
    }

    public void SetPlayerName(string playerName)
    {
        playerDisplayName = playerName;
        UpdateText();
    }

    private void UpdateText()
    {
        //HARDCODED colours and sizes
        string value = string.Empty;
        if(proceedingCharacter != ' ')
        {
            value += "<size=100%>" + "<color=\"red\">" + proceedingCharacter;
        }
        value += "<size=70%>" + "<color=\"white\">" + playerDisplayName;//character.ToString();

        text.text = value;
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

        Vector3 newPosition = camera.WorldToScreenPoint(anchor.position);
        // Debug.Log(bounds.rect.width);
        float scaleFactor = canvas.scaleFactor;
        float xBoundry = halfWidth * scaleFactor;
        float yBoundry = halfHeight * scaleFactor;

        newPosition.x = Mathf.Clamp(newPosition.x, xBoundry, Screen.width - xBoundry);
        newPosition.y = Mathf.Clamp(newPosition.y, yBoundry, Screen.height - yBoundry);
        //newPosition.z = 0;
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
