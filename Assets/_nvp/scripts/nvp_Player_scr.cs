using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nakama;
using ExtensionMethods;

public class nvp_Player_scr : MonoBehaviour, INetworkBehavior
{

  private bool isLocal;
  private INClient client;

  public bool IsLocal()
  {
    return isLocal;
  }


  public void Init(INClient networkClient, bool isLocalPlayer, string id, string playerHandle)
  {
    client = networkClient;
    isLocal = isLocalPlayer;
    matchId = id;
    
    if(!isLocal) nvp_NetworkManager_scr.OnDataReceived += OnMatchData;

    // if(!isLocal) client.OnMatchData = OnMatchData;
  }

  public float speed;
  public string matchId;
  public string playerHandle;

  public Vector3 networkPosition;

  // Use this for initialization
  void Start()
  {
    
  }

  // Update is called once per frame
  void Update()
  {
    if (isLocal)
    {
      // get input
      var input = Input.GetAxis("Vertical");

      // move
      transform.Translate(Vector3.up * Time.deltaTime * input * speed, Space.World);

      // push position
      List<float> data = new List<float>().AddVector(transform.position);
      var message = NMatchDataSendMessage.Default(matchId, 1, data.Serialize());
      client.Send(message, (bool done) => {Debug.Log("done");}, (INError error) => Debug.LogError(error.Message));
    }
    else{
      transform.position = networkPosition;
    }
  }

  void OnMatchData(INMatchData md){
    Debug.Log("received");

    if(md.OpCode == 1L){
      var list = md.Data.Deserialize();
      networkPosition = new Vector3(list[0], list[1], list[2]);
      
    }
  }
}

