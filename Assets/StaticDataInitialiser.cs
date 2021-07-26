using UnityEngine;

public class StaticDataInitialiser : MonoBehaviour
{
    private void Awake()
    {
        StaticData.Initialise();
    }
}
