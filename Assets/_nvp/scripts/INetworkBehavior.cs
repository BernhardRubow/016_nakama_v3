using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;

public interface INetworkBehavior {

	bool IsLocal();

	void Init(INClient networkClient, bool isLocalPlayer, string matchId, string playerHandle);
}
