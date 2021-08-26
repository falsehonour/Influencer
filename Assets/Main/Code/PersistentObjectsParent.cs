using UnityEngine;

public class PersistentObjectsParent : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
