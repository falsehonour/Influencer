using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HashtagChampion
{
    public class MenusCameraController : MonoBehaviour
    {

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform cameraAnchorTransform;
        [SerializeField] private bool allowYRotation;
        [SerializeField] private float yRotationSpeed;
        [SerializeField] private bool allowYMovement;
        [SerializeField] private float yMovementSpeed;
        [SerializeField] FloatRange yRange;
        [SerializeField] private bool allowZMovement;
        [SerializeField] private float zMovementSpeed;
        [SerializeField] FloatRange zRange;
        private bool isControllable;
        public void SetIsControllable(bool value)
        {
            isControllable = value;
        }

        void Update()
        {
            if (isControllable)
            {
                if (Input.GetMouseButton(0))
                {
                    if (allowYRotation)
                    {
                        float xMovement = Input.GetAxisRaw("Mouse X");
                        Vector3 rotation = new Vector3(0, xMovement * yRotationSpeed, 0);
                        cameraAnchorTransform.Rotate(rotation);
                    }
                    if (Input.GetMouseButton(1))//TODO: Pinch
                    {
                        if (allowZMovement)
                        {
                            float zMovement = Input.GetAxisRaw("Mouse Y");
                            Vector3 movement = new Vector3(0, 0, zMovement * zMovementSpeed);
                            Vector3 newPosition = cameraTransform.localPosition + movement;
                            newPosition.z = Mathf.Clamp(newPosition.z, zRange.min, zRange.max);
                            cameraTransform.localPosition = newPosition;
                        }
                    }
                    else if (allowYMovement)
                    {
                        float yMovement = Input.GetAxisRaw("Mouse Y");
                        Vector3 movement = new Vector3(0, yMovement * yMovementSpeed, 0);
                        Vector3 newPosition = cameraTransform.localPosition + movement;
                        newPosition.y = Mathf.Clamp(newPosition.y, yRange.min, yRange.max);
                        cameraTransform.localPosition = newPosition;
                    }

                }
            }
            
        }
    }

}