using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HashtagChampion
{
    public class MenusCameraController : MonoBehaviour
    {
        [SerializeField] private Camera camera;
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
        [SerializeField] private float raycastMaxDistance =4f;
        private bool isControllable;
        private bool touchAnchored =false;
        private bool characterBeingClicked = false;
        private Vector2 lastTouchPosition;
        [SerializeField] private AnimationCurve moveToInitialStateCurve;
        [SerializeField] private Collider characterClickableCollider;
        [SerializeField] private UnityEvent OnCharacterClicked;
        public void SetIsControllable(bool value)
        {
            isControllable = value;
            if(value == false)
            {
                StartCoroutine(MoveToInitialState());
            }
        }

        private IEnumerator MoveToInitialState()
        {
            Quaternion startRotation = cameraAnchorTransform.rotation;
            //TODO:endRotation isnt nesssrily what is HARDCODED in here
            Quaternion endRotation = Quaternion.LookRotation(Vector3.zero);

            Keyframe lastKeyFrame = moveToInitialStateCurve.keys[moveToInitialStateCurve.keys.Length - 1];
            if (lastKeyFrame.value != 1)
            {
                Debug.LogWarning("lastKeyFrame.value != 1, animation will not play properly.");
            }

            float endTime = lastKeyFrame.time;
            float currentTime = 0;

            while (currentTime < endTime)
            {
                Quaternion rotation = Quaternion.Lerp(startRotation, endRotation, moveToInitialStateCurve.Evaluate(currentTime));
                cameraAnchorTransform.rotation = rotation;

                currentTime += Time.deltaTime;
                yield return null;
            }
        }

        void Update()
        {
            if (isControllable)
            {
                if (InputManager.GetTouch())
                {
                    Vector2 touchScreenPosition = InputManager.GetTouchPosition();

                    bool justTouched = InputManager.GetTouchDown();

                    if (justTouched)
                    {
                        RaycastHit raycastHit = RaycastFromCamera(touchScreenPosition);

                        if (raycastHit.collider == characterClickableCollider)
                        {
                            touchAnchored = true;
                        }
                    }
                    else if (touchAnchored)
                    {
                        if (allowYRotation)
                        {
                            float xDifference = (touchScreenPosition - lastTouchPosition).x;
                            Vector3 rotation = new Vector3(0, xDifference * yRotationSpeed, 0);
                            cameraAnchorTransform.Rotate(rotation);
                        }
                    }

                    lastTouchPosition = touchScreenPosition;
                }
                else
                {
                    touchAnchored = false;
                }
                

                if (false)
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
            else if(false)
            {
                if (InputManager.GetTouchDown())
                {
                    Vector2 touchScreenPosition = InputManager.GetTouchPosition();
                    RaycastHit raycastHit = RaycastFromCamera(touchScreenPosition);
                    if (raycastHit.collider == characterClickableCollider)
                    {
                        characterBeingClicked = true;
                    }
                }
                else if(InputManager.GetTouchUp())
                {
                    if (characterBeingClicked)
                    {
                        Vector2 touchScreenPosition = InputManager.GetTouchPosition();
                        RaycastHit raycastHit = RaycastFromCamera(touchScreenPosition);
                        if (raycastHit.collider == characterClickableCollider)
                        {
                            OnCharacterClicked.Invoke();
                        }
                        characterBeingClicked = false;
                    }
                }
            }
            
        }

        private RaycastHit RaycastFromCamera(Vector2 screenPosition)
        {
            Ray ray = camera.ScreenPointToRay(screenPosition);
            // Debug.DrawRay(ray.origin, ray.direction, Color.green, 0.3f);
            RaycastHit raycastHit;
            Physics.Raycast(ray, out raycastHit, raycastMaxDistance);
            return raycastHit;
        }

    }
}