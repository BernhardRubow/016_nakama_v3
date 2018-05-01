using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;

public class nvp_GameManager_scr : MonoBehaviour
{

  // +++ fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  public nvp_NetworkManager_scr networkManager;

  public GameObject playerPrefab;

  Dictionary<string, GameObject> players;

  public bool isMasterClient;

  public List<Transform> spawnPoints;

  private Action currentStateUpdate = () => { };


  // +++ unity callbacks
  void Start()
  {
    networkManager.OnInitMatchComplete = OnInitMatchComplete;
  }

  // Update is called once per frame
  void Update()
  {
    currentStateUpdate();
  }

  // +++ eventhandler +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  void OnInitMatchComplete()
  {
    currentStateUpdate = state_InitGame_update;

  }

  // +++ states +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  void state_InitGame_update()
  {
    Debug.LogFormat("OPPONENT REPORT: ({0})", "InitComplete");
    Debug.Log("------------------------------------------------------");
    Debug.LogFormat("Opponents in game: {0}", networkManager.connectedOpponents.Count);

    foreach (var presence in networkManager.connectedOpponents)
    {
      var userId = presence.UserId;
      var handle = presence.Handle;

      Debug.LogFormat("LocalUser ({2}) - User id '{0}' handle '{1}'.", userId, handle, userId == networkManager.localPlayerId);
    }
    Debug.Log("------------------------------------------------------");

    // set Master client to the first player in the game
    if (networkManager.connectedOpponents[0].Handle == networkManager.localPlayerHandle) isMasterClient = true;

    currentStateUpdate = state_SpawnPlayers_update;
  }

  void state_SpawnPlayers_update()
  {
    for(int i = 0; i < 2; i++){
      var go = Instantiate(playerPrefab, spawnPoints[i].transform.position, Quaternion.identity);
      
      if(networkManager.connectedOpponents[i].Handle == networkManager.localPlayerHandle){
        go.transform.GetChild(0).gameObject.SetActive(false);
      }
      else
      {
        go.transform.GetChild(1).gameObject.SetActive(true);
      }
    }



    currentStateUpdate = () => {};
  }
}
