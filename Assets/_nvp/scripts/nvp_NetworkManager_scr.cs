using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Nakama;

public class nvp_NetworkManager_scr : MonoBehaviour
{

  // +++ fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  public INClient client;

  [Header("NAKAMA CLIENT SETTINGS")]
  public string url;
  public uint port;
  public bool ssl;
  public string serverKey;
  public string cancelTicket;
  public INMatchToken matchToken;
  public string matchId;
  public bool useUniqueId;
  public List<INUserPresence> connectedOpponents = new List<INUserPresence>();

  [Header("LOCAL PLAYER SETTINGS")]
  public string localPlayerId;
  public string localPlayerFullName;
  public string localPlayerHandle;

  [Header("CALLBACK SETTINGS")]
  public System.Action OnInitMatchComplete;

  public delegate void OnMatchDataReceived(INMatchData data);
  public static event OnMatchDataReceived OnDataReceived;

   



  // +++ Unity callbacks ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  void Awake()
  {
    client = InstantiateNetworkClient(serverKey, url, port, ssl);
  }

  void Start()
  {
    LoginWithDeviceId();
  }


  void Update()
  {

  }




  // +++ event handler ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  void OnMatchPresence(INMatchPresence presences)
  {
    Debug.Log("Match Players updated");

    // Remove all users who left.
    foreach (var user in presences.Leave)
    {
      connectedOpponents.Remove(
        connectedOpponents.SingleOrDefault(x=>x.Handle == user.Handle)
      );
    }

    // Add all users who joined.
    foreach (var user in presences.Join){
      if (connectedOpponents.Where(x=>x.Handle == user.Handle).Count() > 0){
        // already in list
        continue;
      }
      else connectedOpponents.Add(user);
    }

    // lock(connectedOpponents){
    //   LogOpponents(connectedOpponents, localPlayerId, "updated");
    // }
    
    if(connectedOpponents.Count == 2) OnInitMatchComplete();
    
  }
	
  void OnMatchJoined(INResultSet<INMatch> matchListWithOneEntry)
  {
    Debug.Log("Match Joined");
    matchId = matchListWithOneEntry.Results[0].Id;
    Debug.LogFormat("Join Match Id: {0}", matchId);

    // get connected users
    connectedOpponents = new List<INUserPresence>();
    // Add list of connected opponents.
    connectedOpponents.AddRange(matchListWithOneEntry.Results[0].Presence);

    client.OnMatchData = (md)=> {
      if(OnDataReceived != null) OnDataReceived(md);
    };

    // lock(connectedOpponents){
    //   LogOpponents(connectedOpponents, localPlayerId, "Joined");
    // }
  }

 

  void OnMatchmakeMatched(INMatchmakeMatched matchInfo)
  {
    // token to join match
    matchToken = matchInfo.Token;
    matchId = matchInfo.Token.Token;

    // and join the match found immediately
    JoinMatchByMatchToken(matchToken);

  }

  void OnMakeMatch(INMatchmakeTicket result)
  {
    Debug.Log("Added to Matchmaker pool");

    // store ticket to leave match maker pool
    cancelTicket = result.Ticket;
  }

  void OnFetchSelf(INSelf self)
  {
    localPlayerId = self.Id;
    localPlayerFullName = self.Fullname;
    localPlayerHandle = self.Handle;

    Debug.LogFormat("The user's ID is '{0}'.", self.Id);
    Debug.LogFormat("The user's fullname is '{0}'.", self.Fullname); // may be null
    Debug.LogFormat("The user's handle is '{0}'.", self.Handle);
  }

  void OnConnected(bool connected)
  {
    Debug.LogFormat("Connection with Server ({0})", connected);

    // get information about player
    FetchSelf();

    // make a match with matchmaker
    MakeMatch(2);
  }

  void OnServerSessionCreatedOrRestored(INSession session)
  {
    Debug.Log("Session created or restored");

    client.Connect(session, OnConnected);
  }

  void OnLoginError(INError error)
  {
    if (error.Code == ErrorCode.UserNotFound)
    {
      // if the user logs in for the first time
      RegisterWithDeviceId();
    }
    else
      OnError(error);
  }

  void OnError(INError error)
  {
    Debug.LogErrorFormat("Error!!! Code: {0} with Message: {1}", error.Code, error.Message);
  }




  // +++ custom methods +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  INClient InstantiateNetworkClient(string serverKey, string url, uint port = 7350, bool ssl = false)
  {
    var tempClient = new NClient
      .Builder(serverKey)
      .Host(url)
      .Port(port)
      .SSL(ssl)
      .Build();

    return tempClient;
  }

  void LoginWithDeviceId()
  {
    var message = NAuthenticateMessage.Device(this.GetId());
    client.Login(message, OnServerSessionCreatedOrRestored, OnLoginError);
  }

  void RegisterWithDeviceId()
  {
    var message = NAuthenticateMessage.Device(this.GetId());
    client.Register(message, OnServerSessionCreatedOrRestored, OnError);
  }

  void FetchSelf()
  {
    var message = NSelfFetchMessage.Default();
    client.Send(message, OnFetchSelf, OnError);
  }

  void MakeMatch(int numberOfPlayers)
  {
    var message = NMatchmakeAddMessage.Default(numberOfPlayers);

    // register for events from matchmaking
    client.OnMatchmakeMatched = OnMatchmakeMatched;

    // register for events belonging to users (join or leave)
    client.OnMatchPresence = OnMatchPresence;

    // send message to join the game
    client.Send(message, OnMakeMatch, OnError);
  }

  void JoinMatchByMatchToken(INMatchToken token)
  {
    var message = NMatchJoinMessage.Default(token);
    client.Send(message, OnMatchJoined, OnError);
  }

  private void LogOpponents(List<INUserPresence> opponents, string localId, string reason)
  {
    Debug.LogFormat("OPPONENT REPORT: ({0})", reason);
    Debug.Log("------------------------------------------------------");
    Debug.LogFormat("Opponents in game: {0}", opponents.Count);

    foreach (var presence in opponents)
    {
      var userId = presence.UserId;
      var handle = presence.Handle;

      Debug.LogFormat("LocalUser ({2}) - User id '{0}' handle '{1}'.", userId, handle, userId == localId);
    }
    Debug.Log("------------------------------------------------------");
  }




  // +++ helper +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  string GetId()
  {
    return useUniqueId
      ? GetDeviceId()
      : GetGuid();
  }

  string GetDeviceId()
  {
    string id = SystemInfo.deviceUniqueIdentifier;
    return id;
  }

  string GetGuid()
  {
    string id = System.Guid.NewGuid().ToString();
    return id;
  }


}
