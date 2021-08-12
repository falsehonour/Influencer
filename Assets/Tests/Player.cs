using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField] private Text text;
    [SyncVar] private string testString;
    [SyncVar] private int testInt;


    private void Start()
    {
        if (hasAuthority)
        {
            Cmd_ManipulateTestVars();
        }
    }
    // Update is called once per frame
    void Update()
    {
        text.text = /*testString + */testInt.ToString();
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Cmd_ManipulateTestVars();
            }
        }
    }



    [Command]
    private void Cmd_ManipulateTestVars()
    {
        int newNum = testInt;
        while (newNum == testInt)
        {
            newNum = Random.Range(1, 10);
        }
        testInt = newNum;
        /*testString = "Six, Six, Six";
        testInt = 666;*/
    }
}
