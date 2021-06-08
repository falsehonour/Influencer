using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StressTester : MonoBehaviour
{
    [SerializeField] private Animator preFab;
    private List<Animator> animators = new List<Animator>();
    [SerializeField] private BoxCollider rangeCollider;
    [SerializeField] private Text fpsCounter;

    private void Start()
    {
        InvokeRepeating("ShowFPS", 0.2f, 0.2f);
    }

    public void Instantiate()
    {
        Bounds bounds = rangeCollider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        Vector3 position = new Vector3(x, y, z);
        Quaternion rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        Animator animator = Instantiate(preFab, position, rotation);
        animators.Add(animator);
    }

    public void Animate()
    {
        for (int i = 0; i < animators.Count; i++)
        {
            animators[i].SetTrigger("StressTrigger");
            animators[i].SetInteger("Stress",Random.Range(0, 8));
        }
    }

    private void ShowFPS()
    {
        fpsCounter.text = "FPS: " + (1f / Time.deltaTime).ToString("f3");
    }
}
