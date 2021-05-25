using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionManager : MonoBehaviour
{
    private Occluder[] occluders;
    private List<Disoccluder> disoccluders = new List<Disoccluder>();
    private Camera camera;
    private Collider[] rayCastColliders = new Collider[16];
    private RaycastHit[] rayCastHits = new RaycastHit[16];

    /*private void OcclusionRoutine()
    {
        int disoccludersCount = disoccluders.Count;
        Vector3 cameraPosition = camera.po
        for (int i = 0; i < disoccludersCount; i++)
        {
            bool isOnScreen = true;
            for (int j = 0; j < occluders.Length; j++)
            {

                int rayCastHitsCount = Physics.RaycastNonAlloc()


            }
        }
       //amera.scree
    }*/

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
