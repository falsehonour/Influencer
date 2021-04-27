using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GameStates
    {
        Waiting, ChoosingTagger, TagGame
    }
    private static GameStates state;
    public static GameStates State
    {
        get { return state; }
    }
    [SerializeField] private Transform[] circleSpawnSpots;
    // private static GameManager instance;

    [Server]
    public void OnServerStarted()
    {
        StartCoroutine(WaitForPlayers());
    }

    [Server]
    private IEnumerator WaitForPlayers()
    {
        Debug.Log("WaitForPlayers()");

        state = GameStates.Waiting;

        int requiredPlayerNumber = 3;
        while (PlayerController.allPlayers.Count < requiredPlayerNumber)
        {
            yield return new WaitForSeconds(0.2f);
        }
        StartCoroutine(ChooseTagger());

    }

    [Server]
    private IEnumerator ChooseTagger()
    {
        Debug.Log("ChooseTagger()");

        state = GameStates.ChoosingTagger;

        int playerCount = PlayerController.allPlayers.Count;
        int taggerIndex = Random.Range(0, playerCount);
        for (int i = 0; i < playerCount; i++)
        {
            PlayerController player = PlayerController.allPlayers[i];
            player.TargetRpc_Teleport(circleSpawnSpots[i].position, circleSpawnSpots[i].rotation);
            player.SetTagger(i == taggerIndex);
        }
        yield return new WaitForSeconds(3f);

        state = GameStates.TagGame;

    }

    public static void PromoteNewTagger()
    {
        int playerCount = PlayerController.allPlayers.Count;
        int taggerIndex = Random.Range(0, playerCount);
        PlayerController.allPlayers[taggerIndex].SetTagger(true);
    }
    /*[SerializeField] private bool tagger;
public static bool Tagger
{
get
{
   return instance.tagger; 
}
}*/
}
