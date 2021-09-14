using UnityEngine;

public class PersistentObjectsParent : MonoBehaviour
{
    private static bool awoke = false;
    void Awake()
    {
        if (awoke)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            awoke = true;
        }

    }
}
