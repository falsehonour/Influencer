using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FirstTaggerPointer : NetworkBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private int extraCompleteRotations;

    [ClientRpc]
    public void Rpc_Spin(Vector3 lookAtPoint)
    {
        StartCoroutine(SpinCoroutine(lookAtPoint));
    }

    private IEnumerator SpinCoroutine(Vector3 lookAtPoint)
    {
        Keyframe lastKeyFrame = curve.keys[curve.keys.Length - 1];
        if(lastKeyFrame.value != 1)
        {
            Debug.LogWarning("lastKeyFrame.value != 1, spin animation will not play properly.");
        }
        float startY = transform.eulerAngles.y;

        Vector3 endDirection = (lookAtPoint - transform.position).normalized;
        Quaternion endRotation = Quaternion.LookRotation(endDirection);
        float endY = endRotation.eulerAngles.y + ((float)extraCompleteRotations * 360f);

        float endTime = lastKeyFrame.time;
        float currentTime = 0;
        float y;
        while (currentTime < endTime)
        {
            y = Mathf.Lerp(startY, endY, curve.Evaluate(currentTime));
            transform.rotation = Quaternion.Euler(0, y, 0);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
