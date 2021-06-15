using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject onScreenParent;
    [SerializeField] private GameObject offScreenParent;
    private bool isOnScreen;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMPro.TextMeshProUGUI character;
    private Camera camera;
    private Transform anchor;
    private Transform myTransform;


    public void SetHealthBarFill(float fill)
    {
        healthBarFill.fillAmount = fill;
    }

    public void SetCharacter(char character)
    {
        this.character.text = character.ToString();
    }


    public void Initialise(Transform anchor, Camera camera)
    {
        this.camera = camera;
        this.anchor = anchor;
        myTransform = transform;
    }

    private void Update()
    {
        bool wasOnScreen = isOnScreen;

        Vector3 newPosition = camera.WorldToScreenPoint(anchor.position);
        isOnScreen = true;
        if (newPosition.x >= 0 && newPosition.x < Screen.width && newPosition.y > 0 && newPosition.y < Screen.height)
        {
            isOnScreen = true;
        }
        else
        {
            isOnScreen = false;
            newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height);
        }
        myTransform.position = newPosition;

        if(isOnScreen != wasOnScreen)
        {
            onScreenParent.SetActive(isOnScreen);
            offScreenParent.SetActive(!isOnScreen);
        }
    }
}
