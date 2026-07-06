using System.Collections.Generic;
using UnityEngine;
using Move.Character;

namespace Move.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private Dictionary<int, RemoteCharacterController> _remotePlayers = new Dictionary<int, RemoteCharacterController>();

        void Awake() => Instance = this;

        public void RegisterRemotePlayer(int id, RemoteCharacterController playerScript)
        {
            _remotePlayers[id] = playerScript;
        }

        //  自身以外のプレイヤーキャラを動かす
        public void UpdateRemotePlayerPosition(int id, Vector3 newPosition)
        {
            if (_remotePlayers.TryGetValue(id, out var remotePlayer))
            {
                remotePlayer.SetTargetPosition(newPosition);
            }
        }
    }
}