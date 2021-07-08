using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text text;
    [SerializeField] private float counterUpdateInterval = 1f ;

    //[SerializeField] private float[] samples;
    private int framesCounted = 0;
    private float accumulatedDeltaTime;
    private void Start()
    {
        UpdateFPSCounter();
    }

   /*private void AddSample()
    {
        accumulatedDeltaTime += Time.deltaTime;
        framesCounted++;
        //Invoke("AddSample", 0.025f);
    }*/

    private void Update()
    {
        accumulatedDeltaTime += Time.deltaTime;
        framesCounted++;
      //  AddSample();
    }

    private void UpdateFPSCounter()
    {
        float fps = 1f / (accumulatedDeltaTime / (float)framesCounted);
        string fpsText = Mathf.RoundToInt(fps).ToString();
        text.text = fpsText;

        accumulatedDeltaTime = 0;
        framesCounted = 0;

        Invoke("UpdateFPSCounter", counterUpdateInterval);

    }
}
