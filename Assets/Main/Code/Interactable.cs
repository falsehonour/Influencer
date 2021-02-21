using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class Interactable : NetworkBehaviour
{
    public UnityEvent OnInteract;
    [SerializeField] private InteractableAccessPoint[] accessPoints;

    [Server]
    public virtual void Interact(/*PlayerController player*/)
    {
        OnInteract.Invoke();
    }
    public InteractableAccessPoint GetClosestAccessPoint(Vector3 position)
    {
        InteractableAccessPoint closestPoint = null;

        if (accessPoints == null || accessPoints.Length <= 0)
        {
            Debug.LogError("No Access Points!");
        }
        else
        {
            float smallestSquaredDistance = float.MaxValue;
            for (int i = 0; i < accessPoints.Length; i++)
            {
                InteractableAccessPoint point = accessPoints[i];
                float squaredDistance = Vector3.SqrMagnitude(point.myTransform.position - position);

                if (squaredDistance < smallestSquaredDistance)
                {
                    smallestSquaredDistance = squaredDistance;
                    closestPoint = point;
                }
            }
        }

        return closestPoint;
    }
}

