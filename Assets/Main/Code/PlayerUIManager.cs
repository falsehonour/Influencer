using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    public class PlayerUIManager : MonoBehaviour
    {
        [SerializeField] private PlayerUI playerUIPreFab;
        [SerializeField] private Transform playerUICanvasTransform;
        [SerializeField] private Camera camera;
        [SerializeField] private float sortChildrenInterval =0.12f;
        private Transform myTransform;
        private Transform[] children;
        private static PlayerUIManager instance;

        private void Awake()
        {
            instance = this;
            Debug.Log("PlayerUIManager AWOKE");
            myTransform = transform;
            int childCount = myTransform.childCount;
            children = new Transform[childCount];
            StartCoroutine(SortChildrenRoutine());
        }

        public static PlayerUI CreatePlayerUI(Transform anchor)
        {
            return instance.CreateNewPlayerUI(anchor);
        }

        private PlayerUI CreateNewPlayerUI(Transform anchor)
        {
            PlayerUI playerUI = Instantiate(playerUIPreFab, playerUICanvasTransform);
            playerUI.Initialise(anchor, camera);
            return playerUI;
        }

        private IEnumerator SortChildrenRoutine()
        {
            WaitForSeconds waitInterval = new WaitForSeconds(sortChildrenInterval); 
            while (true)
            {
                //TODO: Not efficient..
                int childCount = myTransform.childCount;
                if (childCount != children.Length)
                {
                    children = new Transform[childCount];
                }
                for (int i = 0; i < childCount; i++)
                {
                    children[i] = myTransform.GetChild(i);
                }
                for (int i = 0; i < childCount; i++)
                {
                    for (int j = i + 1; j < childCount; j++)
                    {
                        if (children[j].position.y > children[i].position.y)
                        {
                            Transform swap = children[i];
                            children[i] = children[j];
                            children[j] = swap;
                        }
                    }
                    if (children[i].GetSiblingIndex() != i)
                    {
                        children[i].SetSiblingIndex(i);
                    }
                }
                /* for (int i = 0; i < childCount; i++)
                 {
                     children[i].SetSiblingIndex(i);
                 }*/

                yield return waitInterval;
            }

        }
    }

}
