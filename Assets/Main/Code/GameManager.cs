using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool tagger;
    public static bool Tagger
    {
        get
        {
            return instance.tagger; 
        }
    }
    private static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

}
