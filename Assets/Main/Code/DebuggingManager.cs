using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class DebuggingManager : MonoBehaviour
{
    [SerializeField] private GameObject DebuggingUIParent;
    [SerializeField] private PostProcessLayer postProcessLayer;
    [SerializeField] private Light directionalLight;
    [SerializeField] private InputField targetFPSInputField;
    [SerializeField] private GameObject FPSCounter;
    [SerializeField] private InputField qualityLevelInputField;
    [SerializeField] private Material debugMat;

    private void Start()
    {
        DebuggingUIParent.SetActive(false);
        FPSCounter.SetActive(false);
        Color colour = debugMat.color;
        colour.a = 0;
        debugMat.color = colour;
    }

    public void SwitchShowUI()
    {
        DebuggingUIParent.SetActive(!DebuggingUIParent.activeSelf);
    }

    public void SwitchPostProcessing()
    {
        postProcessLayer.enabled = !postProcessLayer.enabled;
    }

    public void SwitchDirectionalLight()
    {
        directionalLight.enabled = !directionalLight.enabled;
    }

    public void SwitchFPSCounter()
    {
        FPSCounter.SetActive(!FPSCounter.activeSelf);
    }

    public void SetTargetFPS()
    {
        int targetFPS;
        if(int.TryParse(targetFPSInputField.text, out targetFPS))
        {
            Application.targetFrameRate = targetFPS;
            //QualitySettings.vSyncCount =
            //QualitySettings.SetQuality Level()
        }
    }

    public void SetQualityLevel()
    {
        int qualityLevel;
        if (int.TryParse(qualityLevelInputField.text, out qualityLevel))
        {
            //QualitySettings.vSyncCount =
            QualitySettings.SetQualityLevel(qualityLevel);
        }
    }

    public void SwitchDebugMat()
    {
        Color colour = debugMat.color;
        colour.a = colour.a > 0 ? 0 : 0.5f;
        debugMat.color = colour;
    }
}
